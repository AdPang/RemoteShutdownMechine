
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
            // 配置 Kestrel 绑定多个 IP 和端口
            builder.WebHost.ConfigureKestrel(options =>
            {
                // 如果你希望监听所有网卡的某个端口，也可以使用如下代码：
                options.ListenAnyIP(5555);
            });

            //var ipAddresses = GetAllLocalIPv4Addresses();

            //var a = ipAddresses.Select(ipAddress => $"{ipAddress}:5555").ToArray();

            //string configIp = "http://localhost:5555";
            //foreach (var ipAddress in ipAddresses) 
            //{
            //    configIp += $";http://{ipAddress}:5555";
            //}

            //builder.WebHost.UseUrls(configIp);

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

        //private static string[] GetAllLocalIPv4Addresses()
        //{
        //    return Dns.GetHostEntry(Dns.GetHostName())
        //               .AddressList
        //               .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
        //               .Select(ip => ip.ToString())
        //               .ToArray();
        //}
    }
}
