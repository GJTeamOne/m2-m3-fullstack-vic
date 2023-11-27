# [WebApplication] - .NET Core Web Application

## Description

This .NET Core web application provides a RESTful API for managing users. It supports CRUD operations (Create, Read, Update, Delete) for `User` entities, including related data such as `ContactDetails`.

## Features

- CRUD operations for User entity.
- Detailed validation and error handling.
- Integration with MediatR for CQRS pattern.
- Use of AutoMapper for object mapping.
- Extensive unit and integration testing.

## Getting Started

### Prerequisites

- .NET Core 3.1
- Visual Studio 2019 or later

### Setup and Installation

1. Clone the repository:
2. Open the solution in Visual Studio.
3. Restore NuGet packages.
4. Update the connection string in `appsettings.json`.
5. Run the application.

## Testing
- Integration tests can be found in `WebApplication.IntegrationTests`.