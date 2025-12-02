using FoodConnectAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDto> AuthenticateAsync(UserLoginDto userLoginDto);
        Task<UserDto> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<bool> IsEmailAvailableAsync(string email);
        Task UpdateProfilePicture(int userId, IFormFile profilePicture);
        Task<UserDto> UpdateProfile(int userId, UserUpdateDto userUpdateDto);
        Task DeleteAsync(string email);
    }
}
