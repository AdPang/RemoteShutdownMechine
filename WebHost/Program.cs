using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Text;
using WebHost.Data;
using WebHost.Models;
using WebHost.Services;

namespace WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // EF Core + MySQL（用户信息存储在本机 MySQL，端口 3306 / root / root）
            var connStr = builder.Configuration.GetConnectionString("MySql");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 36))));

            // JWT 鉴权（账号登录后签发 Bearer 令牌）
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var secret = jwtSection["Secret"] ?? "LanShutdownDefaultSecretChangeMe";
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                    };
                    // 令牌缺失/失效时返回与 ApiResponse 对齐的 JSON 结构
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(new
                            {
                                status = false,
                                message = "未授权或登录已失效，请重新登录",
                                result = (object?)null
                            });
                        }
                    };
                });
            builder.Services.AddScoped<JwtService>();

            // 监听任意网卡的 5555 端口
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5555);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // 确保数据库与表存在，并在首次启动时播种默认管理员账号
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                if (!db.Users.Any())
                {
                    db.Users.Add(new User
                    {
                        Username = "admin",
                        PasswordHash = PasswordHasher.Hash("admin123"),
                        CreatedAt = DateTime.UtcNow
                    });
                    db.SaveChanges();
                    // 生产环境请尽快修改默认管理员密码（可注册新账号后在数据库中改密）。
                }
            }

            app.Run();
        }
    }
}
