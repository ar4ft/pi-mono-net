using System.Collections.Generic;

namespace Pi.AI;

/// <summary>
/// Registry for managing LLM models and their configurations
/// </summary>
public class ModelRegistry
{
    private readonly Dictionary<string, Model> _models = new();
    private readonly Dictionary<string, List<Model>> _providerModels = new();

    /// <summary>
    /// Register a model
    /// </summary>
    public void RegisterModel(Model model)
    {
        _models[model.Id] = model;
        
        if (!_providerModels.ContainsKey(model.Provider))
        {
            _providerModels[model.Provider] = new List<Model>();
        }
        _providerModels[model.Provider].Add(model);
    }

    /// <summary>
    /// Get a model by ID
    /// </summary>
    public Model? GetModel(string modelId)
    {
        return _models.TryGetValue(modelId, out var model) ? model : null;
    }

    /// <summary>
    /// Get all models for a provider
    /// </summary>
    public IReadOnlyList<Model> GetModelsForProvider(string provider)
    {
        return _providerModels.TryGetValue(provider, out var models) 
            ? models 
            : Array.Empty<Model>();
    }

    /// <summary>
    /// Get all registered models
    /// </summary>
    public IReadOnlyCollection<Model> GetAllModels()
    {
        return _models.Values;
    }

    /// <summary>
    /// Check if a model exists
    /// </summary>
    public bool HasModel(string modelId)
    {
        return _models.ContainsKey(modelId);
    }
}
