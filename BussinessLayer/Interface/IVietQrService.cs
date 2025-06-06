using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Interface
{
    public interface IVietQrService
    {
        Task<string> GenerateQrCodeBase64Async(int amount, string description);
    }
}
