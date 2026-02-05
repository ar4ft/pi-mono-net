# .NET Coding Skill Heartbeat

This skill monitors .NET coding best practices compliance.

## What to Check

1. **Code Quality**: Review recent code changes for compliance with .NET best practices
2. **Security**: Check for potential security vulnerabilities
3. **Performance**: Look for performance anti-patterns
4. **Testing**: Ensure adequate test coverage

## Response Guidelines

- Reply `HEARTBEAT_OK` if all code follows .NET best practices
- Alert if any violations are found
- Provide specific file locations and recommendations

## Example Alert

```
⚠️ Found potential issue in src/MyClass.cs:
- Using 'any' type instead of proper type inference
- Missing XML documentation comments
- Consider adding input validation
```
