using FoodConnectAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnectAPI.Interfaces.Services
{
    public interface IUserService
    {
        public Task<UserDto> AuthenticateAsync([FromBody] UserLoginDto userLoginDto);
        public Task RegisterAsync([FromBody] UserRegisterDto userRegisterDto);
        public Task<bool> IsEmailAvailableAsync(string email);
        public Task DeleteAsync(string email);
    }
}
