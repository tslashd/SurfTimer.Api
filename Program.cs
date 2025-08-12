using Dapper;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using SurfTimer.Api.Data;
using SurfTimer.Api.Middleware;
using SurfTimer.Api.Shared.TypeHandlers;
using SurfTimer.Shared.JsonConverters;
using System.Reflection;


// Auth0
/*
var auth0Domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN") ?? throw new InvalidOperationException("AUTH0_DOMAIN is not set.");
var auth0Audience = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE") ?? throw new InvalidOperationException("AUTH0_AUDIENCE is not set.");
var auth0Enable = Environment.GetEnvironmentVariable("AUTH0_ENABLE") ?? throw new InvalidOperationException("AUTH0_ENABLE is not set.");

Console.WriteLine($"auth0Enable = {auth0Enable}");

if (auth0Enable == "1")
{
    Console.WriteLine("Auth0 authentication is enabled.");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {

        options.Authority = $"https://{auth0Domain}/";
        options.Audience = auth0Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });
    builder.Services
      .AddAuthorization(options =>
      {
          options.AddPolicy(
            "read:messages",
            policy => policy.Requirements.Add(
              new HasScopeRequirement("read:messages", auth0Domain)
            )
          );
      });
    builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
}
else
{
    Console.WriteLine("Auth0 authentication is disabled.");
    builder.Services.AddAuthorization(options =>
    {
        // Policy to always allow access
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true)
            .Build();
    });
}

    // Bearer schema (add only if Auth0 is enabled)
    if (auth0Enable == "1")
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Enter JWT token. Example: Bearer {your token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.OperationFilter<AuthorizeCheckOperationFilter>();
    }

if (auth0Enable == "1")
{
    app.UseAuthentication();
    Console.WriteLine("Auth0 authentication is enabled.");
}

*/

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
SqlMapper.AddTypeHandler(new ReplayFramesStringHandler());

var assembly = Assembly.GetExecutingAssembly();
var title = assembly.GetName().Name;
var version = assembly.GetName().Version?.ToString() ?? "6.6.6";

// Environment Variables
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new InvalidOperationException("DB_HOST is not set.");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new InvalidOperationException("DB_USER is not set.");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new InvalidOperationException("DB_PASSWORD is not set.");
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new InvalidOperationException("DB_NAME is not set.");
var connectionString = $"Server={dbHost};Port={dbPort};User={dbUser};Password={dbPassword};Database={dbName};Allow User Variables=true;";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMySqlDataSource(connectionString);
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = title,
        Version = version,
        Description = "by [tslashd](https://github.com/tslashd)",
    });
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new ReplayFramesStringConverter());
});



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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