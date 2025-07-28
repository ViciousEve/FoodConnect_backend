namespace FoodConnectAPI.Interfaces.Services
{
    public interface IUserService
    {
        public Task<bool> IsUsernameAvailableAsync(string username);
        public Task<bool> IsEmailAvailableAsync(string email);
        public Task<bool> IsPhoneNumberAvailableAsync(string phoneNumber);
    }
}
