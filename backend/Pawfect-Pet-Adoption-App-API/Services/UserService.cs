using AutoMapper;
using Pawfect_Pet_Adoption_App_API.DTOs.User;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<GUserDTO> GetUserByIdAsync(string id)
        {
            return _mapper.Map<GUserDTO>(await _userRepository.GetByIdAsync(id));
        }

        public async Task<IEnumerable<GUserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GUserDTO>>(users);
        }
    }
}
