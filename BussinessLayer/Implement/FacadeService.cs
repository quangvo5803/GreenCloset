using BussinessLayer.Interface;
using Microsoft.Extensions.Configuration;
using Repository.Implement;
using Utility.Email;
using Utility.Media;

namespace BussinessLayer.Implement
{
    public class FacadeService : IFacedeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailQueue _emailQueue;
        private readonly CloudinaryService _cloudinaryService;

        public IUserService User { get; private set; }
        public ICategoryService Category { get; private set; }
        public IProductService Product { get; private set; }
        public IItemImageService ItemImage { get; private set; }
        public ICartService Cart { get; private set; }
        public IOrderService Order { get; private set; }
        public IFeedBackService FeedBack { get; private set; }
        public IVnPayService VnPayService { get; private set; }
        public IVietQrService VietQrService { get; private set; }

        public FacadeService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IVnPayService vnPayService,
            IEmailQueue emailQueue,
            IHttpClientFactory httpClientFactory
        )
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailQueue = emailQueue;
            _cloudinaryService ??= new CloudinaryService(_configuration);

            VnPayService = vnPayService;
            VietQrService = new VietQrService(_configuration, httpClientFactory);
            User = new UserService(_unitOfWork, _configuration, _emailQueue, _cloudinaryService);
            Category = new CategoryService(_unitOfWork);
            Product = new ProductService(_unitOfWork, _cloudinaryService);
            ItemImage = new ItemImageService(_unitOfWork, _cloudinaryService);
            Cart = new CartService(_unitOfWork);
            Order = new OrderService(_unitOfWork, VnPayService, _configuration, _emailQueue);
            FeedBack = new FeedBackService(_unitOfWork, _cloudinaryService);
        }
    }
}
