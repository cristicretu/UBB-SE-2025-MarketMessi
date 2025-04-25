# UBB-SE-2025-MarketMessi

# Coding Style Rules

## Naming Conventions
1. Use PascalCase for class names, interface names, and method names.
2. Use camelCase for variable names, parameter names, and private fields.
3. Interface names must begin with the letter 'I' (e.g., ICustomerService).
4. Constant fields should be named using PascalCase.
5. Do not use Hungarian notation for variable names.
6. Do not prefix field names with underscores.

## Code Layout
7. Braces should always be on their own line for methods, classes, and control structures.
8. Each statement should be on its own line; do not place multiple statements on a single line.
9. Do not use more than one blank line in a row.
10. Code should not contain trailing whitespace at the end of lines.
11. Closing braces should not be preceded by a blank line.
12. Single-line comments should begin with a single space after the comment delimiter (// Comment).
13. Opening braces should not be preceded by a blank line.

## Syntax and Formatting
14. Use tabs for indentation instead of spaces.
15. Use string.Empty instead of "" for empty strings.
16. Use built-in type aliases (e.g., string instead of String, int instead of Int32).
17. Always include braces for control structures (if, for, while), even for single-line blocks.
18. Always declare access modifiers explicitly (public, private, protected, internal).
19. Use shorthand syntax for nullable types (int? instead of Nullable<int>).

## Organization
20. System using directives should be placed before other using directives.
21. Property accessors should follow order: get then set.
22. Use arithmetic expressions with explicit parentheses to declare precedence.
23. Use conditional expressions with explicit parentheses to declare precedence.

---

A Windows application for managing an online marketplace with features for buying, selling, borrowing, and auctioning products.

## Features

- 🛍️ **Product Management**
  - Buying products
  - Selling products
  - Borrowing products
  - Auction system
- 🔍 **Search & Filter**
  - Product filtering by tags
  - Advanced search functionality
- 📊 **User Features**
  - Product reviews
  - Product comparison
  - Shopping basket
  - Bidding system

## Dev Setup

Create an `appsettings.json` file in the `MarketMinds` directory with the following content:

```json
{
  "LocalDataSource": "np:\\\\.\\pipe\\your_pipe_name\\tsql\\query",
  "InitialCatalog": "your_database_name",
  "ImgurSettings": {
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret"
  }
}
```

You can find the pipe by running:
```cmd
SqlLocalDB.exe start
SqlLocalDB.exe info MSSQLLocalDB
```

## Project Structure

```
UBB-SE-2025-MarketMinds/
├── 📱 MarketMinds/                   # Main application directory
│   ├── 🖼️ Views/                     # UI components (View layer)
│   │   └── Pages/                    # Main application pages
│   │
│   ├── 📊 ViewModels/                # View models (ViewModel layer)
│   │
│   ├── 📦 Models/                    # Data models (Model layer)
│   │
│   ├── 🔄 Services/                  # Business logic services
│   │
│   ├── 🗄️ Repositories/              # Data access layer
│   │
│   ├── 🧰 Helpers/                   # Helper utilities
│   │
│   ├── 🗄️ Data/                      # Data layer
│   │
│   ├── 🖼️ Assets/                    # Application assets
│   │
│   ├── ⚙️ Properties/                # Project properties
│   │
│   ├── 📄 App.xaml                   # Application definition
│   ├── 📄 App.xaml.cs                # Application code-behind
│   ├── 📄 MainWindow.xaml            # Main window definition
│   ├── 📄 MainWindow.xaml.cs         # Main window code-behind
│   ├── 📄 MarketMinds.csproj         # Project file
│   ├── 📄 MarketMinds.sln            # Solution file
│   └── 📄 appsettings.json           # Application settings
```
## Demo

Hand made GUI video - Seminar 2: [Watch on YouTube](https://youtu.be/OBPRiNfDDVs?si=lRMacDvDzZtjhuQG)
