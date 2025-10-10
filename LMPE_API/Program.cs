using LMPE_API.DAL;
using LMPE_API.Data;
using LMPE_API.Hubs;
using LMPE_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Récupérer JWT depuis configuration
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var jwtExpireHours = int.Parse(builder.Configuration["Jwt:ExpireHours"]!);

// Services
builder.Services.AddSingleton<Database>();
builder.Services.AddScoped<UserDal>();
builder.Services.AddScoped<GroupeConversationDal>();
builder.Services.AddScoped<MessageDal>();
builder.Services.AddScoped<AgendaDal>();
builder.Services.AddScoped<CourbeCADal>();
builder.Services.AddScoped<PushDal>();
builder.Services.AddScoped<FileStorageDal>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PushService>();
builder.Services.AddControllers();
builder.Services.AddSignalR();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });

var app = builder.Build();

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Activer fichiers statiques
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .SetIsOriginAllowed(_ => true));

app.MapControllers();

app.MapHub<MessageHub>("/messageHub");
app.MapHub<AgendaHub>("/agendaHub");
app.MapHub<CourbecaHub>("/courbecaHub");

app.Run();
