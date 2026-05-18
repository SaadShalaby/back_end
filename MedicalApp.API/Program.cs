using MedicalApp.API.Data;
using MedicalApp.API.Hubs;
using MedicalApp.API.Models;
using MedicalApp.API.Services;
using MedicalApp.API.Services.Implementation;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== 1️⃣ Database =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== 2️⃣ Identity =====
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ===== 3️⃣ JWT Authentication =====
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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };

    // ✅ SignalR uses Query String for WebSocket (headers not supported)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/notifications") ||
                 path.StartsWithSegments("/hubs/chat")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ===== 4️⃣ Services =====
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ===== 5️⃣ SignalR =====
builder.Services.AddSignalR();

// ===== 6️⃣ Controllers =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MedicalApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Enter JWT token like: Bearer {your token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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

var app = builder.Build();

// ===== Static Files =====
app.UseStaticFiles();

// ===== Auto-create Roles =====
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Doctor", "Patient", "Admin" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// ===== Middleware Pipeline =====
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MedicalApp API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ===== SignalR Hubs =====
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();