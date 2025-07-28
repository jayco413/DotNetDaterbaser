# Agents Guidance

This repository contains a simple .NET command line tool. The project files live in `DotNetDaterbaser/` and include a sample agents file used when the program creates `AGENTS.md` in a target directory.

## Run

    # 1 — Restore packages (idempotent & cached)
    dotnet restore

    # 2 — Build the main solution / project
    dotnet build --configuration Release --no-restore

    # 3 — Run tests if you have them
    if find . -name '*Tests.csproj' | grep -q .; then
      dotnet test --no-build --verbosity normal
    fi

## Connection String

Ensure your test project can connect to the containerized SQL Server using a connection string like this (from environment variable or config):

    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost,1433;Database=TestDb;User Id=sa;Password=YourStrong!Passw0rd;"
      }
    }

# C# Style Guide

This guide establishes the minimum code quality standards for all C# source files within this project.

---

## 1. File Structure

- Each class must reside in its own `.cs` file.
- The file name must match the class name (e.g., `UserService.cs` for `UserService`).
- Nested classes are discouraged; refactor into standalone classes if needed.

---

## 2. Access Modifiers

- All classes, methods, and members must explicitly declare an access modifier: `public`, `protected`, or `private`.
- Do not use `internal` or `protected internal`.
- Do not omit the access modifier (no defaults).

Example:
```csharp
public class UserService
{
    private string _connectionString;

    public void Initialize() { }
}
```

---

## 3. Namespaces

- All code must be declared within a logically grouped namespace.
- Namespace names should match the folder structure.
- Use the block-scoped namespace declaration introduced in C# 10+.
- Namespace names should follow the format: `Company.Product.Module`.

Example:
```csharp
namespace MyCompany.MyApp.Services
{
    public class EmailService
    {
        // ...
    }
}
```

---

## 4. Bracing and Control Flow

- Always use braces (`{}`) for all control flow blocks, even when they contain a single statement.
- Applies to: `if`, `else`, `for`, `foreach`, `while`, `do`, `switch`, etc.

Example:
```csharp
if (isValid)
{
    Save();
}
else
{
    LogError();
}
```

---

## 5. XML Documentation

- All classes and methods must be preceded by XML documentation comments.
- At a minimum, use:
  - `<summary>` for class/method purpose.
  - `<param>` for each method parameter.
  - `<returns>` for method return type, if not `void`.

Example:
```csharp
/// <summary>
/// Sends an email to the specified recipient.
/// </summary>
/// <param name="to">The recipient's email address.</param>
/// <param name="subject">The subject line of the email.</param>
/// <param name="body">The HTML body of the email.</param>
/// <returns>True if the email was sent successfully.</returns>
public bool SendEmail(string to, string subject, string body)
{
    // ...
}
```

---

## 6. Dead Code Policy

- Unused code must be deleted immediately.
- This includes:
  - Unused classes or methods.
  - Unused `using` directives.
  - Commented-out blocks of obsolete logic.

Use tools like Visual Studio’s Code Cleanup, Analyzer Warnings, or ReSharper to identify dead code.

---

## 7. Inline Comments

- Inline comments must be concise, relevant, and used sparingly.
- Use only when the intent is not obvious from the code itself.
- Prefer XML comments for documentation and avoid redundant or obvious comments.

Example:
```csharp
// Cache the result to avoid repeated DB hits
var user = _userCache.Get(userId);
```

Avoid:
```csharp
// Increment i
i++;
```

---

## 8. Naming Conventions

- Use `PascalCase` for classes, methods, and public properties.
- Use `camelCase` for local variables and private fields.
- Prefix private fields with `_` (underscore).
- Avoid abbreviations unless they are widely accepted (e.g., `Http`, `Xml`).

---

## 9. Constructor Injection

- Required dependencies should be injected via constructors.
- All injected fields must be marked `readonly`.

Example:
```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
}
```

---

## 10. Avoid Magic Numbers and Strings

- Extract literals to `const` or `static readonly` fields with meaningful names.

Example:
```csharp
private const int MaxRetryCount = 3;
private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
```

---

## 11. Expression-bodied Members

- Use expression-bodied syntax for concise, single-expression properties or methods.

Example:
```csharp
public string FullName => $"{FirstName} {LastName}";
```

---

## 12. Exceptions and Control Flow

- Do not use exceptions for regular control flow.
- Catch only the exceptions you intend to handle.

Example:
```csharp
if (File.Exists(path))
{
    var content = File.ReadAllText(path);
}
```

---

## 13. `var` Usage

- Use `var` when the type is obvious from the right-hand side.
- Use explicit types when clarity is improved or the type is not obvious.

Examples:
```csharp
var config = new Configuration();       // Clear
List<string> names = LoadNames();       // Not obvious — use explicit
```

---

## 14. Method Scope and Length

- Keep methods short and focused.
- Each method should have one clear purpose.
- Extract private helper methods when logic is multi-step or branching.

---

## 15. Async Best Practices

- Use `async`/`await` for all I/O and long-running operations.
- Avoid `async void` except for event handlers.
- Use `ConfigureAwait(false)` for library/internal code where context capture is unnecessary.

Example:
```csharp
public async Task<string> LoadDataAsync()
{
    var response = await _httpClient.GetAsync("/data");
    return await response.Content.ReadAsStringAsync();
}
```

---
```

## 16. Implicit Usings

- Implicit `using` directives are **enabled** to reduce boilerplate and maintain clean top-of-file declarations.
- Grayed-out `using` statements (whether implicitly included or unused) **must be deleted**.
- Developers are responsible for removing any unnecessary `using` directives that are:
  - Redundant due to implicit usings (e.g., `System`, `System.IO`, `System.Linq`)
  - Not referenced in the file

This keeps files minimal and reduces noise without sacrificing clarity.

To view which namespaces are included implicitly, refer to:
- https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#implicit-using-directives
- Project type (Console, ASP.NET, etc.) determines which namespaces are included automatically

✅ Tools:
- Visual Studio: Right-click → “Remove Unused Usings”
- `dotnet format` CLI
- ReSharper: Code cleanup profiles