using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using SI24004.Models.PostgreSQL;
using SI24004.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http.Features;
using OfficeOpenXml;
using Microsoft.Extensions.Options;
using SI24004.Models.DTOs;
using System.Reflection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SI24004.Repositories;
using SI24004.Repositories.Interfaces;
using SI24004.Services.Interfaces;
using SI24004.Services;
using SI24004.Models.MySQL;
using SI24004.Models.SqlServer1;
using SI24004.Models.SqlServer;
using SI24004.Models.Special;
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// IIS Configuration - ??????????? Background Services
builder.Services.Configure<IISOptions>(options =>
{
    options.AutomaticAuthentication = false;
    options.ForwardClientCertificate = false;
});

// ?????????????????????? IIS Integration ??? Background Services
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// JWT Secret Key Configuration
var jwtSecret = config["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret Key is not configured.");
}

var key = Encoding.UTF8.GetBytes(jwtSecret);

// DbContext registrations
builder.Services.AddDbContext<PostgrestContext>(options =>
    options.UseNpgsql(config.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(300);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    })
);
builder.Services.AddDbContext<SpecialContext>(options =>
    options.UseMySql(config.GetConnectionString("WhServer"),
        ServerVersion.AutoDetect(config.GetConnectionString("WhServer")),
        mySqlOptions =>
        {
            mySqlOptions.CommandTimeout(300);
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null); // ? MySQL (Pomelo) ??? errorNumbersToAdd
        })
);
builder.Services.AddDbContext<SI24004.Models.MySQL.SqlServerContext>(options =>
    options.UseMySql(config.GetConnectionString("WhServer"),
        ServerVersion.AutoDetect(config.GetConnectionString("WhServer")),
        mySqlOptions =>
        {
            mySqlOptions.CommandTimeout(300);
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null); // ? MySQL (Pomelo) ??? errorNumbersToAdd
        })
);

builder.Services.AddDbContext<ThicknessContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
    {
        sqlOptions.CommandTimeout(300);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));
builder.Services.AddDbContext<SI24004.Models.SqlServer1.OutputContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer1"));
    // ????????? migration ????? database ??????????
}, ServiceLifetime.Scoped);

// Configure SMTP and Email settings
builder.Services.Configure<SmtpSettings>(options =>
{
    var smtpSection = builder.Configuration.GetSection("SmtpSettings");
    options.Host = smtpSection["Host"] ?? "agcmgw.agc.jp";
    options.Port = int.Parse(smtpSection["Port"] ?? "25");
    options.Username = smtpSection["Username"] ?? "noreply@agc.com";
    options.Password = smtpSection["Password"] ?? "";
    options.UseDefaultCredentials = bool.Parse(smtpSection["UseDefaultCredentials"] ?? "true");
    options.EnableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "false");
    options.FromEmail = smtpSection["FromEmail"] ?? "noreply@agc.com";
    options.FromName = smtpSection["FromName"] ?? "Polishing Alert System";
});


builder.Services.Configure<SI24004.Models.DTOs.EmailRecipients>(options =>
{
    var emailSection = builder.Configuration.GetSection("EmailRecipients");

    // default recipients
    options.To = emailSection.GetSection("To").Get<List<string>>() ?? new List<string>
    {
        "anupong.ohok@agc.com",
        "Supattra.Khonraeng@agc.com",
        "Rathapong.Wongsettee@agc.com",
        "Bandith.Srimai@agc.com",
        "Kasiwat.Rattanamakhin@agc.com",
        "Ongart.Jaitamdee@agc.com",
        "Pongsak.Jeepookham@agc.com",
        "Pradongpong.Papuan@agc.com",
        "Sirinan.Wonglangka@agc.com",
        "Sombat.Phansiri@agc.com",
        "Supattra.Khonraeng@agc.com",
        "Tanakrit.Wongwansroi@agc.com",
        "Wilawan.Titta@agc.com",
        "Worapoch.Muakthongkam@agc.com",
        "Pongwid.Thiamsan@agc.com",
    };

    options.Cc = emailSection.GetSection("Cc").Get<List<string>>() ?? new List<string>();
    options.Bcc = emailSection.GetSection("Bcc").Get<List<string>>() ?? new List<string>();
});

