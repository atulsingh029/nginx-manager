using Microsoft.EntityFrameworkCore;
using nginx_manager.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Configure EF Core with MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("Default") ?? string.Empty;
var serverVersionString = builder.Configuration.GetValue<string>("Database:ServerVersion") ?? "8.0.36-mysql";
var serverVersion = ServerVersion.Parse(serverVersionString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion,
        mySqlOptions => mySqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
);

var app = builder.Build();

// Apply any pending migrations automatically at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nginx Manager v1"));
app.MapControllers();
app.Run();

