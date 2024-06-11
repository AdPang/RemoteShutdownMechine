using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using AdPang.FileManager.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace WebHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public TestController()
        {
            
        }

        [HttpGet]
        public ApiResponse<string> Test()
        {
            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
            string result = $"MachineName:{Environment.MachineName}";   //本地计算机名  

            return new ApiResponse<string>(true, "Test")
            {
                Result = result
            };

        }
    }
}
