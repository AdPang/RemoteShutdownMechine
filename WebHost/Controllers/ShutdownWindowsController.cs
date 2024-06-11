using AdPang.FileManager.Shared;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using WebHost.Helper;

namespace WebHost.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ShutdownWindowsController : ControllerBase
    {
        public ShutdownWindowsController()
        {
                
        }

        /// <summary>
        /// 默认会添加10s的缓冲时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse<ShutdownViewModel> ShutdownAt(DateTime dateTime)
        {
            var now = DateTime.Now;
            if(dateTime < DateTime.Now)
            {
                return new ApiResponse<ShutdownViewModel>(false, $"所传入时间: {dateTime} 小于当前时间: {now} ！");
            }
            var seconds = Math.Floor((dateTime - now).TotalSeconds) + 10;
            var commandStr = "Shutdown -s -t " + seconds;

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
            { Message = $"定时关机设定成功！将在 {dateTime.AddSeconds(10).ToShortTimeString()} 关机！"};
        }

        /// <summary>
        /// 延迟在几秒后关机，最低5，若小于5视为5
        /// </summary>
        /// <param name="delaySeconds"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse<ShutdownViewModel> ShutdownDelay(int delaySeconds = 5)
        {
            if (delaySeconds < 5) delaySeconds = 5;
            var now = DateTime.Now;
            var seconds = delaySeconds;
            var commandStr = "Shutdown -s -t " + seconds;
            string cmdResult;
            try
            {
                //先取消关机
                CmdHelper.CmdExecute("Shutdown -a");
                //进行定时关机
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
            { Message = $"设置延迟关机成功，将在 {delaySeconds} 秒后关机" };
        }

        /// <summary>
        /// 取消关机
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse<ShutdownViewModel> CancelShutdown()
        {
            var commandStr = "Shutdown -a";
            string cmdResult;
            try
            {
                //先取消关机
                CmdHelper.CmdExecute("Shutdown -a");
                //进行定时关机
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
                Message = "取消关机成功"
            };
        }

    }
}
