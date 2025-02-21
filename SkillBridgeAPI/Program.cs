using Microsoft.EntityFrameworkCore;
using SkillBridgeAPI.Models;

namespace SkillBridgeAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000")
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    });
            });

            //var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            Globals.connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")!;
            builder.Services.AddDbContext<SkillbridgeContext>(options => options.UseNpgsql(Globals.connectionString));

            var app = builder.Build();
            
            app.UseCors("AllowFrontend");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
