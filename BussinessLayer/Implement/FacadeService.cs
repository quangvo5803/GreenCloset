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
        private readonly IVnPayService _vpnPayService;
        private readonly IEmailQueue _emailQueue;
        private readonly CloudinaryService _cloudinaryService;

        public IUserService User { get; private set; }
        public ICategoryService Category { get; private set; }
        public IProductService Product { get; private set; }
        public IItemImageService ItemImage { get; private set; }
        public ICartService Cart { get; private set; }
        public IOrderService Order { get; private set; }
        public IFeedBackService FeedBack { get; private set; }

        public FacadeService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IVnPayService vnPayService,
            IEmailQueue emailQueue
        )
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _vpnPayService = vnPayService;
            _emailQueue = emailQueue;
            _cloudinaryService ??= new CloudinaryService(_configuration);
            User = new UserService(_unitOfWork, _configuration, _emailQueue);
            Category = new CategoryService(_unitOfWork);
            Product = new ProductService(_unitOfWork, _cloudinaryService);
            ItemImage = new ItemImageService(_unitOfWork, _cloudinaryService);
            Cart = new CartService(_unitOfWork);
            Order = new OrderService(_unitOfWork, _vpnPayService, _configuration, _emailQueue);
            FeedBack = new FeedBackService(_unitOfWork, _cloudinaryService);
        }
    }
}
