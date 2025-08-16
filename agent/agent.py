#!/usr/bin/env python3
"""
Simple agent that pings the backend every N seconds with host info.

Configuration via environment variables:
- AGENT_BASE_URL: Base URL of the backend (default: http://localhost:5083)
- AGENT_PING_URL: Full URL to ping endpoint; if set, overrides BASE_URL + path
- AGENT_PING_PATH: Path to ping endpoint (default: /api/host/ping)
- AGENT_PING_INTERVAL: Interval in seconds between pings (default: 5)
- AGENT_INSECURE_SSL: If "true", disable TLS certificate verification for HTTPS

The backend expects JSON body:
{
  "hostname": "<host>",
  "ip": "<ip>"
}
"""

import json
import os
import socket
import ssl
import sys
import time
import urllib.request
import urllib.error
from typing import Optional


def get_hostname() -> str:
    try:
        return socket.gethostname()
    except Exception:
        return "unknown"


def get_ip_address() -> str:
    """Best-effort to fetch a non-loopback IP for the host."""
    # Try outbound route discovery via UDP socket (no packets sent)
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
            s.connect(("8.8.8.8", 80))
            ip = s.getsockname()[0]
            if ip and not ip.startswith("127."):
                return ip
    except Exception:
        pass

    # Fallback: resolve hostname
    try:
        ip = socket.gethostbyname(socket.gethostname())
        if ip:
            return ip
    except Exception:
        pass

    return "127.0.0.1"


def bool_from_env(name: str, default: bool = False) -> bool:
    val = os.getenv(name)
    if val is None:
        return default
    return val.strip().lower() in {"1", "true", "yes", "on"}


def build_ping_url() -> str:
    # Full URL override
    full = os.getenv("AGENT_PING_URL")
    if full:
        return full

    base = os.getenv("AGENT_BASE_URL", "http://localhost:5083").rstrip("/")
    path = os.getenv("AGENT_PING_PATH", "/api/Host/ping")
    if not path.startswith("/"):
        path = "/" + path
    return f"{base}{path}"


def make_ssl_context_if_needed() -> Optional[ssl.SSLContext]:
    if bool_from_env("AGENT_INSECURE_SSL", False):
        ctx = ssl.create_default_context()
        ctx.check_hostname = False
        ctx.verify_mode = ssl.CERT_NONE
        return ctx
    return None


def post_json(url: str, payload: dict, timeout: float = 5.0, context: Optional[ssl.SSLContext] = None) -> int:
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        url=url,
        data=data,
        headers={"Content-Type": "application/json", "Accept": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout, context=context) as resp:
            return resp.getcode() or 0
    except urllib.error.HTTPError as e:
        # Server responded with error code
        return e.code
    except Exception as e:
        # Network or other errors
        raise e


def main() -> int:
    url = build_ping_url()
    interval_env = os.getenv("AGENT_PING_INTERVAL", "5").strip()
    try:
        interval = max(1.0, float(interval_env))
    except ValueError:
        interval = 5.0

    ssl_ctx = make_ssl_context_if_needed()

    hostname = get_hostname()
    last_ip = None

    print(f"Agent starting. Pinging {url} every {interval} seconds.")
    if ssl_ctx:
        print("WARNING: TLS certificate verification is DISABLED (AGENT_INSECURE_SSL=true)")

    while True:
        try:
            ip = get_ip_address()
            # Only log IP changes to reduce noise
            if ip != last_ip:
                print(f"Using IP: {ip}")
                last_ip = ip

            payload = {"hostname": hostname, "ip": ip}
            status = post_json(url, payload, timeout=5.0, context=ssl_ctx)

            if 200 <= status < 300 or status == 204:
                # Keep output minimal on success
                pass
            else:
                print(f"Ping failed with HTTP {status}")
        except KeyboardInterrupt:
            print("\nAgent stopped by user.")
            return 0
        except Exception as e:
            print(f"Ping error: {e}")

        time.sleep(interval)


if __name__ == "__main__":
    sys.exit(main())