using BussinessLayer.Interface;
using GreenCloset.Repository.Implement;
using Microsoft.Extensions.Configuration;

namespace BussinessLayer.Implement
{
    public class FacadeService : IFacedeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public IUserService User { get; private set; }

        public FacadeService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            User = new UserService(_unitOfWork, _configuration);
        }
    }
}
