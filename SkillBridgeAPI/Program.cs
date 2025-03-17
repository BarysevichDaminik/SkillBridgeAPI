using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using SkillBridgeAPI.Models;
using SkillBridgeAPI.Services;
using System.Net.NetworkInformation;

namespace SkillBridgeAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            string? localIpAddress = GetIPAddressForAlias("Wi-FI") ?? "localhost";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins($"https://{localIpAddress}:3000", "https://localhost:3000", "https://192.168.31.212:3000")
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    });
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(7186, listenOptions =>
                {
                    listenOptions.UseHttps("certificate.pfx");
                });
            });

            builder.Services.AddScoped<RefreshTokenService>();

            //var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            builder.Services.AddDbContext<SkillbridgeContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication("JwtScheme").AddScheme<JwtBearerOptions, CookiesAuthenticationHandler>("JwtScheme", options => { });

            var app = builder.Build();

            app.UseCors("AllowFrontend");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }
        static string? GetIPAddressForAlias(string alias)
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.Name.Equals(alias, StringComparison.OrdinalIgnoreCase) &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    foreach (var unicastAddress in ipProperties.UnicastAddresses)
                    {
                        if (unicastAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return unicastAddress.Address.ToString();
                        }
                    }
                }
            }
            return null;
        }

    }
}
