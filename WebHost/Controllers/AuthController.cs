using AdPang.FileManager.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebHost.Data;
using WebHost.Models;
using WebHost.Services;

namespace WebHost.Controllers
{
    /// <summary>
    /// 账号登录与注册。Login / Register 匿名可访问；
    /// 关机、重启等控制接口需携带本接口签发的 Bearer 令牌。
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

        /// <summary>
        /// 注册新账号（默认对局域网开放，生产环境可改为仅管理员调用）。
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return Ok(new ApiResponse<object>(false, "请输入用户名和密码"));
            }

            if (req.Username.Length < 3)
            {
                return Ok(new ApiResponse<object>(false, "用户名至少 3 位"));
            }

            if (req.Password.Length < 6)
            {
                return Ok(new ApiResponse<object>(false, "密码至少 6 位"));
            }

            if (_db.Users.Any(u => u.Username == req.Username))
            {
                return Ok(new ApiResponse<object>(false, "该用户名已存在"));
            }

            _db.Users.Add(new User
            {
                Username = req.Username,
                PasswordHash = PasswordHasher.Hash(req.Password),
                CreatedAt = DateTime.UtcNow
            });
            _db.SaveChanges();

            return Ok(new ApiResponse<object>(true, null) { Message = "注册成功，请登录" });
        }
    }
}
