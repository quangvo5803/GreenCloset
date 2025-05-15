using BussinessLayer.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Repository.Implement;
using Utility.Email;

namespace BussinessLayer.Implement
{
    public class FacadeService : IFacedeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailQueue _emailQueue;
        public IUserService User { get; private set; }
        public ICategoryService Category { get; private set; }
        public IProductService Product { get; private set; }
        public IItemImageService ItemImage { get; private set; }
        public ICartService Cart { get; private set; }

        public FacadeService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            IEmailQueue emailQueue
        )
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _emailQueue = emailQueue;
            User = new UserService(_unitOfWork, _configuration, _emailQueue);
            Category = new CategoryService(_unitOfWork);
            Product = new ProductService(_unitOfWork, _webHostEnvironment);
            ItemImage = new ItemImageService(_unitOfWork, _webHostEnvironment);
            Cart = new CartService(_unitOfWork);
        }
    }
}
