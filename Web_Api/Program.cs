using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataLayer.Entities;
using DataLayer.Contexts;
using DataLayer.Extensions;
using BusinessLogic.Extensions;
using WebApi.Filters;
using WebApi.Middleware;
using WebApi.Extensions;

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
    options.SwaggerDoc("v1", new()
    {
        Version = "v1.0.0",
        Title = "ToDo API",
        Description = "ToDo API for ToDo application with full CRUD operations"
    });

    // Додаємо підтримку авторизації в Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Налаштування Identity та авторизації
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<AuthUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DatabaseContext>();

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

// Реєструємо Identity API endpoints
app.MapIdentityApi<AuthUser>();

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