# Agents Guidance

This repository contains a simple .NET command line tool. The project files live in `DotNetDaterbaser/` and include a sample agents file used when the program creates `AGENTS.md` in a target directory.

## run
```
# 1 — Restore packages (idempotent & cached)
dotnet restore

# 2 — Build the main solution / project
dotnet build --configuration Release --no-restore

# 3 — Run tests if you have them (adjust pattern as needed)
if find . -name '*Tests.csproj' | grep -q .; then
  dotnet test --no-build --verbosity normal
fi
```
