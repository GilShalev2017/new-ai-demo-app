using Serilog;
using WhisperService.Controllers;
using WhisperService.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/whisper-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Whisper Service API",
        Version = "v1"
    });
});

//builder.Services.AddSingleton<WhisperController>();
builder.Services.AddSingleton<WhisperSvc>();
builder.Services.AddSingleton<TranscriptionStateService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WhisperService", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Whisper Service API v1");
        c.RoutePrefix = "swagger"; // default
    });
}

app.UseHttpsRedirection();

app.UseCors("WhisperService");

app.UseAuthorization();

app.MapControllers();

Log.Information("Whisper Service API starting...");

app.Run();
