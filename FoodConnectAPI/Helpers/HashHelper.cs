namespace FoodConnectAPI.Helpers
{
    //Password hashing and verification helper class
    public class HashHelper
    {
        public static string HashPassword(string password)
        {
            // A secure hashing algorithm Bcrypt
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