// Email service registration
builder.Services.AddSingleton<SI24004.Services.Interfaces.IEmailRecipientsService, SI24004.Services.EmailRecipientsService>();

// Bind ScheduleSettings from appsettings.json
builder.Services.Configure<SI24004.Models.DTOs.ScheduleSettings>(
    builder.Configuration.GetSection("ScheduleSettings"));

// Register EmailScheduleManager as singleton (accessible via DI)
builder.Services.AddSingleton<SI24004.Services.EmailScheduleManager>();
builder.Services.AddSingleton<SI24004.Services.Interfaces.IEmailScheduleManager>(provider =>
    provider.GetRequiredService<SI24004.Services.EmailScheduleManager>());

// Register IISCompatibleEmailService as HostedService so StartAsync() is called
builder.Services.AddSingleton<SI24004.Services.IISCompatibleEmailService>();
builder.Services.AddHostedService<SI24004.Services.IISCompatibleEmailService>(provider =>
    provider.GetRequiredService<SI24004.Services.IISCompatibleEmailService>());

// Keep-alive service (?????? email service)
builder.Services.AddSingleton<SI24004.Services.IISKeepAliveService>();
builder.Services.AddHostedService<SI24004.Services.IISKeepAliveService>(provider =>
    provider.GetRequiredService<SI24004.Services.IISKeepAliveService>());

// Other services
builder.Services.AddScoped<SI24004AVIService>();
builder.Services.AddScoped<SI25007Service>();

builder.Services.AddScoped<IChemicalSearchRepository, ChemicalSearchRepository>();
builder.Services.AddScoped<IChemicalSearchService, SI24004.Services.ChemicalSearchService>();
builder.Services.AddScoped<IRegularSubstanceService, RegularSubstanceService>();
builder.Services.AddScoped<ISvhcSubstanceService, SvhcSubstanceService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500_000_000; // 500MB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
    options.BufferBody = true;
    options.MemoryBufferThreshold = int.MaxValue;
    options.BufferBodyLengthLimit = long.MaxValue;
});

// Kestrel Server Options
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 500_000_000; // 500MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        // Configure HTTPS binding if needed
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS Setup
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://172.18.106.100:9014",
                "http://172.18.106.100:9011",
                "http://172.18.106.100:9020",
                "http://localhost:3000",
                "http://localhost:9014",
                "http://localhost:9011",
                "http://localhost"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// JSON Handling
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddFilter("Microsoft", LogLevel.Warning);
    logging.AddFilter("System", LogLevel.Warning);
    logging.AddFilter("SI24004.Services", LogLevel.Information);
});

builder.Services.AddEndpointsApiExplorer();

// Swagger Setup
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SI24004 API with PDF Comparison",
        Version = "v1",
        Description = "API including PDF vs Excel comparison functionality. Use Bearer token for authentication."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and the JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    options.OperationFilter<FileUploadOperationFilter>();
});

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// ========================================
// ?? FIX: PostgreSQL DateTime Timezone Error
// ========================================
// ????? error: "Cannot write DateTime with Kind=UTC to PostgreSQL type 'timestamp without time zone'"
// ?????????????? DateTime.UtcNow ?????????????? PostgreSQL ??????????? error
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// ========================================

var app = builder.Build();

