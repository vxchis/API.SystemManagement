using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SystemManagement.Application.Common.Security;
using SystemManagement.Domain.Constants;
using SystemManagement.Infrastructure;
using SystemManagement.Infrastructure.Authentication;
using SystemManagement.Infrastructure.Persistence;
using SystemManagement.Infrastructure.Realtime;
using SystemManagement.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing JWT configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireAuthenticatedUser().RequireAssertion(ctx => HasMinimumRoleLevel(ctx.User, RoleLevels.Admin)));

    options.AddPolicy(AuthorizationPolicies.ManagerOrAdmin, policy =>
        policy.RequireAuthenticatedUser().RequireAssertion(ctx => HasMinimumRoleLevel(ctx.User, RoleLevels.PhoPhong)));

    options.AddPolicy(AuthorizationPolicies.StaffOrHigher, policy =>
        policy.RequireAuthenticatedUser().RequireAssertion(ctx => HasMinimumRoleLevel(ctx.User, RoleLevels.ChuyenVien)));

    options.AddPolicy(AuthorizationPolicies.DepartmentManager, policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim(ClaimNames.DepartmentId)
            .RequireAssertion(ctx => HasMinimumRoleLevel(ctx.User, RoleLevels.PhoPhong)));
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SystemManagement API",
        Version = "v1",
        Description = ".NET 10 Clean Architecture API with JWT, hierarchical roles, department groups, task assignment, file attachments, SignalR notifications and ready-to-run migrations"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste raw JWT token here. Do not prefix with Bearer."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var uploadsRoot = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "uploads");
Directory.CreateDirectory(uploadsRoot);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("ReactClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

await SeedData.InitializeAsync(app.Services);

app.Run();

static bool HasMinimumRoleLevel(ClaimsPrincipal user, int requiredLevel)
{
    var value = user.FindFirstValue(ClaimNames.RoleLevel);
    return int.TryParse(value, out var level) && level >= requiredLevel;
}
