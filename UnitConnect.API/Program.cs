using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UnitConnect.API.Middleware;
using UnitConnect.Application;
using UnitConnect.Infrastructure;
using UnitConnect.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Application + Infrastructure DI ──────────────────────────────────────────
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

// ── MVC + OpenAPI / Swagger ───────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

app.MapOpenApi();           // serves /openapi/v1.json
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "UnitConnect API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
