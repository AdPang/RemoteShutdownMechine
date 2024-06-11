using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdPang.FileManager.Shared;
using Shared.Dtos;

namespace HttpRequest.IServices
{
    public interface IRemoteShutdownRequestService
    {
        Task<ApiResponse<ShutdownViewModel>> SetDelayShutdown(int delaySeconds);

        Task<ApiResponse<ShutdownViewModel>> SetShutdownAt(DateTime dateTime);

        Task<ApiResponse<ShutdownViewModel>> CancelShutdown();
    }
}
