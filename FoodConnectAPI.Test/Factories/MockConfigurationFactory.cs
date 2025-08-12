using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FoodConnectAPI.Test.Factories
{
    public static class MockConfigurationFactory
    {
        public static Mock<IConfiguration> CreateJwtMock()
        {
            var config = new Mock<IConfiguration>();
            config.Setup(x => x["Jwt:SecretKey"]).Returns("your-super-secret-key-with-at-least-32-characters");
            config.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("30");
            return config;
        }
    }
}