// ? FIXED: ????? endpoint ????? debug services
app.MapGet("/debug-services", (IServiceProvider serviceProvider, ILogger<Program> logger) =>
{
    try
    {
        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();
        var emailServices = hostedServices.Where(s =>
            s.GetType().Name.Contains("Email") ||
            s.GetType().Name.Contains("Schedule")).ToList();

        logger.LogInformation("?? Total Hosted Services: {Count}", hostedServices.Count);
        logger.LogInformation("?? Email-related Services: {Count}", emailServices.Count);

        foreach (var service in emailServices)
        {
            logger.LogInformation("   - {ServiceType}", service.GetType().FullName);
        }

        return Results.Ok(new
        {
            TotalHostedServices = hostedServices.Count,
            EmailServicesCount = emailServices.Count,
            EmailServices = emailServices.Select(s => new {
                TypeName = s.GetType().Name,
                FullName = s.GetType().FullName
            }).ToList(),
            AllServices = hostedServices.Select(s => s.GetType().Name).ToList(),
            Time = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "?? Error in debug-services");
        return Results.Problem($"Debug failed: {ex.Message}");
    }
});

// Manual email trigger endpoint
app.MapPost("/trigger-email", async (IEmailScheduleManager emailManager, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("?? Manual email trigger requested");

        if (emailManager.IsRunning)
        {
            return Results.BadRequest(new
            {
                Status = "Error",
                Message = "Email task is already running",
                Time = DateTime.Now
            });
        }

        await emailManager.ExecuteOnDemandAsync();

        return Results.Ok(new
        {
            Status = "Success",
            Message = "Email task executed successfully",
            Time = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "?? Manual email trigger failed");
        return Results.Problem($"Failed to trigger email: {ex.Message}");
    }
});

// Debug SMTP endpoint - ดู settings จริงที่โหลดอยู่
app.MapGet("/debug-smtp", (IOptions<SmtpSettings> smtpOptions, IOptions<EmailRecipients> recipientsOptions) =>
{
    var smtp = smtpOptions.Value;
    var recipients = recipientsOptions.Value;
    return Results.Ok(new
    {
        SmtpHost     = smtp.Host,
        SmtpPort     = smtp.Port,
        FromEmail    = smtp.FromEmail,
        FromName     = smtp.FromName,
        EnableSsl    = smtp.EnableSsl,
        UseDefaultCredentials = smtp.UseDefaultCredentials,
        HasUsername  = !string.IsNullOrEmpty(smtp.Username),
        HasPassword  = !string.IsNullOrEmpty(smtp.Password),
        ToCount      = recipients?.To?.Count ?? 0,
        ToList       = recipients?.To ?? new List<string>(),
        CcCount      = recipients?.Cc?.Count ?? 0,
        BccCount     = recipients?.Bcc?.Count ?? 0,
    });
});

// Test SMTP connection endpoint
app.MapPost("/test-smtp-connection", async (IOptions<SmtpSettings> smtpOptions, ILogger<Program> logger) =>
{
    var smtp = smtpOptions.Value;
    try
    {
        using var client = new System.Net.Mail.SmtpClient(smtp.Host, smtp.Port);
        client.EnableSsl = smtp.EnableSsl;
        client.UseDefaultCredentials = smtp.UseDefaultCredentials;
        client.Timeout = 10000;

        var msg = new System.Net.Mail.MailMessage();
        msg.From = new System.Net.Mail.MailAddress(smtp.FromEmail, smtp.FromName);
        msg.To.Add("anupong.ohok@agc.com");
        msg.Subject = "SMTP Test - SI24004";
        msg.Body = $"SMTP connection test from SI24004 at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        msg.IsBodyHtml = false;

        await client.SendMailAsync(msg);

        return Results.Ok(new { Status = "Success", Message = "Test email sent to anupong.ohok@agc.com", Smtp = smtp.Host, Port = smtp.Port });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "SMTP test failed");
        return Results.Problem($"SMTP failed: {ex.Message} | InnerException: {ex.InnerException?.Message}");
    }
});


