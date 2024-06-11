using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdPang.FileManager.Shared;
using Shared.Dtos;

namespace HttpRequest.IServices
{
    public interface ITestRequestService
    {
        Task<ApiResponse<string>> TestHostConnection();
    }
}
