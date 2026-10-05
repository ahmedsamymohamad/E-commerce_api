# E-Commerce Backend API

An ASP.NET Core 10 Web API demonstrating a layered e-commerce backend with
SQL Server, Entity Framework Core, JWT authentication, inventory management,
cart checkout, order status transitions, validation, logging, and OpenAPI
documentation.

## Technology

- ASP.NET Core 10 Web API
- C# and Entity Framework Core
- SQL Server / LocalDB
- JWT bearer authentication
- Built-in ASP.NET Core OpenAPI support
- BCrypt password hashing

## Architecture

```text
HTTP Controllers
        |
Application Services
        |
EF Core DbContext
        |
SQL Server
```

Controllers only handle HTTP concerns. Business rules are implemented in the
services:

- `AuthService` registers users and creates JWTs.
- `ProductService` handles product search, filtering, sorting, pagination, and
  creation.
- `CartService` validates product availability and maintains cart quantities.
- `OrderService` performs transactional checkout, validates stock, calculates
  totals, creates order snapshots, and reduces inventory.

## Prerequisites

- .NET SDK 10
- SQL Server or SQL Server LocalDB
- Optional: VS Code REST Client extension for the `.http` examples

## Configuration

The default development connection string is in
[`appsettings.json`](./appsettings.json):

```json
"Server=(localdb)\\MSSQLLocalDB;Database=ECommerceApi;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

For another SQL Server instance, replace `ConnectionStrings:DefaultConnection`
or provide it through an environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=ECommerceApi;Trusted_Connection=True;TrustServerCertificate=True"
```

Change `Jwt:Key` to a long, random secret before deploying. Do not commit
production secrets to configuration files.

## Database setup

The repository contains EF Core migrations in [`Migrations/`](./Migrations/).
Apply them with:

```powershell
dotnet ef database update
```

If the EF tool is not installed:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

To create a new migration after changing the model:

```powershell
dotnet ef migrations add DescribeYourChange
dotnet ef database update
```

The migrations seed the `Electronics` and `Home` categories and the `Contoso`
and `Fabrikam` brands.

## Run the API

```powershell
dotnet restore
dotnet build
dotnet run
```

The default HTTP URL is:

```text
http://localhost:5078
```

## OpenAPI / Swagger documentation

This project exposes an interactive Swagger UI during development:

```text
http://localhost:5078/swagger
```

Open the page in a browser to browse endpoints, inspect DTO schemas, execute
requests, and authorize JWT-protected endpoints. Click **Authorize**, enter
`Bearer <jwt-token>`, and click **Authorize** again.

The project also exposes the raw Swagger JSON document at:

```text
http://localhost:5078/swagger/v1/swagger.json
```

The built-in .NET 10 OpenAPI generator remains available at:

```text
http://localhost:5078/openapi/v1.json
```

The Swagger UI document includes the controllers, request DTOs, response DTOs,
validation metadata, JSON string enum values, and a Bearer JWT security
scheme.

## Authentication

### Register

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "Customer123!",
  "name": "Demo Customer"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "Customer123!"
}
```

Copy the returned `token` and send it on protected requests:

```http
Authorization: Bearer <jwt-token>
```

## API endpoints

| Method | Route | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register a customer |
| `POST` | `/api/auth/login` | Public | Authenticate and receive a JWT |
| `GET` | `/api/products` | Public | Search, filter, sort, and paginate products |
| `GET` | `/api/products/{id}` | Public | Get one product |
| `POST` | `/api/products` | Admin | Create a product |
| `GET` | `/api/cart` | Customer/Admin | Get the current user's cart |
| `POST` | `/api/cart/items` | Customer/Admin | Add a product to the cart |
| `POST` | `/api/orders` | Customer/Admin | Checkout the current user's cart |
| `GET` | `/api/orders/my-orders` | Customer/Admin | List the current user's orders |
| `PUT` | `/api/orders/{id}/status` | Admin | Change an order status |

Product listing supports:

```text
/api/products?page=1&pageSize=20
/api/products?search=mouse
/api/products?category=Electronics
/api/products?sort=price_asc
/api/products?sort=price_desc
/api/products?sort=name
```

## Checkout business flow

1. The customer adds a product and quantity to their cart.
2. The cart service verifies that the product is active and the requested
   quantity does not exceed current stock.
3. Checkout opens a SQL transaction.
4. Each cart item is checked again against current inventory.
5. The order total is calculated from the current product prices.
6. Order items store product name and unit-price snapshots.
7. Inventory is reduced.
8. The order is persisted and the cart is cleared.
9. The transaction commits. If any validation fails, no inventory or order
   changes are committed.

## Error handling

Errors are returned as RFC 7807 Problem Details by the global exception
handler. Common responses include:

- `400 Bad Request` for invalid DTO validation
- `401 Unauthorized` for missing or invalid JWTs
- `404 Not Found` for missing resources
- `409 Conflict` for business rule violations such as insufficient stock
- `500 Internal Server Error` for unexpected failures

## Request examples

The complete request walkthrough is in
[`E-commerce API.http`](./E-commerce%20API.http), including registration,
login, product search, cart operations, checkout, order history, and admin
status updates.

## Validation

```powershell
dotnet build
```

The current project builds with zero warnings and zero errors.
