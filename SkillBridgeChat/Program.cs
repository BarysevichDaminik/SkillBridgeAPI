using Microsoft.EntityFrameworkCore;
using SkillBridgeChat.Hubs;
using SkillBridgeChat.Models;
using System.Diagnostics;
using System.Net;

namespace SkillBridgeChat
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();

            builder.Services.AddControllers();

            var hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(hostName);
            string localIpAddress = hostEntry.AddressList[4].ToString();

            localIpAddress ??= "localhost";


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins($"https://{localIpAddress}:3000", "https://localhost:3000")
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
    }
}
