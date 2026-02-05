# Webhook/API Integration Status

## Question: Do we have API-based (Webhooks) for external system integrations currently?

**Answer: NO - Not currently implemented**

---

## Current Status Summary

| Feature | Status | Notes |
|---------|--------|-------|
| Webhook Endpoints | ❌ Not Implemented | No HTTP endpoints for webhooks |
| REST API | ❌ Not Implemented | No API server |
| External System Integration | ❌ Not Implemented | No webhook handlers |
| Documentation | ⚠️ Examples Only | Conceptual examples, not real code |

---

## What Exists

### 1. Documentation (Examples Only)

The scaffolding guide (`docs/guides/scaffolding-specialized-agents.md`) includes webhook examples:

```csharp
// This is EXAMPLE CODE - NOT IMPLEMENTED
app.MapPost("/webhook/trade-signal", async (TradeSignal signal) =>
{
    await tradingAgent.Prompt($"Analyze trade signal: {signal.Symbol}");
});
```

**Status**: This is conceptual documentation showing what COULD be built, not actual working code.

### 2. Pi.WebUI (Shell Only)

Location: `src/Pi.WebUI/`

**What it is**:
- ASP.NET Core Razor Pages application
- Basic web server setup
- Placeholder UI pages

**What it's NOT**:
- ❌ No API endpoints
- ❌ No webhook receivers
- ❌ No external system integrations
- ❌ Just a shell for future web UI

```csharp
// Current Pi.WebUI/Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
var app = builder.Build();
app.MapRazorPages();
app.Run();
```

No webhook routes, no API controllers.

### 3. Pi.Gateway (Internal Message Broker)

Location: `src/Pi.Gateway/`

**What it is**:
- Internal message routing between channels and agents
- Session management
- Channel registry
- Event bus

**What it's NOT**:
- ❌ Not an HTTP server
- ❌ No webhook endpoints
- ❌ No external API access
- Works with internal channels (iMessage, etc.), not external webhooks

**Architecture:**
```
Gateway (Internal)
  ↓
Channels (iMessage, Telegram, etc.)
  ↓
Agents
```

**Missing:**
```
External System → Webhook → Gateway → Agents
```

---

## What's Missing

To support webhooks/API integration, the following would need to be implemented:

### 1. HTTP API Server

**Required Components:**
- ASP.NET Core Web API project
- REST API controllers
- Webhook endpoint routes
- Request/response handling
- Authentication/authorization

**Example:**
```csharp
// What WOULD need to be created
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    [HttpPost("github")]
    public async Task<IActionResult> HandleGitHub([FromBody] GitHubWebhook webhook)
    {
        // Route to agent
        await _gateway.RouteWebhookAsync(webhook);
        return Ok();
    }
    
    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripe([FromBody] StripeWebhook webhook)
    {
        // Verify signature
        // Route to agent
        return Ok();
    }
}
```

### 2. Webhook Processing Layer

**Required Components:**
- Webhook signature verification
- Payload validation
- Event type routing
- Async processing queue
- Retry logic
- Error handling

### 3. Gateway Integration

**Required Changes:**
- Add HTTP endpoint registration to Gateway
- Route webhook events to appropriate agents
- Handle webhook responses
- Manage webhook sessions

### 4. Platform Adapters

**Common Integrations:**
- GitHub webhooks
- Stripe webhooks
- Slack webhooks (different from Slack bot)
- Twilio webhooks
- Generic webhook handler

---

## Current Trigger Mechanisms

What DOES work currently:

### ✅ Time-Based Triggers (Pi.Scheduler)

```csharp
var scheduler = new CronScheduler();
scheduler.AddJob(new ScheduledJob
{
    CronExpression = "*/15 * * * *", // Every 15 minutes
    Action = async (ct) => { /* agent work */ }
});
```

**Status**: ✅ Fully implemented

### ✅ File-Based Triggers (FileSystemWatcher)

```csharp
var watcher = new FileSystemWatcher("./inbox");
watcher.Created += async (s, e) =>
{
    // Process new file
    await agent.ProcessFileAsync(e.FullPath);
};
```

**Status**: ✅ Available via .NET framework

### ✅ Channel-Based Triggers (Pi.Gateway + Channels)

```csharp
// iMessage channel
var channel = new IMessageChannel(config);
await gateway.RegisterChannelAsync(channel);
```

**Status**: ✅ Fully implemented for iMessage

### ❌ API-Based Triggers (Webhooks)

**Status**: ❌ NOT IMPLEMENTED

---

## Implementation Roadmap

