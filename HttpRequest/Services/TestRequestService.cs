using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdPang.FileManager.Shared;
using HttpRequest.IServices;
using HttpRequestClient.Services;
using RestSharp;

namespace HttpRequest.Services
{
    public class TestRequestService : ITestRequestService
    {
        private readonly HttpRestClient httpRestClient;

        public TestRequestService(HttpRestClient httpRestClient) 
        {
            this.httpRestClient = httpRestClient;
        }
        public async Task<ApiResponse<string>> TestHostConnection()
        {
            var result = await httpRestClient.ExecuteAsync<string>(new BaseRequest()
            {
                ContentType = "application/json",
                Method = Method.GET,
                Route = "api/test",
            });
            return result;
        }
    }
}
