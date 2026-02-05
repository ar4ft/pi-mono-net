# Troubleshooting Guide
Common issues and solutions.

## Authentication Issues
Clear cached tokens:
```bash
rm -rf ~/.pi/auth/
```

## Permission Issues (macOS)
Grant Full Disk Access to Terminal in System Settings.

## Build Failures
```bash
dotnet restore
dotnet clean
dotnet build
```

See [Getting Started](getting-started.md) for more help.