app.MapGet("/warmup", (IEmailScheduleManager emailManager, IServiceProvider serviceProvider, ILogger<Program> logger) =>
{
    try
    {
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        var emailServiceCount = hostedServices.Count(s => s.GetType().Name.Contains("Email"));
        var emailStatus = emailManager.GetStatus();

        logger.LogInformation("?? Warmup requested - EmailServices: {Count}", emailServiceCount);

        return Results.Ok(new
        {
            Status = "Warm",
            Time = DateTime.Now,
            EmailServicesRunning = emailServiceCount > 0,
            TotalHostedServices = hostedServices.Count(),
            EmailServicesCount = emailServiceCount,
            EmailServiceStatus = emailStatus
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "?? Warmup failed");
        return Results.Problem($"Warmup failed: {ex.Message}");
    }
});

app.MapGet("/keep-alive", () => Results.Ok(new
{
    Status = "Alive",
    Time = DateTime.Now
}));

// Updated email service status endpoint
app.MapGet("/email-service-status", (IEmailScheduleManager emailManager, IServiceProvider serviceProvider, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("?? Email service status requested");

        var status = emailManager.GetStatus();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        var emailHostedServices = hostedServices.Where(s => s.GetType().Name.Contains("Email")).ToList();

        return Results.Ok(new
        {
            EmailManagerStatus = status,
            HostedEmailServices = emailHostedServices.Count,
            ServiceType = "IEmailScheduleManager",
            EmailServiceNames = emailHostedServices.Select(s => s.GetType().Name).ToList(),
            Time = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "?? Error getting service status");
        return Results.Problem($"Error getting service status: {ex.Message}");
    }
});

// Enhanced test email endpoint
app.MapPost("/test-email", async (IEmailScheduleManager emailManager, IServiceProvider serviceProvider, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("?? Manual test email requested");

        await emailManager.SendTestEmailAsync();

        return Results.Ok(new
        {
            Status = "Success",
            Message = "Test email sent successfully via EmailScheduleManager",
            Time = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "?? Failed to send test email: {Message}", ex.Message);
        return Results.Problem($"Failed to send test email: {ex.Message}");
    }
});

// SMTP connection test endpoint
app.MapPost("/test-smtp", async (IServiceProvider serviceProvider, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("?? Testing SMTP connection");

        using var scope = serviceProvider.CreateScope();
        var smtpSettings = scope.ServiceProvider.GetRequiredService<IOptions<SmtpSettings>>().Value;

        using var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port);
        smtpClient.EnableSsl = smtpSettings.EnableSsl;
        smtpClient.UseDefaultCredentials = smtpSettings.UseDefaultCredentials;
        smtpClient.Timeout = 10000; // 10 seconds timeout

        if (!smtpSettings.UseDefaultCredentials)
        {
            smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);
        }

        using var message = new MailMessage();
        message.From = new MailAddress(smtpSettings.FromEmail, smtpSettings.FromName);
        message.To.Add("anupong.ohok@agc.com");
        message.Subject = "SMTP Connection Test";
        message.Body = $"SMTP connection test successful at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        message.IsBodyHtml = false;

        await smtpClient.SendMailAsync(message);

        logger.LogInformation("? SMTP test successful");

        return Results.Ok(new
        {
            Status = "Success",
            Message = "SMTP connection test passed",
            SmtpHost = smtpSettings.Host,
            SmtpPort = smtpSettings.Port,
            EnableSsl = smtpSettings.EnableSsl,
            UseDefaultCredentials = smtpSettings.UseDefaultCredentials,
            Time = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "?? SMTP test failed: {Message}", ex.Message);
        return Results.Problem($"SMTP test failed: {ex.Message}");
    }
});

// Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// CORS before Authentication
app.UseCors("AllowFrontend");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// ? FIXED: ???? email configuration ????? service count
try
{
    var smtpOptions = app.Services.GetRequiredService<IOptions<SmtpSettings>>();
    var scheduleOptions = app.Services.GetRequiredService<IOptions<ScheduleSettings>>();
    var hostedServices = app.Services.GetServices<IHostedService>();
    var emailServices = hostedServices.Where(s => s.GetType().Name.Contains("Email")).ToList();

    foreach (var emailService in emailServices)
    {
        logger.LogInformation("   - {ServiceName}", emailService.GetType().Name);
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "?? Error displaying configuration");
}

app.Run();

