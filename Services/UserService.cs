using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using SearchTool_ServerSide.Dtos.UserDtos;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;
using ServerSide;

namespace SearchTool_ServerSide.Services
{
    public class UserSevice(UserRepository _userRepository, IMapper _mapper, JwtOptions jwtOptions,BranchRepository _branchRepository)
    {
        internal async Task<UserReadDto> Register(UserAddDto userAddDto)
        {
            var user = _mapper.Map<User>(userAddDto);

            user = await _userRepository.Add(user);
            var userReadDto = _mapper.Map<UserReadDto>(user);
            return userReadDto;
        }
        internal async Task<UserReadDto> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            var userReadDto = _mapper.Map<UserReadDto>(user);
            return userReadDto;
        }

        internal async Task<(string accessToken, string refreshToken, string userId, string branchId)?> Login(UserLoginDto userLoginDto)
        {
            var user = await _userRepository.GetUserByEmail(userLoginDto.Email);
            if (user == null)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
            {
                return null;
            }
            var accessToken = TokenGenerate(user, expiresInMinutes: 3);
            var refreshToken = TokenGenerate(user, expiresInDays: 1);
            var userId = user.Id.ToString();
            var branchId = user.BranchId.ToString();
            return (accessToken, refreshToken, userId, branchId);
        }
        public string TokenGenerate(User user, int expiresInMinutes = 60, int expiresInDays = 0)
        {
            var expirationDate = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            if (expiresInDays > 0)
            {
                expirationDate = DateTime.UtcNow.AddDays(expiresInDays);
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new(ClaimTypes.Email,user.Email),
                    new(ClaimTypes.Role,user.Role.ToString()),
                    new("BranchId",user.BranchId.ToString())
                }),
                Expires = expirationDate

            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);
            return accessToken;
        }
        public async Task<UserReadDto?> GetUserById(int id)
        {
            var user = await _userRepository.GetById(id);
            var userReadDto = _mapper.Map<UserReadDto>(user);
            userReadDto.RoleName = user.Role.ToString();
            var branch = await _branchRepository.GetById(user.BranchId);
            userReadDto.BranchName = branch.Name;
            return userReadDto;
        }
        public async Task<(string accessToken, string refreshToken, string userId)?> Refresh(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                return null;
            }
            var accessToken = TokenGenerate(user, expiresInMinutes: 1);
            var refreshToken = TokenGenerate(user, expiresInDays: 2);
            var userId = user.Id.ToString();
            return (accessToken, refreshToken, userId);
        }

        internal async Task<UserReadDto> UserUpdate(int userId, UserUpdateDto userUpdateDto)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null)
                return null;
            _mapper.Map(userUpdateDto, user);
            await _userRepository.Update(user);
            var userReadDto = _mapper.Map<UserReadDto>(user);
            return userReadDto;
        }

        internal async Task<UserReadDto> DeleteUserById(int id)
        {
            var user = await _userRepository.Delete(id);
            var userReadDto = _mapper.Map<UserReadDto>(user);
            return userReadDto;
        }

        internal async Task<ICollection<UserReadDto>> GetAllUser()
        {
            var users = await _userRepository.GetAll();
            var userReadDtos = _mapper.Map<ICollection<UserReadDto>>(users);
            return userReadDtos;
        }
    }
}