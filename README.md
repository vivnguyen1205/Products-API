
# Products API
My First Web API!!!
This project is a simple **CRUD** **REST API** built using **ASP.NET Core** and **Entity Framework Core**. The API allows you to manage a list of products and is secured using **JWT (JSON Web Token) authentication**.
## Features

- **CRUD Operations**: Create, Read, Update, and Delete products.
- **JWT Authentication**: Secure your API endpoints with JWT tokens.
- **Entity Framework Core**: Interact with the database using EF Core.
- **Database Migrations**: Manage database schema changes with migrations.

## Technologies Used

- **ASP.NET Core**: A cross-platform framework for building modern cloud-based web applications.
- **Entity Framework Core**: An object-relational mapper (ORM) for .NET, providing a high-level abstraction for working with databases.
- **JWT (JSON Web Token)**: A compact and self-contained way to securely transmit information between parties as a JSON object.

## Getting Started

### Prerequisites

- **.NET 6.0 SDK**: [Download .NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- **SQL Server**: You can use SQL Server Express or any other edition.

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/vivnnguyen1205/Products-API.git
   cd ProductManagementAPI
   ```

2. **Install dependencies:**
   ```bash
   dotnet restore
   ```

### Database Setup

1. **Update the connection string** in `appsettings.json` to point to your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=your_server_name;Database=ProductDB;Trusted_Connection=True;"
   }
   ```

2. **Apply migrations** to create the database schema:
   ```bash
   dotnet ef database update
   ```

### Running the Application

1. **Run the application:**
   ```bash
   dotnet run
   ```

2. The API will be available at `https://localhost:5001`.

## API Endpoints

### Products

- **Get All Products**
  - `GET /api/products`
- **Get Product by ID**
  - `GET /api/products/{id}`
- **Create New Product**
  - `POST /api/products`
- **Update Product**
  - `PUT /api/products/{id}`
- **Delete Product**
  - `DELETE /api/products/{id}`

### Authentication

- **Login**
  - `POST /api/auth/login`
  - Request Body: 
    ```json
    {
      "Email": "your_email",
      "password": "your_password"
    }
    ```
  - Response: 
    ```json
    {
      "token": "your_jwt_token"
    }
    ```

You will need to include the JWT token in the `Authorization` header of your requests to access secured endpoints:

```http
Authorization: Bearer your_jwt_token
```