// FileUploadOperationFilter remains the same
public class FileUploadOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var parameters = context.MethodInfo.GetParameters();
        var hasFileParameter = false;
        var allProperties = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSchema>();
        var parametersToRemove = new List<string>();

        foreach (var parameter in parameters)
        {
            // ??????? IFormFile ?????? (?????????? [FromForm] ???????)
            if (parameter.ParameterType == typeof(IFormFile))
            {
                hasFileParameter = true;
                parametersToRemove.Add(parameter.Name);
                allProperties[parameter.Name] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = $"Upload file for {parameter.Name}"
                };
            }
            // ??????? IFormFileCollection
            else if (parameter.ParameterType == typeof(IFormFileCollection))
            {
                hasFileParameter = true;
                parametersToRemove.Add(parameter.Name);
                allProperties[parameter.Name] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "array",
                    Items = new Microsoft.OpenApi.Models.OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
            // ??????? Model class ????? IFormFile properties
            else if (IsComplexType(parameter.ParameterType))
            {
                var hasFromFormAttribute = HasFromFormAttribute(parameter);
                if (hasFromFormAttribute)
                {
                    parametersToRemove.Add(parameter.Name);
                    var properties = parameter.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var prop in properties)
                    {
                        if (prop.PropertyType == typeof(IFormFile))
                        {
                            hasFileParameter = true;
                            allProperties[prop.Name] = new Microsoft.OpenApi.Models.OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary",
                                Description = $"Upload file for {prop.Name}"
                            };
                        }
                        else if (prop.PropertyType == typeof(IFormFileCollection))
                        {
                            hasFileParameter = true;
                            allProperties[prop.Name] = new Microsoft.OpenApi.Models.OpenApiSchema
                            {
                                Type = "array",
                                Items = new Microsoft.OpenApi.Models.OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            };
                        }
                        else if (IsSimpleType(prop.PropertyType))
                        {
                            allProperties[prop.Name] = GetSchemaForType(prop.PropertyType);
                        }
                    }
                }
            }
            // Handle other [FromForm] parameters (????????? IFormFile)
            else if (HasFromFormAttribute(parameter) && IsSimpleType(parameter.ParameterType))
            {
                parametersToRemove.Add(parameter.Name);
                allProperties[parameter.Name] = GetSchemaForType(parameter.ParameterType);
            }
        }

        if (hasFileParameter)
        {
            operation.RequestBody = new Microsoft.OpenApi.Models.OpenApiRequestBody
            {
                Content = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiMediaType>
                {
                    ["multipart/form-data"] = new Microsoft.OpenApi.Models.OpenApiMediaType
                    {
                        Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                        {
                            Type = "object",
                            Properties = allProperties
                        }
                    }
                }
            };

            // ?? parameters ??????? file ???? form data ?????? operation parameters
            if (operation.Parameters != null)
            {
                var paramsToRemove = operation.Parameters
                    .Where(p => parametersToRemove.Contains(p.Name))
                    .ToList();

                foreach (var param in paramsToRemove)
                {
                    operation.Parameters.Remove(param);
                }
            }
        }
    }

    private bool IsComplexType(Type type)
    {
        return !IsSimpleType(type) &&
               type != typeof(string) &&
               type != typeof(IFormFile) &&
               type != typeof(IFormFileCollection) &&
               !type.IsEnum &&
               type.IsClass;
    }

    private bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType.IsPrimitive ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(Guid) ||
               underlyingType.IsEnum;
    }

    private bool HasFromFormAttribute(System.Reflection.ParameterInfo parameter)
    {
        return parameter.GetCustomAttributes<Microsoft.AspNetCore.Mvc.FromFormAttribute>().Any();
    }

    private Microsoft.OpenApi.Models.OpenApiSchema GetSchemaForType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
            return new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" };
        if (underlyingType == typeof(int) || underlyingType == typeof(long))
            return new Microsoft.OpenApi.Models.OpenApiSchema { Type = "integer" };
        if (underlyingType == typeof(bool))
            return new Microsoft.OpenApi.Models.OpenApiSchema { Type = "boolean" };
        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
            return new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date-time" };
        if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
            return new Microsoft.OpenApi.Models.OpenApiSchema { Type = "number" };
        if (underlyingType.IsEnum)
            return new Microsoft.OpenApi.Models.OpenApiSchema
            {
                Type = "string",
                Enum = Enum.GetNames(underlyingType).Select(name => (Microsoft.OpenApi.Any.IOpenApiAny)new Microsoft.OpenApi.Any.OpenApiString(name)).ToList()
            };

        return new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" };
    }
}
