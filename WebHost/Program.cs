
using System.Net.Sockets;
using System.Net;

namespace WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var ipAddresses = GetAllLocalIPv4Addresses();

            var a = ipAddresses.Select(ipAddress => $"{ipAddress}:5555").ToArray();

            string configIp = "http://localhost:5555";
            foreach (var ipAddress in ipAddresses) 
            {
                configIp += $";http://{ipAddress}:5555";
            }

            builder.WebHost.UseUrls(configIp);

            /*var a = ipAddresses.Select(ipAddress => $"{ipAddress}:5555").ToArray();
            builder.WebHost.UseUrls(ipAddresses);*/

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static string[] GetAllLocalIPv4Addresses()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                       .AddressList
                       .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                       .Select(ip => ip.ToString())
                       .ToArray();
        }
    }
}
