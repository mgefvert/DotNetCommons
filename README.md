# DotNetCommons

DotNetCommons is a comprehensive suite of utility libraries for .NET, designed to streamline common programming tasks and extend the standard library with powerful, reusable components.

## Project Modules

The solution is divided into several specialized modules:

*   **DotNetCommons**: The core library containing fundamental utilities, extension methods, and specialized data structures.
*   **DotNetCommons.EF**: Extensions and patterns for Entity Framework Core, including CRUD services and partial update (patching) support.
*   **DotNetCommons.Web**: Helpers for ASP.NET Core, including HTTP context extensions and specialized action attributes.
*   **DotNetCommons.Services**: Integration services for external providers, specifically focusing on Email (SMTP, Debug) and SMS (Spirius, Debug).
*   **DotNetCommons.WinForms**: A collection of Windows Forms controls, extensions, and Win32 API wrappers.
*   **DotNetCommons.PlaywrightTesting**: Utilities for building robust automated UI tests using Playwright.

## Core Features

### Extension Methods
Extensive set of extensions for common .NET types:
*   **Strings**: Case conversion, sub-item extraction, and advanced manipulation.
*   **Collections**: Enhanced LINQ-like operations and collection helpers.
*   **DateTime/DateOnly**: Period calculations, business day logic, and formatting.
*   **IO**: Simplified file and stream operations.

### Specialized Collections
*   **Grid<T>**: A flexible 2D grid structure.
*   **CircularBuffer<T>**: Efficient fixed-size buffer.
*   **DrawList<T>**: Specialized list for drawing/selection logic.

### Temporal & Holidays
*   **Holiday Calculation**: Built-in support for Swedish and US holidays, with extensible logic for religious and relative holidays.
*   **DateRange**: Manage and compare periods of time easily.

### Text Processing
*   **Aho-Corasick**: High-performance multi-pattern string searching.
*   **Shunting-Yard & Tokenizer**: Tools for building expression parsers.
*   **Wildcards**: Support for file-style wildcard matching.
*   **FixedWidth**: Tools for handling fixed-width text formats.

### Numerics & Check Digits
*   **Check Digits**: Implementations of Luhn and ABA check digit algorithms.
*   **Roman Numerals**: Conversion to and from Roman numeral strings.
*   **JiwiConverter**: Specialized numeric encoding.

### IO & File System
*   **IFileAccessor**: An abstraction layer for file systems, allowing easy switching between physical storage and in-memory mocks for testing.

## Getting Started

DotNetCommons is built for .NET 8.0 and later. To use it in your project, add the reference to the specific module you need.

```powershell
dotnet add package DotNetCommons
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
