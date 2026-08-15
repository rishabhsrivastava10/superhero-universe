using System.Text;
using CommonAPI.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using WebAPISuperheroUniverse.API.Class;
using WebAPISuperheroUniverse.API.Middleware;
using WebAPISuperheroUniverse.API.Services;
using WebAPISuperheroUniverse.DBContext.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------------------------------------
// Authentication / authorization
// ---------------------------------------------------------------------------------------------
builder.Services.Configure<ClsJwtSettings>(builder.Configuration.GetSection(ClsJwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(ClsJwtSettings.SectionName).Get<ClsJwtSettings>()
                  ?? throw new InvalidOperationException($"Missing '{ClsJwtSettings.SectionName}' configuration section.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
{
    // Fail fast at startup rather than issuing tokens signed with a weak or absent key.
    throw new InvalidOperationException(
        "Jwt:Key must be configured and at least 32 characters. Set it via user secrets or the JWT__KEY environment variable.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ValidateLifetime = true,
        // Default is 5 minutes of leeway, which would let an "expired" token keep working.
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IPasswordHasher, ClsPasswordHasher>();
builder.Services.AddSingleton<ITokenGenerator, ClsTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISuperheroService, SuperheroService>();

builder.Services.AddValidatorsFromAssemblyContaining<ClsRegisterRequestValidator>();

builder.Services.AddControllers(options => options.Filters.Add<ClsValidationFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Superhero Universe API",
        Version = "v1",
        Description = "REST API for the Superhero Universe Management System - superheroes, powers, teams, missions and battles."
    });

    // Adds the "Authorize" button in Swagger UI so protected endpoints can be tried out.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste only the JWT access token - Swagger adds the \"Bearer \" prefix itself."
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Superhero Universe API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AngularClient");

// Authentication must run before authorization - you cannot check what someone may do
// until you know who they are.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
