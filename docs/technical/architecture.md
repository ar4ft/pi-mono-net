# Architecture Overview
Comprehensive guide to Pi-Mono-Net's architecture.

## System Architecture
```
Gateway → Channels → Agent Runtime → LLM
```

## Components
- **Pi.AI**: LLM integration
- **Pi.Agent**: Agent runtime
- **Pi.Gateway**: Message routing
- **Pi.Channels**: Channel abstraction
- **Pi.Browser**: Web automation
- **Pi.Scheduler**: Task scheduling

## Message Flow
1. User sends message via channel
2. Channel normalizes to ChannelMessage
3. Gateway routes to session
4. Agent processes with LLM
5. Response flows back

See component docs for details.
