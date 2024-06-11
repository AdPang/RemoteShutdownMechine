using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public class ShutdownViewModel
    {
        public string CommandStr { get; set; }
        public DateTime? ShutdownAt { get; set; }
        public DateTime ExecuteTime { get; set; }
        public int? RemainSeconds { get; set; }

        public string CmdResultDetail { get; set; }

    }
}
