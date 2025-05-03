# final-project---TKPM1
# DevTools

DevTools is a comprehensive suite of tools designed for developers and professionals. It includes multiple projects such as APIs, UI, and utility tools to enhance productivity and streamline workflows.

## Table of Contents
- [Projects Overview](#projects-overview)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Running with Docker](#running-with-docker)
  - [Setting the Startup Project](#setting-the-startup-project)
- [Features](#features)
- [Contributing](#contributing)

---

## Projects Overview

The solution consists of the following projects:

1. **DevTools.API**: The backend API for managing tools, authentication, and data.
2. **DevTools.UI**: The frontend application built with WinUI for interacting with the tools.
3. **DevTools.Application**: Contains business logic and services.
4. **DevTools.DataAccess**: Handles database interactions and migrations.
5. **Tools**: Utility tools.

---

## Getting Started

### Prerequisites

Ensure you have the following installed on your system:
- [Docker](https://www.docker.com/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the following workloads:
  - **.NET Desktop Development**
  - **ASP.NET and Web Development**
- .NET SDK 8.0 or later

---

### Running with Docker

To run the solution using Docker, follow these steps:

1. **Setup the Docker**:
	- Open the Developer PowerShell
	- Navigate to the project's folder: ...\DevTools
	- Run this command: docker-compose up
2. **Access the Application**:
   - The API will be available at `https://localhost:5000/swagger/index.html`.
   - If the UI is hosted, follow the instructions provided in the container logs to access it.

---

### Setting the Startup Project

To run the solution in Visual Studio, set the startup project to `DevTools.UI`:

1. Open the solution in Visual Studio 2022.
2. In the __Solution Explorer__, right-click on the `DevTools.UI` project.
3. Select __Set as Startup Project__.
4. Press `F5` to build and run the application.

---

## Features

- **User Authentication**: Secure login and registration with JWT-based authentication.
- **Tool Management**: Add, edit, and manage tools and tool groups.
- **Premium Features**: Access exclusive tools with a premium subscription.
- **Utility Tools**: Includes tools like Token generator, JSON to CSV conversion,...
- **Responsive UI**: Built with WinUI for a modern and intuitive user experience.

---

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Commit your changes and push them to your fork.
4. Submit a pull request with a detailed description of your changes.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

   