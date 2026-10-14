using AdPang.FileManager.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using WebHost.Helper;

namespace WebHost.Controllers
{
    /// <summary>
    /// 远程重启控制器。与关机对齐，使用 <c>shutdown -r -t</c> 命令。
    /// 计划中的关机/重启共用同一笔待执行操作，CancelShutdown 同样可取消重启。
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class RestartWindowsController : ControllerBase
    {
        public RestartWindowsController()
        {

        }

        /// <summary>
        /// 在指定时间重启。默认会多留 10s 缓冲，避免传参瞬间过期。
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse<ShutdownViewModel> RestartAt(DateTime dateTime)
        {
            var now = DateTime.Now;
            if (dateTime < DateTime.Now)
            {
                return new ApiResponse<ShutdownViewModel>(false, $"所传入时间: {dateTime} 小于当前时间: {now} ！");
            }
            var seconds = Math.Floor((dateTime - now).TotalSeconds) + 10;
            var commandStr = "Shutdown -r -t " + seconds;

            string cmdResult;
            try
            {
                cmdResult = CmdHelper.CmdExecute(commandStr);
            }
            catch (Exception e)
            {
                return new ApiResponse<ShutdownViewModel>(false, "发生错误：" + e.Message);
            }

            return new ApiResponse<ShutdownViewModel>(true, new ShutdownViewModel()
            {
                ExecuteTime = now,
                RemainSeconds = (int)seconds,
                CommandStr = commandStr,
                ShutdownAt = dateTime.AddSeconds(10),
                CmdResultDetail = cmdResult
            })
            { Message = $"定时重启设定成功！将在 {dateTime.AddSeconds(10).ToShortTimeString()} 重启！" };
        }

        /// <summary>
        /// 延迟在几秒后重启，最低 5，若小于 5 视为 5。会先取消已有计划再设定新的。
        /// </summary>
        /// <param name="delaySeconds"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse<ShutdownViewModel> RestartDelay(int delaySeconds = 5)
        {
            if (delaySeconds < 5) delaySeconds = 5;
            var now = DateTime.Now;
            var seconds = delaySeconds;
            var commandStr = "Shutdown -r -t " + seconds;
            string cmdResult;
            try
            {
                //先取消已有计划（关机/重启共用同一笔）
                CmdHelper.CmdExecute("Shutdown -a");
                //进行定时重启
                cmdResult = CmdHelper.CmdExecute(commandStr);
            }
            catch (Exception e)
            {
                return new ApiResponse<ShutdownViewModel>(false, "发生错误：" + e.Message);
            }

            return new ApiResponse<ShutdownViewModel>(true, new ShutdownViewModel()
            {
                ExecuteTime = now,
                RemainSeconds = seconds,
                CommandStr = commandStr,
                ShutdownAt = now.AddSeconds(seconds),
                CmdResultDetail = cmdResult
            })
            { Message = $"设置延迟重启成功，将在 {delaySeconds} 秒后重启" };
        }

        /// <summary>
        /// 取消待执行操作（关机/重启共用同一笔计划，一条命令即可取消）。
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse<ShutdownViewModel> CancelRestart()
        {
            var commandStr = "Shutdown -a";
            string cmdResult;
            try
            {
                cmdResult = CmdHelper.CmdExecute(commandStr);
            }
            catch (Exception e)
            {
                return new ApiResponse<ShutdownViewModel>(false, "发生错误：" + e.Message);
            }

            return new ApiResponse<ShutdownViewModel>(true, new ShutdownViewModel
            {
                CommandStr = commandStr,
                ExecuteTime = DateTime.Now,
                CmdResultDetail = cmdResult
            })
            {
                Message = "取消重启成功"
            };
        }
    }
}
