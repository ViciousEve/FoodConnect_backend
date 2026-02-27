# 🍽️ FoodConnectAPI

A RESTful Web API for a food recipe sharing social platform built with **ASP.NET Core 9.0**. Users can share recipes, interact through likes and comments, and connect with food enthusiasts worldwide.

![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-blue)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Database Schema](#-database-schema)
- [Testing](#-testing)

---

## ✨ Features

- **User Authentication** - JWT-based authentication with secure password hashing (BCrypt)
- **Recipe Posts** - Create, read, update, delete food recipes with images
- **Social Interactions** - Like posts, comment on recipes, follow other users
- **Tagging System** - Organize recipes with tags for easy discovery
- **File Upload** - Support for multiple image uploads per post
- **Rate Limiting** - Protect API from abuse with request throttling
- **Profile Management** - Update profile info and profile pictures

---

## 🛠️ Tech Stack

| Category | Technology |
|----------|------------|
| **Framework** | ASP.NET Core 9.0 |
| **ORM** | Entity Framework Core 9.0 |
| **Database** | SQL Server |
| **Authentication** | JWT Bearer Tokens |
| **Password Hashing** | BCrypt.Net |
| **API Documentation** | Swagger / OpenAPI |
| **Testing** | xUnit, Moq, FluentAssertions |

---

## 🏗️ Architecture

The project follows a **Clean Architecture** pattern with clear separation of concerns:

```
FoodConnectAPI/
├── Controllers/        # API endpoints (HTTP layer)
├── Services/           # Business logic layer
├── Repositories/       # Data access layer
├── Interfaces/         # Abstractions (dependency inversion)
│   ├── Services/
│   └── Repositories/
├── Entities/           # Database models
├── Models/             # DTOs (Data Transfer Objects)
├── Data/               # DbContext & seeding
├── Helpers/            # Utility classes
└── Migrations/         # EF Core migrations
```

### Design Patterns Used
- **Repository Pattern** - Abstracts data access logic
- **Dependency Injection** - Loose coupling between layers
- **DTO Pattern** - Separates internal models from API contracts

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/FoodConnectAPI.git
   cd FoodConnectAPI
   ```

2. **Configure the database connection**
   
   Update `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "FoodConnectDB": "Server=YOUR_SERVER;Database=FoodConnect;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Configure JWT settings** (for production, use User Secrets)
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:SecretKey" "YourSuperSecretKeyHere123!@#"
   ```

4. **Apply database migrations**
   ```bash
   cd FoodConnectAPI
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   
   Open your browser and navigate to: `https://localhost:5001/swagger`

---

## 📡 API Endpoints

### Authentication
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register a new user | ❌ |
| POST | `/api/auth/login` | Login and get JWT token | ❌ |

### Posts
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/posts` | Get all posts | ❌ |
| POST | `/api/posts` | Create a new post | ✅ |
| PUT | `/api/posts/{id}` | Update a post | ✅ |
| DELETE | `/api/posts/{id}` | Delete a post | ✅ |

### Comments
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/posts/{postId}/comments` | Get comments for a post | ❌ |
| POST | `/api/posts/{postId}/comments` | Add a comment | ✅ |
| PATCH | `/api/comments/{id}` | Update a comment | ✅ |

### Likes
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/posts/{postId}/likes` | Toggle like on a post | ✅ |

### Users
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| PATCH | `/api/users/profile-picture` | Update profile picture | ✅ |
| PATCH | `/api/users/update` | Update user profile | ✅ |

### Tags
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/tags` | Get all tags | ❌ |

---

## 🗄️ Database Schema

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│    User     │────<│    Post     │────<│   Comment   │
└─────────────┘     └─────────────┘     └─────────────┘
       │                   │
       │                   │
       ▼                   ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Follow    │     │    Like     │     │    Media    │
└─────────────┘     └─────────────┘     └─────────────┘
                           │
                    ┌──────┴──────┐
                    ▼             ▼
             ┌─────────────┐ ┌─────────────┐
             │   PostTag   │ │    Tag      │
             └─────────────┘ └─────────────┘
```

### Main Entities
- **User** - Account information, profile, authentication
- **Post** - Recipe with title, ingredients, description, calories
- **Comment** - User comments on posts
- **Like** - User likes on posts
- **Follow** - User following relationships
- **Tag** - Recipe categorization
- **Media** - Image storage for posts

---

## 🧪 Testing

The project includes unit tests using **xUnit**, **Moq**, and **FluentAssertions**.

```bash
# Run all tests
cd FoodConnectAPI.Test
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure
```
FoodConnectAPI.Test/
├── Factories/          # Test utilities (InMemoryContextFactory)
├── Services/           # Service layer tests
│   ├── UserServiceTest.cs
│   ├── PostServiceTest.cs
│   ├── CommentServiceTest.cs
│   └── ...
└── Repositories/       # Repository layer tests
    └── UserRepositoryTest.cs
```

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

---

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📧 Contact

Nguyen Thanh Tuan - tuannguyenonair@gmail.com

Project Link: [https://github.com/ViciousEve/FoodConnectAPI](https://github.com/ViciousEve/FoodConnectAPI)