var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nginx Manager v1")); 
app.MapControllers();
app.Run();

