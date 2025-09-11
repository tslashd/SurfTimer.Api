using System.Reflection;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using SurfTimer.Api.Middleware;
using SurfTimer.Shared.Data;
using SurfTimer.Shared.Data.MySql;
using SurfTimer.Shared.JsonConverters;

var assembly = Assembly.GetExecutingAssembly();
var title = assembly.GetName().Name;
var version = assembly.GetName().Version?.ToString() ?? "6.6.6";

// Environment Variables
var dbHost =
    Environment.GetEnvironmentVariable("DB_HOST")
    ?? throw new InvalidOperationException("DB_HOST is not set.");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbUser =
    Environment.GetEnvironmentVariable("DB_USER")
    ?? throw new InvalidOperationException("DB_USER is not set.");
var dbPassword =
    Environment.GetEnvironmentVariable("DB_PASSWORD")
    ?? throw new InvalidOperationException("DB_PASSWORD is not set.");
var dbName =
    Environment.GetEnvironmentVariable("DB_NAME")
    ?? throw new InvalidOperationException("DB_NAME is not set.");
var connectionString =
    $"Server={dbHost};Port={dbPort};User={dbUser};Password={dbPassword};Database={dbName};Allow User Variables=true;";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMySqlDataSource(connectionString);

// IDbConnectionFactory -> using the already registered MySqlDataSource
builder.Services.AddSingleton<IDbConnectionFactory>(sp =>
{
    var ds = sp.GetRequiredService<MySqlDataSource>();
    return new MySqlDataSourceConnectionFactory(ds);
});

// User the SurfTimer.Shared Dapper integration
builder.Services.AddScoped<SurfTimer.Shared.Data.IDatabaseService, DapperDatabaseService>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = title,
            Version = version,
            Description = "by [tslashd](https://github.com/tslashd)",
        }
    );
});
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new ReplayFramesStringConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dapper bootstrap (snake_case mapping + type handlers)
DapperBootstrapper.Init();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestTimingMiddleware>();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
