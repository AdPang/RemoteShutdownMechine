using System;
using System.ComponentModel.DataAnnotations;

namespace WebHost.Models
{
    /// <summary>
    /// 账号表。密码以「salt:hash」形式存储（PBKDF2），不保存明文。
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>格式：Base64(salt):Base64(hash)</summary>
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
