---
name: dotnet-coding
description: Best practices and guidelines for writing C# code in .NET projects.
---

# .NET Coding Skill

This skill provides guidance for writing high-quality C# code in .NET projects.

## Code Style

- Use PascalCase for public members and types
- Use camelCase for private fields (with _ prefix)
- Use var when the type is obvious
- Prefer expression-bodied members when concise
- Use nullable reference types (#nullable enable)
- Add XML documentation comments for public APIs

## Modern C# Features

- Use record types for immutable data
- Use init-only properties
- Use pattern matching where appropriate
- Use async/await for asynchronous operations
- Use System.Threading.Channels for async streaming
- Prefer LINQ for collection operations

## Best Practices

1. **Error Handling**: Use structured exception handling, avoid swallowing exceptions
2. **Async**: Don't block on async code, use ConfigureAwait when appropriate
3. **Disposal**: Implement IDisposable for resources, use using statements
4. **Testing**: Write unit tests with xUnit, use FluentAssertions for readable assertions
5. **Nullability**: Check for null with null-coalescing and null-conditional operators

## Common Patterns

### Channel-Based Streaming

To avoid yield-in-try-catch issues:

```csharp
public async IAsyncEnumerable<T> StreamAsync()
{
    var channel = Channel.CreateUnbounded<T>();
    var writeTask = WriteToChannelAsync(channel.Writer);
    
    await foreach (var item in channel.Reader.ReadAllAsync())
        yield return item;
    
    await writeTask;
}
```
