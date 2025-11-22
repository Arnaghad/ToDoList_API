using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataLayer.Entities;
using DataLayer.Contexts;
using DataLayer.Extensions;
using BusinessLogic.Extensions;
using Microsoft.IdentityModel.Tokens;
using WebApi.Filters;
using WebApi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- РЕЄСТРАЦІЯ СЕРВІСІВ ---

// Додаємо DataLayer (DbContext, Repositories, UnitOfWork)
builder.Services.AddDataLayer(builder.Configuration);

// Додаємо BusinessLogic (Services)
builder.Services.AddBusinessLogic();

// Додаємо AutoMapper
builder.Services.AddAutoMapperProfiles();

// Додаємо FluentValidation
builder.Services.AddFluentValidators();

// Реєструємо Transaction Filter та Validation Filter
builder.Services.AddScoped<TransactionActionFilter>();
builder.Services.AddScoped<ValidationActionFilter>();

// Додаємо Controllers з фільтрами
builder.Services.AddControllers(options =>
{
    // Додаємо глобальні фільтри
    options.Filters.Add<ValidationActionFilter>();  // Валідація
    options.Filters.Add<TransactionActionFilter>(); // Транзакції
});

// Додаємо CORS (опціонально, для frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Налаштування Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "ToDo API",
        Description = "ToDo API for ToDo application with full CRUD operations"
    });

    // Визначаємо схему безпеки
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    // Налаштування вимог безпеки для .NET 10 / Swashbuckle v10
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>() // ВИПРАВЛЕНО: Використовуємо List<string> замість Array.Empty<string>()
        }
    });
});

// Налаштування Identity та авторизації
builder.Services.AddAuthorization();

builder.Services.AddIdentity<AuthUser, IdentityRole>(options => 
    {
        // Налаштування пароля (для спрощення тестів)
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

// Налаштування Authentication та JWT
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddSingleton<IEmailSender<AuthUser>, DummyEmailSender>();

var app = builder.Build();

// --- НАЛАШТУВАННЯ HTTP PIPELINE ---

// Застосовуємо міграції до бази даних при старті (асинхронно)
await ApplyMigrationsAsync(app.Services);

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await dbContext.Database.MigrateAsync();
        
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

// Налаштування Swagger (доступний в Development та Production для тестування)
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "ToDo API Documentation";
    });
}

app.UseHttpsRedirection();

// Увімкнення CORS
app.UseCors("AllowAll");

// Важливо: UseAuthentication має бути перед UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// Реєструємо Controllers
app.MapControllers();

// Додаємо простий health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
}))
.WithName("HealthCheck")
.WithTags("Health")
.AllowAnonymous();

// Додаємо інформаційний endpoint про API
app.MapGet("/", () => Results.Ok(new
{
    Name = "ToDo API",
    Version = "v1.0.0",
    Documentation = "/swagger",
    Endpoints = new
    {
        Health = "/health",
        Auth = "/register, /login",
        Items = "/api/items",
        Categories = "/api/categories"
    }
}))
.WithName("ApiInfo")
.WithTags("Info")
.AllowAnonymous();

// Логування інформації про старт
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting ToDo API...");
logger.LogInformation("Swagger UI available at: /swagger");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();