using SmartX.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddSingleton<SmartXDataStructures>();
builder.Services.AddSingleton<PredictiveActionEngine>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart-X IoT Gateway API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Smart-X IoT Gateway API";
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();


Console.WriteLine("╔═════════════════════════════════════════════════════════╗");
Console.WriteLine("║           Smart-X Gateway Started Successfully           ║");
Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
Console.WriteLine("║  API Endpoints:                                          ║");
Console.WriteLine("║    POST   /api/telemetry            - Send telemetry     ║");
Console.WriteLine("║    GET    /api/telemetry            - Get all data       ║");
Console.WriteLine("║    GET    /api/telemetry/latest     - Latest per device  ║");
Console.WriteLine("║    GET    /api/telemetry/statistics - Dashboard stats    ║");
Console.WriteLine("║    DELETE /api/telemetry/clear      - Clear all data     ║");
Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
Console.WriteLine("║  Swagger UI:  http://localhost:5044/swagger              ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

app.Run();