using AdPang.FileManager.Shared;
using HttpRequest.IServices;
using HttpRequestClient.Services;
using RestSharp;
using Shared.Dtos;

namespace HttpRequest.Services
{
    public class RemoteShutdownRequestService : IRemoteShutdownRequestService
    {
        private readonly HttpRestClient httpRestClient;

        public RemoteShutdownRequestService(HttpRestClient httpRestClient)
        {
            this.httpRestClient = httpRestClient;
        }

        public async Task<ApiResponse<ShutdownViewModel>> CancelShutdown()
        {
            var result = await httpRestClient.ExecuteAsync<ShutdownViewModel>(new BaseRequest()
            {
                ContentType = "application/json",
                Method = Method.POST,
                Route = "api/shutdownWindows/cancelShutdown",
            });
            return result;
        }

        public async Task<ApiResponse<ShutdownViewModel>> SetDelayShutdown(int delaySeconds)
        {
            var result = await httpRestClient.ExecuteAsync<ShutdownViewModel>(new BaseRequest()
            {
                ContentType = "application/json",
                Method = Method.POST,
                Route = "api/shutdownWindows/shutdownDelay?delaySeconds=" + delaySeconds,
            });
            return result;
        }

        public async Task<ApiResponse<ShutdownViewModel>> SetShutdownAt(DateTime dateTime)
        {
            throw new NotImplementedException();
        }
    }
}
