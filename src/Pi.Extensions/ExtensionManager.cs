using System.Reflection;

namespace Pi.Extensions;

/// <summary>
/// Extension/Plugin interface
/// </summary>
public interface IExtension
{
    /// <summary>
    /// Extension ID
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Extension name
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Extension version
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Extension description
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Initialize the extension
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Start the extension
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Stop the extension
    /// </summary>
    Task StopAsync();
}

/// <summary>
/// Extension loader and manager
/// </summary>
public class ExtensionManager
{
    private readonly Dictionary<string, LoadedExtension> _extensions = new();
    private readonly string _extensionsDirectory;
    
    public ExtensionManager(string extensionsDirectory)
    {
        _extensionsDirectory = extensionsDirectory ?? throw new ArgumentNullException(nameof(extensionsDirectory));
        Directory.CreateDirectory(_extensionsDirectory);
    }
    
    /// <summary>
    /// Discover extensions in the extensions directory
    /// </summary>
    public List<string> DiscoverExtensions()
    {
        var extensions = new List<string>();
        
        foreach (var dll in Directory.GetFiles(_extensionsDirectory, "*.dll"))
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(dll);
                extensions.Add(dll);
            }
            catch
            {
                // Skip invalid assemblies
            }
        }
        
        return extensions;
    }
    
    /// <summary>
    /// Load an extension from a DLL file
    /// </summary>
    public async Task<bool> LoadExtensionAsync(string assemblyPath)
    {
        try
        {
            // Load the assembly
            var assembly = Assembly.LoadFrom(assemblyPath);
            
            // Find types that implement IExtension
            var extensionType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(IExtension).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            
            if (extensionType == null)
                return false;
            
            // Create instance
            var extension = Activator.CreateInstance(extensionType) as IExtension;
            if (extension == null)
                return false;
            
            // Initialize
            await extension.InitializeAsync();
            
            // Store
            _extensions[extension.Id] = new LoadedExtension
            {
                Extension = extension,
                AssemblyPath = assemblyPath,
                LoadedAt = DateTime.UtcNow,
                IsActive = false
            };
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Unload an extension
    /// </summary>
    public async Task<bool> UnloadExtensionAsync(string extensionId)
    {
        if (!_extensions.TryGetValue(extensionId, out var loaded))
            return false;
        
        if (loaded.IsActive)
        {
            await loaded.Extension.StopAsync();
        }
        
        _extensions.Remove(extensionId);
        return true;
    }
    
    /// <summary>
    /// Start an extension
    /// </summary>
    public async Task<bool> StartExtensionAsync(string extensionId)
    {
        if (!_extensions.TryGetValue(extensionId, out var loaded))
            return false;
        
        if (loaded.IsActive)
            return true;
        
        await loaded.Extension.StartAsync();
        loaded.IsActive = true;
        
        return true;
    }
    
    /// <summary>
    /// Stop an extension
    /// </summary>
    public async Task<bool> StopExtensionAsync(string extensionId)
    {
        if (!_extensions.TryGetValue(extensionId, out var loaded))
            return false;
        
        if (!loaded.IsActive)
            return true;
        
        await loaded.Extension.StopAsync();
        loaded.IsActive = false;
        
        return true;
    }
    
    /// <summary>
    /// Get all loaded extensions
    /// </summary>
    public IReadOnlyList<ExtensionInfo> GetExtensions()
    {
        return _extensions.Values.Select(l => new ExtensionInfo
        {
            Id = l.Extension.Id,
            Name = l.Extension.Name,
            Version = l.Extension.Version,
            Description = l.Extension.Description,
            IsActive = l.IsActive,
            LoadedAt = l.LoadedAt
        }).ToList();
    }
    
    /// <summary>
    /// Get an extension by ID
    /// </summary>
    public IExtension? GetExtension(string extensionId)
    {
        return _extensions.TryGetValue(extensionId, out var loaded) ? loaded.Extension : null;
    }
}

/// <summary>
/// Loaded extension context
/// </summary>
internal class LoadedExtension
{
    public required IExtension Extension { get; init; }
    public required string AssemblyPath { get; init; }
    public required DateTime LoadedAt { get; init; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Extension information
/// </summary>
public record ExtensionInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Description { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime LoadedAt { get; init; }
}
