using LMPE_API.DAL;
using LMPE_API.Data;
using LMPE_API.Hubs;
using LMPE_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddScoped<JwtService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .SetIsOriginAllowed(_ => true));

app.MapControllers();

app.MapHub<MessageHub>("/messageHub"); // route du hub
app.MapHub<AgendaHub>("/agendaHub"); // route du hub

// Swagger uniquement en dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.Start();

    if (app.Urls.Any())
    {
        var url = app.Urls.First();
        var swaggerUrl = url + "/swagger";
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = swaggerUrl,
                UseShellExecute = true
            });
        }
        catch { }
    }

    app.WaitForShutdown();
}
else
{
    app.Run();
}
