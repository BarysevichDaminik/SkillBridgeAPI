using Microsoft.EntityFrameworkCore;
using SkillBridgeChat.Hubs;
using SkillBridgeChat.Models;

namespace SkillBridgeChat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins("https://192.168.166.233:3000")
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
