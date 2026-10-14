using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebHost.Middleware
{
    /// <summary>
    /// 简易 API Key 鉴权中间件。
    /// 对所有 /api/ 下的请求校验 <c>X-Api-Key</c> 请求头（或 <c>apiKey</c> 查询参数），
    /// 校验失败返回 401。密钥在 appsettings.json 的 "ApiKey" 中配置。
    /// 若 "ApiKey" 未配置（为空），则开发模式下放行全部 /api/ 请求。
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;
        private const string HeaderName = "X-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKey = configuration["ApiKey"] ?? string.Empty;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 仅对 /api/ 受保护接口鉴权
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    // 未配置密钥：开发模式放行（在 appsettings 设置 ApiKey 即可启用鉴权）
                    await _next(context);
                    return;
                }

                var provided = context.Request.Headers[HeaderName].FirstOrDefault()
                               ?? context.Request.Query["apiKey"].FirstOrDefault();

                if (string.IsNullOrEmpty(provided) || !string.Equals(_apiKey, provided, StringComparison.Ordinal))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = false,
                        message = "未授权：缺少或错误的 API Key",
                        result = (object?)null
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}
