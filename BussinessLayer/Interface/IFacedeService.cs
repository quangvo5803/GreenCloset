﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Interface
{
    public interface IFacedeService
    {
        IUserService User { get; }
        ICategoryService Category { get; }
        IProductService Product { get; }
        IItemImageService ItemImage { get; }
        ICartService Cart { get; }
        IOrderService Order { get; }
        IFeedBackService FeedBack { get; }
        IVnPayService VnPayService { get; }
        IVietQrService VietQrService { get; }
    }
}
