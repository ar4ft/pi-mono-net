# Scaffolding Specialized Agents

A comprehensive guide to creating base agents that can be personalized into specific tasks like trading, accounting, or document review.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Core Components](#core-components)
3. [Personalization Strategy](#personalization-strategy)
4. [Trigger Mechanisms](#trigger-mechanisms)
5. [Concrete Examples](#concrete-examples)
6. [Best Practices](#best-practices)
7. [Directory Structure](#directory-structure)

---

## Architecture Overview

The Pi-Mono-Net system provides a flexible architecture for creating specialized agents:

```
Base Agent Runtime (Pi.Agent)
    ↓
Personalization Layer (Skills, SOUL.md, Tools)
    ↓
Trigger System (Scheduler, File Watcher, Gateway Events)
    ↓
Specialized Agent (Trading, Accounting, Document Review)
```

### Key Principle

**One Base, Many Specializations**: Instead of creating separate agent codebases, you use the same base agent runtime (Pi.Agent) and personalize it through:
- Skills (task-specific knowledge)
- SOUL.md (personality and behavior)
- Tools (task-specific capabilities)
- Triggers (activation conditions)
- Settings (configuration)

---

## Core Components

### 1. Base Agent (Pi.Agent)

The foundation that provides:
- LLM integration (OpenAI, GitHub Copilot)
- Tool execution
- Event streaming
- State management
- Message handling

**You don't modify this** - it's the common runtime for all agents.

### 2. Skills System (.agents/skills/)

Task-specific knowledge and instructions stored as SKILL.md files:

```
.agents/skills/
├── trading-analysis/SKILL.md    # For trading agent
├── accounting-rules/SKILL.md    # For accounting agent
└── document-review/SKILL.md     # For document review agent
```

**Purpose**: Teaches the agent HOW to do specific tasks without coding.

### 3. SOUL.md (.agents/SOUL.md)

Defines agent personality, behavior, and boundaries:

```markdown
---
summary: "Trading Agent Personality"
---

# SOUL.md - Trading Agent

## Core Truths
- Be data-driven and analytical
- Never execute trades without explicit confirmation
- Risk management is paramount
- Always show your reasoning

## Boundaries
- Never trade outside authorized hours
- Never exceed position size limits
- Always validate data before decisions
```

**Purpose**: Shapes WHO the agent is for a specific role.

### 4. Tools (Pi.CodingAgent/Tools/)

Built-in tools available to all agents:
- `bash` - Execute shell commands
- `read` - Read files
- `write` - Write files
- `edit` - Edit files
- `grep` - Search content
- `find` - Find files
- `ls` - List directories

Plus specialized tools:
- `browser_navigate` - Web automation
- `browser_screenshot` - Capture screenshots
- `browser_get_text` - Extract web content

**Purpose**: Gives agents the ability to DO things.

### 5. Settings (Settings/UserSettings.cs)

Per-agent configuration:

```csharp
{
  "agentName": "trading-agent",
  "defaultModel": "gpt-4",
  "heartbeatInterval": "15m",
  "customSettings": {
    "tradingHours": "09:30-16:00",
    "maxPositionSize": 10000,
    "riskLimit": 0.02
  }
}
```

**Purpose**: Configurable parameters for each specialized agent.

### 6. Sessions (Session/SessionManager.cs)

Persistent conversations and state:
- Stores conversation history
- Saves agent decisions
- Maintains context across restarts

**Purpose**: Remembers what the agent has done.

---

## Personalization Strategy

### Step 1: Define the Agent's Purpose

Start with clear answers:
1. **What task** does this agent perform? (trading, accounting, document review)
2. **When** should it activate? (time-based, event-based, file-based)
3. **What data** does it need access to? (files, APIs, databases)
4. **What actions** can it take? (read-only, advisory, autonomous)
5. **What constraints** must it respect? (risk limits, approval requirements)

### Step 2: Create Skill Files

For each specialized capability, create a SKILL.md:

**Trading Analysis Skill** (.agents/skills/trading-analysis/SKILL.md):
```markdown
---
name: trading-analysis
description: Technical and fundamental analysis for trading decisions
---

# Trading Analysis

When analyzing trading opportunities:

## Technical Analysis
1. Check key indicators: RSI, MACD, Moving Averages
2. Identify support/resistance levels
3. Analyze volume patterns
4. Consider trend direction

## Fundamental Analysis
1. Review recent news and earnings
2. Check sector performance
3. Analyze key ratios (P/E, P/B, etc.)
4. Assess market conditions

## Risk Assessment
1. Calculate position size (max 2% account risk)
2. Set stop-loss at technical support
3. Define profit targets
4. Consider correlation with existing positions

## Output Format
Provide:
- Trade direction (long/short/neutral)
- Entry price
- Stop loss
- Profit targets
- Risk/reward ratio
- Confidence level
- Key reasoning
```

**Accounting Rules Skill** (.agents/skills/accounting-rules/SKILL.md):
```markdown
---
name: accounting-rules
description: GAAP accounting principles and transaction categorization
---

# Accounting Rules

When processing transactions:

## Transaction Categorization
1. Identify transaction type (revenue, expense, asset, liability)
2. Apply double-entry bookkeeping
3. Assign proper GL codes
4. Note tax implications

## Revenue Recognition
- Follow ASC 606 principles
- Identify performance obligations
- Determine transaction price
- Allocate to obligations
- Recognize when satisfied

## Expense Matching
- Match expenses to revenues
- Apply accrual accounting
- Defer when appropriate
- Amortize over useful life

## Review Checklist
- Verify amounts and dates
- Check supporting documentation
- Confirm proper approval
- Validate account coding
- Flag unusual items for review
```

**Document Review Skill** (.agents/skills/document-review/SKILL.md):
```markdown
---
name: document-review
description: Legal and compliance document review procedures
---

# Document Review

When reviewing documents:

## Initial Assessment
1. Identify document type (contract, agreement, memo, etc.)
2. Note effective dates and parties
3. Scan for critical terms
4. Flag urgent items

## Detailed Review
1. **Terms & Conditions**
   - Payment terms
   - Delivery obligations
   - Performance metrics
   - Termination clauses

2. **Risk Areas**
   - Liability limitations
   - Indemnification
   - Confidentiality
   - Non-compete clauses

3. **Compliance**
   - Regulatory requirements
   - Internal policies
   - Industry standards
   - Legal jurisdiction

## Output Format
Provide:
- Document summary
- Key terms extracted
- Risk assessment
- Required actions
- Recommended changes
- Priority level
```

### Step 3: Configure SOUL.md

Define the agent's personality for the specific role:

```markdown
---
summary: "Trading Agent - Conservative Risk Profile"
---

# SOUL.md - Trading Agent

## Core Truths
- **Data-driven decisions only** - No gut feelings, only analysis
- **Risk management first** - Preservation of capital is paramount
- **Transparency always** - Show all calculations and reasoning
- **Human approval required** - Never execute trades autonomously

## Boundaries
- Only trade during market hours (09:30-16:00 ET)
- Never exceed 2% account risk per trade
- Never trade without stop-loss
- No options or derivatives without explicit approval
- Flag any anomalies in data

## Vibe
Be analytical, precise, and cautious. You're a risk-conscious trading assistant, not a gambler. When uncertain, err on the side of caution.

## Continuity
Track:
- Open positions and their performance
- Historical recommendations and outcomes
- Market conditions and trends
- Risk metrics and drawdowns
```

### Step 4: Add Custom Tools (Optional)

If built-in tools aren't enough, create task-specific tools:

**Example: Market Data Tool**
```csharp
public class MarketDataTool : ITool
{
    public string Name => "get_market_data";
    public string Description => "Fetch real-time or historical market data";
    
    public object Parameters => new
    {
        type = "object",
        properties = new
        {
            symbol = new { type = "string", description = "Stock symbol" },
            timeframe = new { type = "string", description = "1d, 1h, 15m, etc." },
            period = new { type = "string", description = "1mo, 3mo, 1y, etc." }
        },
        required = new[] { "symbol" }
    };
    
    public async Task<ToolResult> ExecuteAsync(...)
    {
        // Fetch from API (Yahoo Finance, Alpha Vantage, etc.)
        // Return formatted data
    }
}
```

**Example: Accounting System Tool**
```csharp
public class QuickBooksIntegrationTool : ITool
{
    public string Name => "quickbooks_query";
    public string Description => "Query QuickBooks for transactions and reports";
    
    // Implementation connects to QuickBooks API
}
```

---

## Trigger Mechanisms

### 1. Time-Based Triggers (Scheduler)

Use `Pi.Scheduler` for periodic tasks:

```csharp
var scheduler = new CronScheduler();

// Trading Agent: Check markets every 15 minutes during trading hours
scheduler.AddJob(new ScheduledJob
{
    Id = "market-check",
    Name = "Market Monitoring",
    CronExpression = "*/15 9-16 * * 1-5", // Every 15 min, 9am-4pm, Mon-Fri
    Action = async (ct) =>
    {
        await agent.Prompt("Check current market conditions and open positions");
    }
});

// Accounting Agent: Daily reconciliation at 6 PM
scheduler.AddJob(new ScheduledJob
{
    Id = "daily-reconciliation",
    Name = "Daily Account Reconciliation",
    CronExpression = "0 18 * * 1-5", // 6 PM every weekday
    Action = async (ct) =>
    {
        await agent.Prompt("Perform daily account reconciliation");
    }
});

// Document Review: Check for new documents every hour
scheduler.AddJob(new ScheduledJob
{
    Id = "document-scan",
    Name = "Scan for New Documents",
    CronExpression = "0 * * * *", // Every hour
    Action = async (ct) =>
    {
        await agent.Prompt("Check inbox for new documents requiring review");
    }
});

scheduler.Start();
```

### 2. File-Based Triggers (File System Watcher)

Monitor directories for new files:

```csharp
var watcher = new FileSystemWatcher("/path/to/watch");
watcher.Filter = "*.csv"; // Or *.pdf, *.xlsx, etc.
watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;

watcher.Created += async (sender, e) =>
{
    // New file detected - trigger agent
    await agent.Prompt($"Process new file: {e.FullPath}");
};

watcher.EnableRaisingEvents = true;
```

**Use Cases:**
- **Trading**: Watch for new market data CSV exports
- **Accounting**: Monitor for uploaded receipts or invoices
- **Document Review**: Watch inbox folder for new contracts

### 3. Event-Based Triggers (Gateway/Channels)

Respond to messages from various channels:

```csharp
// Set up Gateway with channels
var gateway = new GatewayService();

// Email channel - for document review requests
var emailChannel = new EmailChannel();
gateway.RegisterChannel(emailChannel);

// Slack channel - for accounting queries
var slackChannel = new SlackChannel();
gateway.RegisterChannel(slackChannel);

// Handle incoming messages
gateway.MessageReceived += async (sender, message) =>
{
    if (message.Channel == "email" && message.Content.Contains("contract review"))
    {
        // Trigger document review agent
        await documentReviewAgent.Prompt($"Review document: {message.Content}");
    }
    else if (message.Channel == "slack" && message.Content.Contains("expense"))
    {
        // Trigger accounting agent
        await accountingAgent.Prompt($"Process expense query: {message.Content}");
    }
};
```

### 4. API-Based Triggers (Webhooks)

Respond to external API events:

```csharp
// Example: Trading platform webhook
app.MapPost("/webhook/trade-signal", async (TradeSignal signal) =>
{
    await tradingAgent.Prompt($"Analyze trade signal: {signal.Symbol} {signal.Action}");
});

// Example: Accounting platform webhook
app.MapPost("/webhook/new-transaction", async (Transaction tx) =>
{
    await accountingAgent.Prompt($"Categorize transaction: {tx.Amount} {tx.Description}");
});
```

### 5. Heartbeat Monitoring

Add HEARTBEAT.md to skills for proactive monitoring:

**Trading Heartbeat** (.agents/skills/trading-analysis/HEARTBEAT.md):
```markdown
---
every: "15m"
target: "last"
---

# Trading Heartbeat

Every 15 minutes during market hours, check:

- Are there any open positions approaching stop-loss?
- Have any price alerts been triggered?
- Are there unusual volume spikes?
- Is the account within risk limits?
- Are there any pending orders?

If any issues: ALERT immediately.
Otherwise: HEARTBEAT_OK
```

---

## Concrete Examples

### Example 1: Trading Agent

**Purpose**: Monitor markets, analyze opportunities, provide trade recommendations

**Directory Structure**:
```
trading-agent/
├── .agents/
│   ├── SOUL.md                           # Trading personality
│   ├── HEARTBEAT.md                      # Market monitoring
│   └── skills/
│       ├── trading-analysis/
│       │   ├── SKILL.md                  # Analysis methodology
│       │   └── HEARTBEAT.md              # Position monitoring
│       ├── risk-management/
│       │   └── SKILL.md                  # Risk rules
│       └── market-data/
│           └── SKILL.md                  # Data interpretation
├── settings.json                         # Trading parameters
└── Program.cs                            # Agent initialization
```

**settings.json**:
```json
{
  "agentName": "trading-agent",
  "defaultModel": "gpt-4",
  "heartbeatInterval": "15m",
  "customSettings": {
    "tradingHours": "09:30-16:00",
    "timezone": "America/New_York",
    "maxPositionSize": 10000,
    "maxAccountRisk": 0.02,
    "brokerApi": "alpaca",
    "watchlist": ["AAPL", "MSFT", "GOOGL", "AMZN"],
    "strategies": ["momentum", "mean-reversion"],
    "requireHumanApproval": true
  }
}
```

**Triggers**:
1. **Time-based**: Check markets every 15 minutes during trading hours
2. **Heartbeat**: Monitor open positions every 15 minutes
3. **File-based**: Watch for new market data exports
4. **Event-based**: React to price alerts or news

**Workflow**:
```
09:30 → Agent starts → Load watchlist
09:45 → Scheduled check → Analyze market open
10:00 → Scheduled check → Screen for opportunities
10:15 → Price alert triggered → Analyze AAPL breakout
10:20 → Present recommendation → Await human approval
... (continues every 15 minutes)
16:00 → Market close → Summary report
```

### Example 2: Accounting Agent

**Purpose**: Process transactions, categorize expenses, generate reports

**Directory Structure**:
```
accounting-agent/
├── .agents/
│   ├── SOUL.md                           # Accounting personality
│   ├── HEARTBEAT.md                      # Daily reconciliation
│   └── skills/
│       ├── accounting-rules/
│       │   ├── SKILL.md                  # GAAP principles
│       │   └── HEARTBEAT.md              # Compliance checks
│       ├── transaction-processing/
│       │   └── SKILL.md                  # Processing workflow
│       └── tax-compliance/
│           └── SKILL.md                  # Tax rules
├── settings.json                         # Accounting configuration
└── Program.cs                            # Agent initialization
```

**settings.json**:
```json
{
  "agentName": "accounting-agent",
  "defaultModel": "gpt-4",
  "heartbeatInterval": "1d",
  "customSettings": {
    "fiscalYearEnd": "12-31",
    "accountingStandard": "GAAP",
    "baseCurrency": "USD",
    "chartOfAccounts": "/config/chart-of-accounts.json",
    "approvalThreshold": 1000,
    "autoCategorizationEnabled": true,
    "requireReview": ["vendor-payments", "payroll", "fixed-assets"]
  }
}
```

**Triggers**:
1. **Time-based**: Daily reconciliation at 6 PM
2. **File-based**: Monitor uploads folder for receipts/invoices
3. **Event-based**: Email notifications for approval requests
4. **Heartbeat**: Daily compliance check

**Workflow**:
```
Morning → Heartbeat check → Review yesterday's transactions
10:00 → New file detected → Process receipt.pdf
10:02 → Categorize transaction → Auto-assign GL code
10:05 → Amount > $1000 → Flag for approval
12:00 → Email received → "Expense report submitted"
12:01 → Process expense report → Validate & categorize
18:00 → Daily trigger → Reconciliation report
18:30 → Generate summary → Email to accounting team
```

### Example 3: Document Review Agent

**Purpose**: Review contracts, NDAs, agreements for risks and compliance

**Directory Structure**:
```
document-review-agent/
├── .agents/
│   ├── SOUL.md                           # Legal review personality
│   ├── HEARTBEAT.md                      # Document backlog check
│   └── skills/
│       ├── document-review/
│       │   ├── SKILL.md                  # Review methodology
│       │   └── HEARTBEAT.md              # SLA monitoring
│       ├── contract-analysis/
│       │   └── SKILL.md                  # Contract-specific rules
│       └── risk-assessment/
│           └── SKILL.md                  # Risk framework
├── settings.json                         # Review configuration
└── Program.cs                            # Agent initialization
```

**settings.json**:
```json
{
  "agentName": "document-review-agent",
  "defaultModel": "gpt-4",
  "heartbeatInterval": "1h",
  "customSettings": {
    "watchFolder": "/inbox/contracts",
    "outputFolder": "/reviewed/contracts",
    "documentTypes": ["contract", "nda", "agreement", "msa"],
    "reviewDepth": "standard",
    "flagKeywords": ["indemnification", "liability", "termination", "penalty"],
    "slaHours": 24,
    "requireLegalReview": [">$100k", "international", "ip-related"]
  }
}
```

**Triggers**:
1. **File-based**: Watch inbox folder for new PDFs
2. **Email-based**: Forward documents to agent email
3. **Heartbeat**: Check for overdue reviews every hour
4. **Time-based**: Daily summary at 5 PM

**Workflow**:
```
09:00 → New file: contract.pdf → Extract text
09:01 → Identify document type → MSA (Master Service Agreement)
09:02 → Apply contract-analysis skill → Extract key terms
09:05 → Apply risk-assessment skill → Flag liability clause
09:08 → Generate review report → Save to output folder
09:10 → Value >$100k → Flag for legal team review
09:15 → Send summary email → Stakeholders notified
```

---

## Best Practices

### 1. Start Simple, Iterate

**Phase 1**: Basic agent with one skill
- Get the core workflow working
- Test with simple cases
- Validate triggers work

**Phase 2**: Add complexity gradually
- Add more skills
- Refine SOUL.md
- Add custom tools if needed

**Phase 3**: Optimize and monitor
- Add heartbeat monitoring
- Tune settings
- Analyze performance

### 2. Clear Separation of Concerns

- **Skills**: WHAT the agent knows (domain knowledge)
- **SOUL.md**: WHO the agent is (personality/behavior)
- **Tools**: WHAT the agent can do (capabilities)
- **Settings**: HOW the agent is configured (parameters)
- **Triggers**: WHEN the agent acts (activation conditions)

Don't mix these concerns - keep them separate for maintainability.

### 3. Test with Human-in-the-Loop

Start with agents that:
- Recommend but don't execute
- Flag for review
- Require approval

Only move to autonomous actions after validation.

### 4. Comprehensive Logging

Log everything:
- When triggers fire
- What the agent analyzed
- What recommendations were made
- What actions were taken
- What errors occurred

Use sessions to maintain history.

### 5. Graceful Degradation

Handle failures gracefully:
- If LLM API is down → Queue for later
- If data is incomplete → Flag for manual review
- If confidence is low → Ask for guidance
- If tool fails → Retry with exponential backoff

### 6. Version Your Skills

Track skill versions:
```markdown
---
name: trading-analysis
description: Technical and fundamental analysis
version: 2.1.0
---
```

Update HEARTBEAT.md when skills change to re-validate.

### 7. Monitor and Measure

Track key metrics:
- Trigger frequency
- Processing time
- Success rate
- Error rate
- Human intervention frequency

Use these to optimize.

---

## Directory Structure

Complete example for a specialized agent project:

```
my-specialized-agent/
├── .agents/                              # Agent configuration
│   ├── SOUL.md                          # Agent personality
│   ├── HEARTBEAT.md                     # Global monitoring
│   └── skills/                          # Task-specific skills
│       ├── primary-skill/
│       │   ├── SKILL.md
│       │   └── HEARTBEAT.md
│       ├── secondary-skill/
│       │   └── SKILL.md
│       └── utility-skill/
│           └── SKILL.md
├── config/                               # Configuration files
│   ├── settings.json                    # Agent settings
│   └── credentials.json                 # API keys (gitignored)
├── data/                                # Data files
│   ├── input/                          # Watched folder
│   └── output/                         # Results folder
├── logs/                                # Log files
│   ├── agent.log
│   └── triggers.log
├── src/                                 # Source code
│   ├── Program.cs                      # Main entry point
│   ├── AgentSetup.cs                   # Agent initialization
│   ├── TriggerSetup.cs                 # Trigger configuration
│   └── Tools/                          # Custom tools
│       ├── MyCustomTool.cs
│       └── AnotherTool.cs
├── tests/                               # Tests
│   ├── SkillTests.cs
│   └── ToolTests.cs
├── README.md                            # Project documentation
└── MyAgent.csproj                       # Project file
```

---

## Summary

To create a specialized agent:

1. **Define the task** clearly (trading, accounting, document review)
2. **Create skills** that teach the agent domain knowledge
3. **Configure SOUL.md** to shape agent behavior
4. **Choose triggers** for when the agent activates
5. **Set up monitoring** with HEARTBEAT.md
6. **Test incrementally** with human oversight
7. **Monitor and improve** based on performance

The beauty of this architecture is **you don't write agent code** - you configure behavior through skills, personality files, and settings. The base Pi.Agent runtime handles all the execution.

---

## Next Steps

1. Read [Creating Skills](../technical/creating-skills.md)
2. Read [Creating Tools](../technical/creating-tools.md) if you need custom capabilities
3. Review [API Reference](../technical/api-reference.md) for agent API details
4. See [Scheduler Component](../components/pi-scheduler.md) for trigger setup
5. Check example skills in `.agents/skills/` for patterns

**Start with a simple use case, get it working, then expand from there.**
