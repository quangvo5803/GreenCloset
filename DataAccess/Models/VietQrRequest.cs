using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class VietQrRequest
    {
        public string accountNo { get; set; }
        public string accountName { get; set; }
        public string acqId { get; set; }
        public int amount { get; set; }
        public string addInfo { get; set; }
        public string template { get; set; } = "compact";
    }
}
