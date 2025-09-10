#### This a fork from my own organization ignexai, just to have it publicly available on my profile repositories
# AquaPP

AquaPP is a .NET Avalonia UI application designed to provide a comprehensive solution for water quality management and treatment process optimization. It offers features for data entry, chat-based interactions, and dashboard visualizations to help users monitor and manage water-related data efficiently.

## Features

*   **Data Entry:** Input and manage water quality readings and other relevant data.
*   **Chat Interface:** Interact with an AI agent for insights and assistance related to water treatment.
*   **Dashboard:** Visualize key metrics and trends in water quality and treatment processes.
*   **Settings:** Configure application preferences and themes.

## Technologies Used

*   **.NET 9.0:** The core framework for the application logic.
*   **Avalonia UI:** A cross-platform UI framework for building desktop applications.
*   **Entity Framework Core:** For database interactions and data persistence.
*   **SQLite:** The default database used for local data storage.

## Development Environment Setup

To set up the development environment for AquaPP, follow these steps:

1.  **Install .NET SDK:**
    Ensure you have the .NET 9.0 SDK installed on your machine. You can download it from the official .NET website: [https://dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)

2.  **Install Visual Studio (Windows) or Rider (Cross-platform):**
    *   **Visual Studio (Windows):** Download and install Visual Studio with the ".NET desktop development" workload.
    *   **JetBrains Rider (Cross-platform):** Download and install JetBrains Rider, which provides excellent support for .NET development on Windows, macOS, and Linux.

3.  **Clone the Repository:**
    ```bash
    git clone https://github.com/Abdssamie/aqua-plus-plus.git
    cd AquaPP
    ```
    *(Note: Replace `https://github.com/Abdssamie/aqua-plus-plus.git` with the actual repository URL if it's hosted elsewhere.)*

4.  **Restore NuGet Packages:**
    Navigate to the project root directory (where `AquaPP.sln` is located) and restore the NuGet packages:
    ```bash
    dotnet restore
    ```

5.  **Apply Database Migrations:**
    If the application uses a database, apply the migrations to set up the database schema. Ensure you are in the directory containing the `.csproj` file (e.g., `AquaPP/`):
    ```bash
    dotnet ef database update
    ```

## Running the Application

After setting up the development environment, you can run the AquaPP application:

1.  **Build the Project:**
    Navigate to the project root directory (where `AquaPP.sln` is located) and build the solution:
    ```bash
    dotnet build
    ```

2.  **Run the Application:**
    From the project root directory, you can run the application:
    ```bash
    dotnet run --project AquaPP
    ```
    This command will launch the AquaPP desktop application.
