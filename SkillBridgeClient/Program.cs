using Microsoft.AspNetCore.SignalR.Client;

namespace SkillBridgeClient
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7215/myhub")
                .Build();

            connection.On<string>("SendMessage", message =>
            {
                Console.WriteLine($"Received: {message}");
            });

            while (true)
            {
                try
                {
                    await connection.StartAsync();
                    break;
                }
                catch { Console.WriteLine("Could not establish a connection!\nStarting to try again."); }
            }

            Console.WriteLine("Connected!");

            Console.ReadLine();
        }
    }
}
