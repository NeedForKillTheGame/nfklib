using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using demowebapi.Controllers;
using demowebapi.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        var json = options.JsonSerializerOptions;
        // The demoviewer relies on the exact member names of nfklib structures (PascalCase, public fields).
        json.PropertyNamingPolicy = null;
        json.IncludeFields = true;
        // Demo files may contain NaN/Infinity floats, write them as literals instead of throwing.
        json.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        // Keep cyrillic nicknames and chat as plain UTF-8 instead of \uXXXX escapes.
        json.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlDocs = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlDocs))
        options.IncludeXmlComments(xmlDocs);
});

// The parser is consumed from other origins (demo viewers), same as the old [Cors] attribute allowed.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Parsed demos are served pre-compressed from the cache; this covers everything else (the old api had a [Compress] filter).
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddHttpClient(DemoController.HttpClientName, client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.MaxResponseContentBufferSize = DemoController.MaxDemoSize;
    client.DefaultRequestHeaders.UserAgent.ParseAdd("nfk-demoparser/2.0 (+https://github.com/NeedForKillTheGame/nfklib)");
});

// Cache of parsed demos: memory (sized, short lived) + disk (sized), see appsettings.json / docker-compose.yml.
builder.Services.Configure<DemoCacheOptions>(builder.Configuration.GetSection(DemoCacheOptions.SectionName));
builder.Services.Configure<DemoSourceOptions>(builder.Configuration.GetSection(DemoSourceOptions.SectionName));
var cacheOptions = builder.Configuration.GetSection(DemoCacheOptions.SectionName).Get<DemoCacheOptions>() ?? new DemoCacheOptions();
builder.Services.AddMemoryCache(options =>
{
    if (cacheOptions.MemorySizeMB > 0)
        options.SizeLimit = cacheOptions.MemorySizeMB * 1024L * 1024;
});
builder.Services.AddSingleton<DemoCache>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseResponseCompression();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Relative path, so the UI also works behind a reverse proxy prefix (e.g. https://host/demoparser/).
    options.SwaggerEndpoint("swagger/v1/swagger.json", "NFK Demo Parser");
    options.RoutePrefix = string.Empty; // Swagger UI at the application root
});

app.MapControllers();

app.Run();
