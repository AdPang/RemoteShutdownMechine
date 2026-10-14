using AdPang.FileManager.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebHost.Data;
using WebHost.Models;
using WebHost.Services;

namespace WebHost.Controllers
{
    /// <summary>
    /// 账号登录。Login 匿名可访问；
    /// 关机、重启等控制接口需携带本接口签发的 Bearer 令牌。
    /// （账号只通过本机 MySQL 中预置/手动维护，不开放注册接口。）
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(ApplicationDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        /// <summary>
        /// 账号登录。成功返回令牌（Bearer），客户端后续请求在 Authorization 头携带。
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return Ok(new ApiResponse<LoginResult>(false, "请输入用户名和密码"));
            }

            var user = _db.Users.FirstOrDefault(u => u.Username == req.Username);
            if (user == null || !PasswordHasher.Verify(req.Password, user.PasswordHash))
            {
                return Ok(new ApiResponse<LoginResult>(false, "用户名或密码错误"));
            }

            var expiresMin = _jwt.ExpiryMinutes;
            var result = new LoginResult
            {
                Token = _jwt.GenerateToken(user.Username),
                Username = user.Username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiresMin),
                ExpiresInMinutes = expiresMin
            };

            return Ok(new ApiResponse<LoginResult>(true, result) { Message = "登录成功" });
        }
    }
}
