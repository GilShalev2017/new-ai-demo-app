using MongoDB.Driver;
using Serilog;
using Server;
using Server.InsightProviders;
using Server.Repositories;
using Server.Services;
using Server.Settings;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ai-demo-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

MongoMappings.Register();

// Add services to the container.
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                 {
                     options.JsonSerializerOptions.Converters.Add(
                         new JsonStringEnumConverter());
                 });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "AI Demo Server API",
        Version = "v1"
    });
});

// Configure MongoDB Settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return new MongoClient(settings?.ConnectionString);
});

builder.Services.AddSingleton(serviceProvider =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings?.DatabaseName);
});

builder.Services.AddScoped<IVectorDBRepository, VectorDBRepository>();
builder.Services.AddSingleton<IVideoUtilityService, VideoUtilityService>();
builder.Services.AddSingleton<IClipRepository, ClipRepository>();
builder.Services.AddSingleton<IClipRequestRepository, ClipRequestRepository>();
builder.Services.AddScoped<IInsightDefinitionRepository, InsightDefinitionRepository>();
builder.Services.AddScoped<IClipService, ClipService>();
builder.Services.AddSingleton<DataSeederService>();
builder.Services.AddScoped<ProviderBase, WhisperTranscriberProvider>();
builder.Services.AddScoped<ProviderBase, OpenAIChatProvider>();
builder.Services.AddScoped<IAIProviderService, AIProviderService>();

builder.Services.AddScoped<IInsightInputBuilder, TranscriptionInputBuilder>();
builder.Services.AddScoped<IInsightInputBuilder, TranscriptionDependentInputBuilder>();

builder.Services.AddHttpClient<IEmbeddingProvider, OpenAiEmbeddingProvider>();
builder.Services.AddScoped<IEntityExtractor, EntityExtractor>();
builder.Services.AddScoped<IPromptComposer, PromptComposer>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Demo Server API v1");
        c.RoutePrefix = "swagger"; // default
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    await seeder.SeedClipsAsync();
}

Log.Information("AI Demo Server API starting...");

DotNetEnv.Env.Load();

app.Run();
