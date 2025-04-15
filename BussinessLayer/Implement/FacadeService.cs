using BussinessLayer.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Repository.Implement;

namespace BussinessLayer.Implement
{
    public class FacadeService : IFacedeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public IUserService User { get; private set; }
        public ICategoryService Category { get; private set; }
        public IProductService Product { get; private set; }
        public IItemImageService ItemImage { get; private set; }
        public ICartService Cart { get; private set; }
        public FacadeService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            User = new UserService(_unitOfWork, _configuration);
            Category = new CategoryService(_unitOfWork);
            Product = new ProductService(_unitOfWork, _webHostEnvironment);
            ItemImage = new ItemImageService(_unitOfWork, _webHostEnvironment);
            Cart = new CartService(_unitOfWork);
        }
    }
}
