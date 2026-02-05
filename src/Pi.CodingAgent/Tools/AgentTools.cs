using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Pi.Agent;

namespace Pi.CodingAgent.Tools;

/// <summary>
/// Factory for creating agent tools for file operations and shell execution
/// </summary>
public static class AgentTools
{
    /// <summary>
    /// Creates all available agent tools
    /// </summary>
    public static List<AgentTool> CreateAll(string workingDirectory = ".")
    {
        var wd = Path.GetFullPath(workingDirectory);
        return new List<AgentTool>
        {
            CreateBashTool(wd),
            CreateReadTool(wd),
            CreateWriteTool(wd),
            CreateEditTool(wd),
            CreateGrepTool(wd),
            CreateFindTool(wd),
            CreateLsTool(wd)
        };
    }

    private static string ResolvePath(string workingDirectory, string path)
    {
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }
        return Path.GetFullPath(Path.Combine(workingDirectory, path));
    }

    private static void ValidatePath(string workingDirectory, string path)
    {
        var fullPath = ResolvePath(workingDirectory, path);
        var workingPath = Path.GetFullPath(workingDirectory);
        
        if (!fullPath.StartsWith(workingPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Access denied: Path '{path}' is outside working directory");
        }
    }

    public static AgentTool CreateBashTool(string workingDirectory)
    {
        return new AgentTool
        {
            Name = "bash",
            Label = "Execute Shell Command",
            Description = "Execute a bash/shell command and return its output. Use this to run commands, scripts, and interact with the system.",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    command = new { type = "string", description = "The command to execute" },
                    timeout = new { type = "number", description = "Timeout in seconds (default: 30)" }
                },
                required = new[] { "command" }
            },
            Execute = async (id, args, ct, stream) =>
            {
                var command = args.TryGetValue("command", out var cmd) ? cmd.ToString()! : "";
                var timeout = args.TryGetValue("timeout", out var to) ? Convert.ToInt32(to) : 30;

                var isWindows = OperatingSystem.IsWindows();
                var fileName = isWindows ? "cmd.exe" : "/bin/bash";
                var arguments = isWindows ? $"/c {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };
                var output = new StringBuilder();
                var error = new StringBuilder();

                process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) error.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

                try
                {
                    await process.WaitForExitAsync(linkedCts.Token);
                }
                catch (OperationCanceledException)
                {
                    try { process.Kill(true); } catch { }
                    var partial = $"Command timed out after {timeout} seconds\nPartial output:\n{output}";
                    return new ToolExecutionResult { Content = new List<object> { new { text = partial } } };
                }

                var result = new StringBuilder();
                result.AppendLine($"Exit Code: {process.ExitCode}");
                if (output.Length > 0) result.AppendLine($"\nOutput:\n{output}");
                if (error.Length > 0) result.AppendLine($"\nError:\n{error}");

                return new ToolExecutionResult { Content = new List<object> { new { text = result.ToString().Trim() } } };
            }
        };
    }

    public static AgentTool CreateReadTool(string workingDirectory)
    {
        return new AgentTool
        {
            Name = "read",
            Label = "Read File",
            Description = "Read the contents of a file. Returns the file content as text.",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Path to the file to read" }
                },
                required = new[] { "path" }
            },
            Execute = async (id, args, ct, stream) =>
            {
                try
                {
                    var path = args.TryGetValue("path", out var p) ? p.ToString()! : "";
                    ValidatePath(workingDirectory, path);
                    
                    var fullPath = ResolvePath(workingDirectory, path);
                    if (!File.Exists(fullPath))
                    {
                        return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: File not found: {path}" } } };
                    }

                    var content = await File.ReadAllTextAsync(fullPath, ct);
                    var result = $"File: {path}\nSize: {content.Length} characters\nLines: {content.Split('\n').Length}\n\nContent:\n{content}";
                    return new ToolExecutionResult { Content = new List<object> { new { text = result } } };
                }
                catch (Exception ex)
                {
                    return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: {ex.Message}" } } };
                }
            }
        };
    }

    public static AgentTool CreateWriteTool(string workingDirectory)
    {
        return new AgentTool
        {
            Name = "write",
            Label = "Write File",
            Description = "Write content to a file. Creates the file if it doesn't exist, overwrites if it does.",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Path to the file to write" },
                    content = new { type = "string", description = "Content to write to the file" }
                },
                required = new[] { "path", "content" }
            },
            Execute = async (id, args, ct, stream) =>
            {
                try
                {
                    var path = args.TryGetValue("path", out var p) ? p.ToString()! : "";
                    var content = args.TryGetValue("content", out var c) ? c.ToString()! : "";
                    
                    ValidatePath(workingDirectory, path);
                    var fullPath = ResolvePath(workingDirectory, path);

                    var directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var existed = File.Exists(fullPath);
                    await File.WriteAllTextAsync(fullPath, content, ct);

                    var action = existed ? "Updated" : "Created";
                    var result = $"{action} file: {path}\nSize: {content.Length} characters\nLines: {content.Split('\n').Length}";
                    return new ToolExecutionResult { Content = new List<object> { new { text = result } } };
                }
                catch (Exception ex)
                {
                    return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: {ex.Message}" } } };
                }
            }
        };
    }

    public static AgentTool CreateEditTool(string workingDirectory)
    {
        return new AgentTool
        {
            Name = "edit",
            Label = "Edit File",
            Description = "Edit a file by replacing old content with new content. Finds and replaces the first occurrence.",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Path to the file to edit" },
                    old_content = new { type = "string", description = "The exact content to find and replace" },
                    new_content = new { type = "string", description = "The new content to replace with" }
                },
                required = new[] { "path", "old_content", "new_content" }
            },
            Execute = async (id, args, ct, stream) =>
            {
                try
                {
                    var path = args.TryGetValue("path", out var p) ? p.ToString()! : "";
                    var oldContent = args.TryGetValue("old_content", out var old) ? old.ToString()! : "";
                    var newContent = args.TryGetValue("new_content", out var newC) ? newC.ToString()! : "";
                    
                    ValidatePath(workingDirectory, path);
                    var fullPath = ResolvePath(workingDirectory, path);

                    if (!File.Exists(fullPath))
                    {
                        return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: File not found: {path}" } } };
                    }

                    var content = await File.ReadAllTextAsync(fullPath, ct);
                    
                    if (!content.Contains(oldContent))
                    {
                        return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: Old content not found in file: {path}" } } };
                    }

                    var index = content.IndexOf(oldContent);
                    var updatedContent = content.Remove(index, oldContent.Length).Insert(index, newContent);

                    await File.WriteAllTextAsync(fullPath, updatedContent, ct);

                    var result = $"Edited file: {path}\nReplaced {oldContent.Length} characters with {newContent.Length} characters";
                    return new ToolExecutionResult { Content = new List<object> { new { text = result } } };
                }
                catch (Exception ex)
                {
                    return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: {ex.Message}" } } };
                }
            }
        };
    }

    public static AgentTool CreateGrepTool(string workingDirectory)
    {
        return new AgentTool
        {
            Name = "grep",
            Label = "Search Files",
            Description = "Search for a pattern in files. Supports regex patterns and glob file patterns.",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    pattern = new { type = "string", description = "The regex pattern to search for" },
                    path = new { type = "string", description = "File or directory to search in (supports glob patterns)" },
                    case_sensitive = new { type = "boolean", description = "Whether the search should be case-sensitive (default: false)" }
                },
                required = new[] { "pattern", "path" }
            },
            Execute = async (id, args, ct, stream) =>
            {
                try
                {
                    var pattern = args.TryGetValue("pattern", out var p) ? p.ToString()! : "";
                    var path = args.TryGetValue("path", out var pa) ? pa.ToString()! : "";
                    var caseSensitive = args.TryGetValue("case_sensitive", out var cs) && Convert.ToBoolean(cs);

                    var regexOptions = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    var regex = new Regex(pattern, regexOptions);

                    ValidatePath(workingDirectory, path);
                    var fullPath = ResolvePath(workingDirectory, path);

                    var results = new List<string>();
                    var files = new List<string>();

                    if (File.Exists(fullPath))
                    {
                        files.Add(fullPath);
                    }
                    else if (Directory.Exists(fullPath) || path.Contains("*"))
                    {
                        if (path.Contains("**"))
                        {
                            var parts = path.Split("**", 2);
                            var basePath = ResolvePath(workingDirectory, parts[0].TrimEnd('/', '\\'));
                            var filePattern = parts.Length > 1 ? parts[1].TrimStart('/', '\\') : "*";
                            if (Directory.Exists(basePath))
                            {
                                files.AddRange(Directory.GetFiles(basePath, filePattern, SearchOption.AllDirectories));
                            }
                        }
                        else if (path.Contains("*") || path.Contains("?"))
                        {
                            var directory = Path.GetDirectoryName(fullPath) ?? workingDirectory;
                            var filePattern = Path.GetFileName(path);
                            if (Directory.Exists(directory))
                            {
                                files.AddRange(Directory.GetFiles(directory, filePattern));
                            }
                        }
                        else if (Directory.Exists(fullPath))
                        {
                            files.AddRange(Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories));
                        }
                    }

                    foreach (var file in files)
                    {
                        var lines = await File.ReadAllLinesAsync(file, ct);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (regex.IsMatch(lines[i]))
                            {
                                var relativePath = Path.GetRelativePath(workingDirectory, file);
                                results.Add($"{relativePath}:{i + 1}: {lines[i].Trim()}");
                            }
                        }
                    }

                    var result = results.Count == 0 
                        ? $"No matches found for pattern: {pattern}"
                        : $"Found {results.Count} match(es):\n\n{string.Join("\n", results)}";
                    
                    return new ToolExecutionResult { Content = new List<object> { new { text = result } } };
                }
                catch (Exception ex)
                {
                    return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: {ex.Message}" } } };
                }
            }
        };
    }

    public static AgentTool CreateFindTool(string workingDirectory)
    {
        return new AgentTool
        {
            Name = "find",
            Label = "Find Files",
            Description = "Find files by name pattern. Supports glob patterns like '*.cs' or '**/*.txt'.",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    pattern = new { type = "string", description = "File name pattern to search for (supports * and ** wildcards)" },
                    path = new { type = "string", description = "Directory to search in (default: current directory)" }
                },
                required = new[] { "pattern" }
            },
            Execute = async (id, args, ct, stream) =>
            {
                try
                {
                    var pattern = args.TryGetValue("pattern", out var p) ? p.ToString()! : "";
                    var searchPath = args.TryGetValue("path", out var pa) ? pa.ToString()! : ".";

                    ValidatePath(workingDirectory, searchPath);
                    var fullSearchPath = ResolvePath(workingDirectory, searchPath);

                    if (!Directory.Exists(fullSearchPath))
                    {
                        return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: Directory not found: {searchPath}" } } };
                    }

                    var files = new List<string>();

                    if (pattern.Contains("**"))
                    {
                        var parts = pattern.Split("**", 2);
                        var filePattern = parts.Length > 1 ? parts[1].TrimStart('/', '\\') : "*";
                        files.AddRange(Directory.GetFiles(fullSearchPath, filePattern, SearchOption.AllDirectories));
                    }
                    else
                    {
                        files.AddRange(Directory.GetFiles(fullSearchPath, pattern, SearchOption.TopDirectoryOnly));
                    }

                    if (files.Count == 0)
                    {
                        return new ToolExecutionResult { Content = new List<object> { new { text = $"No files found matching pattern: {pattern}" } } };
                    }

                    var output = new StringBuilder();
                    output.AppendLine($"Found {files.Count} file(s):\n");
                    
                    foreach (var file in files.OrderBy(f => f))
                    {
                        var relativePath = Path.GetRelativePath(workingDirectory, file);
                        var fileInfo = new FileInfo(file);
                        output.AppendLine($"{relativePath} ({fileInfo.Length} bytes)");
                    }

                    return new ToolExecutionResult { Content = new List<object> { new { text = output.ToString() } } };
                }
                catch (Exception ex)
                {
                    return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: {ex.Message}" } } };
                }
            }
        };
    }

    public static AgentTool CreateLsTool(string workingDirectory)
    {
        return new AgentTool
        {
            Name = "ls",
            Label = "List Directory",
            Description = "List contents of a directory with file details (size, type, modified date).",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Directory path to list (default: current directory)" },
                    all = new { type = "boolean", description = "Show hidden files (default: false)" }
                },
                required = new string[] { }
            },
            Execute = async (id, args, ct, stream) =>
            {
                try
                {
                    var path = args.TryGetValue("path", out var p) ? p.ToString()! : ".";
                    var showAll = args.TryGetValue("all", out var a) && Convert.ToBoolean(a);

                    ValidatePath(workingDirectory, path);
                    var fullPath = ResolvePath(workingDirectory, path);

                    if (!Directory.Exists(fullPath))
                    {
                        return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: Directory not found: {path}" } } };
                    }

                    var output = new StringBuilder();
                    output.AppendLine($"Directory: {path}\n");

                    var directories = Directory.GetDirectories(fullPath)
                        .Where(d => showAll || !Path.GetFileName(d).StartsWith("."))
                        .OrderBy(d => d)
                        .ToList();

                    if (directories.Any())
                    {
                        output.AppendLine("Directories:");
                        foreach (var dir in directories)
                        {
                            output.AppendLine($"  [DIR]  {Path.GetFileName(dir)}/");
                        }
                        output.AppendLine();
                    }

                    var files = Directory.GetFiles(fullPath)
                        .Where(f => showAll || !Path.GetFileName(f).StartsWith("."))
                        .OrderBy(f => f)
                        .ToList();

                    if (files.Any())
                    {
                        output.AppendLine("Files:");
                        foreach (var file in files)
                        {
                            var fileInfo = new FileInfo(file);
                            var size = FormatSize(fileInfo.Length);
                            var modified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                            output.AppendLine($"  {size,10}  {modified}  {Path.GetFileName(file)}");
                        }
                    }

                    if (!directories.Any() && !files.Any())
                    {
                        output.AppendLine("(empty directory)");
                    }

                    return new ToolExecutionResult { Content = new List<object> { new { text = output.ToString() } } };
                }
                catch (Exception ex)
                {
                    return new ToolExecutionResult { Content = new List<object> { new { text = $"Error: {ex.Message}" } } };
                }
            }
        };
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.#} {sizes[order]}";
    }
}
