using Pi.Channels;

namespace Pi.AutoReply;

/// <summary>
/// Auto-reply rule for automatic responses
/// </summary>
public record AutoReplyRule
{
    /// <summary>
    /// Rule ID
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Rule name
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Rule description
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// Whether the rule is enabled
    /// </summary>
    public bool Enabled { get; init; } = true;
    
    /// <summary>
    /// Priority (higher = evaluated first)
    /// </summary>
    public int Priority { get; init; } = 0;
    
    /// <summary>
    /// Conditions that must match for the rule to trigger
    /// </summary>
    public List<AutoReplyCondition> Conditions { get; init; } = new();
    
    /// <summary>
    /// Response template
    /// </summary>
    public required string ResponseTemplate { get; init; }
    
    /// <summary>
    /// Maximum times this rule can trigger per session (0 = unlimited)
    /// </summary>
    public int MaxTriggers { get; init; } = 0;
    
    /// <summary>
    /// Cooldown period in seconds before rule can trigger again
    /// </summary>
    public int CooldownSeconds { get; init; } = 0;
}

/// <summary>
/// Condition for auto-reply rules
/// </summary>
public record AutoReplyCondition
{
    /// <summary>
    /// Field to check (content, sender, channel, etc.)
    /// </summary>
    public required string Field { get; init; }
    
    /// <summary>
    /// Operator (contains, equals, startsWith, endsWith, matches)
    /// </summary>
    public required string Operator { get; init; }
    
    /// <summary>
    /// Value to compare against
    /// </summary>
    public required string Value { get; init; }
    
    /// <summary>
    /// Case sensitive comparison
    /// </summary>
    public bool CaseSensitive { get; init; } = false;
}

/// <summary>
/// Auto-reply service for automatic message responses
/// </summary>
public class AutoReplyService
{
    private readonly List<AutoReplyRule> _rules = new();
    private readonly Dictionary<string, DateTime> _lastTriggers = new();
    private readonly Dictionary<string, int> _triggerCounts = new();
    
    /// <summary>
    /// Add a rule
    /// </summary>
    public void AddRule(AutoReplyRule rule)
    {
        _rules.Add(rule);
        _rules.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }
    
    /// <summary>
    /// Remove a rule
    /// </summary>
    public bool RemoveRule(string ruleId)
    {
        return _rules.RemoveAll(r => r.Id == ruleId) > 0;
    }
    
    /// <summary>
    /// Get all rules
    /// </summary>
    public IReadOnlyList<AutoReplyRule> GetRules() => _rules.AsReadOnly();
    
    /// <summary>
    /// Process a message and generate auto-reply if applicable
    /// </summary>
    public AutoReplyResult? ProcessMessage(ChannelMessage message)
    {
        foreach (var rule in _rules.Where(r => r.Enabled))
        {
            if (ShouldTrigger(rule, message))
            {
                var response = GenerateResponse(rule, message);
                RecordTrigger(rule, message.SessionKey);
                
                return new AutoReplyResult
                {
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    Response = response
                };
            }
        }
        
        return null;
    }
    
    private bool ShouldTrigger(AutoReplyRule rule, ChannelMessage message)
    {
        // Check cooldown
        var key = $"{rule.Id}:{message.SessionKey}";
        if (_lastTriggers.TryGetValue(key, out var lastTrigger))
        {
            if ((DateTime.UtcNow - lastTrigger).TotalSeconds < rule.CooldownSeconds)
                return false;
        }
        
        // Check max triggers
        if (rule.MaxTriggers > 0 && _triggerCounts.GetValueOrDefault(key) >= rule.MaxTriggers)
            return false;
        
        // Check all conditions
        return rule.Conditions.All(c => EvaluateCondition(c, message));
    }
    
    private bool EvaluateCondition(AutoReplyCondition condition, ChannelMessage message)
    {
        var fieldValue = condition.Field.ToLower() switch
        {
            "content" => message.Content,
            "sender" => message.SenderId,
            "channel" => message.ChannelType,
            "sessionkey" => message.SessionKey,
            _ => ""
        };
        
        var comparison = condition.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        
        return condition.Operator.ToLower() switch
        {
            "contains" => fieldValue.Contains(condition.Value, comparison),
            "equals" => fieldValue.Equals(condition.Value, comparison),
            "startswith" => fieldValue.StartsWith(condition.Value, comparison),
            "endswith" => fieldValue.EndsWith(condition.Value, comparison),
            "matches" => System.Text.RegularExpressions.Regex.IsMatch(fieldValue, condition.Value),
            _ => false
        };
    }
    
    private string GenerateResponse(AutoReplyRule rule, ChannelMessage message)
    {
        // Simple template variable replacement
        return rule.ResponseTemplate
            .Replace("{sender}", message.SenderName ?? message.SenderId)
            .Replace("{content}", message.Content)
            .Replace("{time}", DateTime.Now.ToString("HH:mm"));
    }
    
    private void RecordTrigger(AutoReplyRule rule, string sessionKey)
    {
        var key = $"{rule.Id}:{sessionKey}";
        _lastTriggers[key] = DateTime.UtcNow;
        _triggerCounts[key] = _triggerCounts.GetValueOrDefault(key) + 1;
    }
}

/// <summary>
/// Auto-reply result
/// </summary>
public record AutoReplyResult
{
    public required string RuleId { get; init; }
    public required string RuleName { get; init; }
    public required string Response { get; init; }
}
