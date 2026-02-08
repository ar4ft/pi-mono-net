using Pi.AI;
using Xunit;

namespace Pi.AI.Tests;

public class ModelRegistryTests
{
    [Fact]
    public void RegisterModel_AddsModelToRegistry()
    {
        // Arrange
        var registry = new ModelRegistry();
        var model = CreateTestModel();

        // Act
        registry.RegisterModel(model);

        // Assert
        var retrieved = registry.GetModel(model.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(model.Id, retrieved.Id);
        Assert.Equal(model.Name, retrieved.Name);
    }

    [Fact]
    public void GetModel_ReturnsNullForNonExistentModel()
    {
        // Arrange
        var registry = new ModelRegistry();

        // Act
        var result = registry.GetModel("non-existent-model");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetModelsForProvider_ReturnsCorrectModels()
    {
        // Arrange
        var registry = new ModelRegistry();
        var model1 = CreateTestModel("model1", "openai");
        var model2 = CreateTestModel("model2", "openai");
        var model3 = CreateTestModel("model3", "anthropic");

        registry.RegisterModel(model1);
        registry.RegisterModel(model2);
        registry.RegisterModel(model3);

        // Act
        var openaiModels = registry.GetModelsForProvider("openai");
        var anthropicModels = registry.GetModelsForProvider("anthropic");

        // Assert
        Assert.Equal(2, openaiModels.Count);
        Assert.Single(anthropicModels);
        Assert.Contains(openaiModels, m => m.Id == "model1");
        Assert.Contains(openaiModels, m => m.Id == "model2");
    }

    [Fact]
    public void HasModel_ReturnsTrueForExistingModel()
    {
        // Arrange
        var registry = new ModelRegistry();
        var model = CreateTestModel();
        registry.RegisterModel(model);

        // Act
        var exists = registry.HasModel(model.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void HasModel_ReturnsFalseForNonExistentModel()
    {
        // Arrange
        var registry = new ModelRegistry();

        // Act
        var exists = registry.HasModel("non-existent");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void GetAllModels_ReturnsAllRegisteredModels()
    {
        // Arrange
        var registry = new ModelRegistry();
        var model1 = CreateTestModel("model1");
        var model2 = CreateTestModel("model2");
        var model3 = CreateTestModel("model3");

        registry.RegisterModel(model1);
        registry.RegisterModel(model2);
        registry.RegisterModel(model3);

        // Act
        var allModels = registry.GetAllModels();

        // Assert
        Assert.Equal(3, allModels.Count);
    }

    private static Model CreateTestModel(string id = "test-model", string provider = "test-provider")
    {
        return new Model
        {
            Id = id,
            Name = $"Test Model {id}",
            Api = "openai-completions",
            Provider = provider,
            BaseUrl = "https://api.example.com",
            Reasoning = false,
            Input = new List<string> { "text" },
            Cost = new ModelCost
            {
                Input = 0.01,
                Output = 0.02,
                CacheRead = 0.001,
                CacheWrite = 0.002
            },
            ContextWindow = 8000,
            MaxTokens = 4000
        };
    }
}

public class TypesTests
{
    [Fact]
    public void TextContent_CreatesCorrectly()
    {
        // Arrange & Act
        var content = new TextContent
        {
            Text = "Hello, World!",
            TextSignature = "sig123"
        };

        // Assert
        Assert.Equal("text", content.Type);
        Assert.Equal("Hello, World!", content.Text);
        Assert.Equal("sig123", content.TextSignature);
    }

    [Fact]
    public void ThinkingContent_CreatesCorrectly()
    {
        // Arrange & Act
        var content = new ThinkingContent
        {
            Thinking = "Let me think...",
            ThinkingSignature = "think123"
        };

        // Assert
        Assert.Equal("thinking", content.Type);
        Assert.Equal("Let me think...", content.Thinking);
        Assert.Equal("think123", content.ThinkingSignature);
    }

    [Fact]
    public void ImageContent_CreatesCorrectly()
    {
        // Arrange & Act
        var content = new ImageContent
        {
            Data = "base64encodeddata",
            MimeType = "image/png"
        };

        // Assert
        Assert.Equal("image", content.Type);
        Assert.Equal("base64encodeddata", content.Data);
        Assert.Equal("image/png", content.MimeType);
    }

    [Fact]
    public void ToolCall_CreatesCorrectly()
    {
        // Arrange & Act
        var toolCall = new ToolCall
        {
            Id = "call123",
            Name = "test_tool",
            Arguments = new Dictionary<string, object>
            {
                ["arg1"] = "value1",
                ["arg2"] = 42
            }
        };

        // Assert
        Assert.Equal("toolCall", toolCall.Type);
        Assert.Equal("call123", toolCall.Id);
        Assert.Equal("test_tool", toolCall.Name);
        Assert.Equal(2, toolCall.Arguments.Count);
    }

    [Fact]
    public void Usage_CalculatesTotalTokens()
    {
        // Arrange & Act
        var usage = new Usage
        {
            Input = 100,
            Output = 50,
            CacheRead = 20,
            CacheWrite = 10,
            TotalTokens = 180,
            Cost = new UsageCost
            {
                Input = 0.001,
                Output = 0.002,
                CacheRead = 0.0001,
                CacheWrite = 0.0002,
                Total = 0.0033
            }
        };

        // Assert
        Assert.Equal(180, usage.TotalTokens);
        Assert.Equal(0.0033, usage.Cost.Total);
    }

    [Fact]
    public void Model_AllowsOpenAICompatOverrides()
    {
        // Arrange
        var compat = new OpenAICompletionsCompat
        {
            SupportsStore = true,
            SupportsDeveloperRole = false,
            MaxTokensField = "max_completion_tokens",
            ThinkingFormat = "openai",
            OpenRouterRouting = new OpenRouterRouting
            {
                Only = new List<string> { "anthropic" },
                Order = new List<string> { "openai" }
            },
            VercelGatewayRouting = new VercelGatewayRouting
            {
                Only = new List<string> { "bedrock" }
            }
        };

        // Act
        var model = new Model
        {
            Id = "test-model",
            Name = "Test Model",
            Api = "openai-completions",
            Provider = "openai",
            BaseUrl = "https://api.example.com",
            Reasoning = false,
            Input = new List<string> { "text" },
            Cost = new ModelCost
            {
                Input = 0.01,
                Output = 0.02,
                CacheRead = 0,
                CacheWrite = 0
            },
            ContextWindow = 8000,
            MaxTokens = 4000,
            Compat = compat
        };

        // Assert
        Assert.True(model.Compat?.SupportsStore);
        Assert.False(model.Compat?.SupportsDeveloperRole);
        Assert.Equal("max_completion_tokens", model.Compat?.MaxTokensField);
        Assert.Equal("openai", model.Compat?.ThinkingFormat);
        Assert.Contains("anthropic", model.Compat?.OpenRouterRouting?.Only ?? new List<string>());
        Assert.Contains("bedrock", model.Compat?.VercelGatewayRouting?.Only ?? new List<string>());
    }
}
