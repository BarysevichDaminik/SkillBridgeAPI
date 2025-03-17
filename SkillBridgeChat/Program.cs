using Microsoft.EntityFrameworkCore;
using SkillBridgeChat.Hubs;
using SkillBridgeChat.Models;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace SkillBridgeChat
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();

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
                options.ListenAnyIP(7215, listenOptions =>
                {
                    listenOptions.UseHttps("../SkillBridgeAPI/certificate.pfx");
                });
            });

            builder.Services.AddDbContext<SkillbridgeContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            app.UseCors("AllowFrontend");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<ChatHub>("/myhub");

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
