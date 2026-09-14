using SmartX.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1. Register Services
// ---------------------------------------------------------
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Our custom telemetry service — Singleton so the in-memory
// store is shared across all incoming requests.
builder.Services.AddSingleton<TelemetryService>();

// ---------------------------------------------------------
// 2. CORS — allow the WPF client to call this API
// ---------------------------------------------------------
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

// ---------------------------------------------------------
// 3. Middleware Pipeline
// ---------------------------------------------------------
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

// ---------------------------------------------------------
// 4. Startup Banner
// ---------------------------------------------------------
Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║           Smart-X Gateway Started Successfully           ║");
Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
Console.WriteLine("║  API Endpoints:                                          ║");
Console.WriteLine("║    POST   /api/telemetry            - Send telemetry     ║");
Console.WriteLine("║    GET    /api/telemetry            - Get all data       ║");
Console.WriteLine("║    GET    /api/telemetry/latest     - Latest per device  ║");
Console.WriteLine("║    GET    /api/telemetry/statistics - Dashboard stats    ║");
Console.WriteLine("║    DELETE /api/telemetry/clear      - Clear all data     ║");
Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
Console.WriteLine("║  Swagger UI:  http://localhost:5000/swagger              ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

app.Run();