If webhook support is desired, here's the recommended approach:

### Phase 1: Basic Infrastructure (1-2 weeks)

**Create Pi.Webhooks Project:**
```
src/Pi.Webhooks/
├── Pi.Webhooks.csproj
├── Program.cs (API server)
├── Controllers/
│   └── WebhookController.cs
├── Services/
│   ├── WebhookService.cs
│   └── SignatureVerifier.cs
└── Models/
    └── WebhookPayload.cs
```

**Key Features:**
- ASP.NET Core Web API
- Basic webhook endpoints
- Signature verification
- Integration with Gateway

### Phase 2: Platform Adapters (1 week per platform)

**Create specific adapters:**
- GitHub webhook adapter
- Stripe webhook adapter
- Generic webhook adapter
- Custom webhook support

### Phase 3: Advanced Features (1-2 weeks)

**Add capabilities:**
- Webhook registration UI
- Event replay
- Webhook testing tools
- Monitoring/logging
- Rate limiting
- Queue management

### Phase 4: Documentation (1 week)

**Update docs:**
- Webhook configuration guide
- Security best practices
- Platform-specific guides
- Troubleshooting

---

## Comparison: What Works vs What Doesn't

| Trigger Type | Implementation | Status | Location |
|--------------|----------------|--------|----------|
| **Time-based** | Cron scheduler | ✅ Works | Pi.Scheduler |
| **File-based** | FileSystemWatcher | ✅ Works | .NET Framework |
| **Message-based** | Gateway + Channels | ✅ Works | Pi.Gateway, Pi.IMessage |
| **API-based** | Webhooks | ❌ Missing | Not created |

---

## Why Webhooks Aren't Implemented Yet

Based on the project history and priorities:

1. **Focus on Core Functionality**: Phases 1-5 focused on:
   - Agent runtime
   - LLM integration
   - Tools and skills
   - Sessions and settings
   - Core channels (iMessage)

2. **Gateway Built for Channels**: The Gateway was designed for internal messaging channels (iMessage, Telegram, etc.), not external HTTP webhooks.

3. **WebUI is Shell Only**: Pi.WebUI was created as a placeholder for future web interface, not as an API server.

4. **Documentation Got Ahead**: The scaffolding guide included webhook examples as "what could be built," but implementation didn't follow yet.

---

## How to Add Webhook Support

If you want to add webhooks, here's the quickest path:

### Option 1: Extend Pi.WebUI

Add API controllers to existing Pi.WebUI project:

```csharp
// Add to Pi.WebUI/Program.cs
builder.Services.AddControllers();
app.MapControllers();

// Create Pi.WebUI/Controllers/WebhookController.cs
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly GatewayService _gateway;
    
    [HttpPost("{platform}")]
    public async Task<IActionResult> HandleWebhook(string platform, [FromBody] JsonDocument payload)
    {
        // Route to gateway
        return Ok();
    }
}
```

### Option 2: Create New Pi.Webhooks Project (Recommended)

Better separation of concerns:
- Dedicated webhook handling
- Independent scaling
- Clear responsibility
- Easier to secure

---

## Conclusion

**To directly answer the question:**

> Do we have API-based (Webhooks): External system integrations currently?

**NO.** The system does not currently have webhook/API integration capability.

**What exists:**
- ✅ Time-based triggers (Scheduler)
- ✅ File-based triggers (FileSystemWatcher)
- ✅ Channel-based triggers (Gateway + iMessage)
- ❌ API-based triggers (Webhooks) - NOT IMPLEMENTED

**What would be needed:**
1. Create Pi.Webhooks project (ASP.NET Core Web API)
2. Implement webhook controllers
3. Add signature verification
4. Integrate with Gateway
5. Create platform adapters
6. Update documentation with real examples

**Estimated effort to add:**
- Basic webhook support: 2-3 weeks
- Platform-specific adapters: 1 week each
- Full production-ready: 4-6 weeks total

---

## Recommendations

**For users who need external system integration NOW:**

1. **Use file-based triggers**: Have external systems write files that trigger agents
2. **Use channel-based triggers**: Set up a messaging channel (if applicable)
3. **Use time-based polling**: Agent checks external API periodically

**For future development:**

Consider adding Pi.Webhooks as the next major feature after Phase 5, as it would:
- Enable integration with thousands of external services
- Support modern event-driven architectures
- Complete the trigger mechanism suite
- Open up new use cases (CI/CD, payment processing, etc.)

---

*Last Updated: 2026-02-05*
*Status: No webhook support currently implemented*
