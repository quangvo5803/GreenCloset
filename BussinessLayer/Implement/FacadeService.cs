using BussinessLayer.Interface;
using Microsoft.Extensions.Configuration;
using Repository.Implement;

namespace BussinessLayer.Implement
{
    public class FacadeService : IFacedeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public IUserService User { get; private set; }
        public ICategoryService Category { get; private set; }

        public FacadeService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            User = new UserService(_unitOfWork, _configuration);
            Category = new CategoryService(_unitOfWork);
        }
    }
}
