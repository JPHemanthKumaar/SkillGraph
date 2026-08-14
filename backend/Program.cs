using SkillGraph.Api.Services;

// Load .env only outside Production (Render injects real env vars)
var contentRoot = Directory.GetCurrentDirectory();
var aspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
if (!aspEnv.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    EnvLoader.Load(
        Path.Combine(contentRoot, ".env"),
        Path.Combine(contentRoot, "..", ".env"),
        Path.Combine(contentRoot, "..", "..", ".env")
    );
}

var builder = WebApplication.CreateBuilder(args);

// Also allow appsettings.Local.json for secrets (gitignored)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SkillGraph API", Version = "v1" });
});

builder.Services.AddSingleton<IGraphService, GraphService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();

var wwwroot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (Directory.Exists(wwwroot))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
}

app.Run();
