# Build a Channel Adapter

Create a Telegram channel adapter in 20 minutes.

## Goal
Integrate Telegram Bot API with Pi-Mono-Net.

## Step 1: Install Telegram Bot SDK

```bash
dotnet add package Telegram.Bot
```

## Step 2: Create the Channel

```csharp
using Pi.Channels;
using Telegram.Bot;

public class TelegramChannel : ChannelBase
{
    private readonly TelegramBotClient _client;
    
    public TelegramChannel(string botToken)
    {
        _client = new TelegramBotClient(botToken);
    }
    
    public override string Id => "telegram";
    public override string Name => "Telegram";
    
    protected override async Task StartInternalAsync(
        CancellationToken cancellationToken)
    {
        _client.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            cancellationToken: cancellationToken
        );
    }
    
    private async Task HandleUpdateAsync(
        ITelegramBotClient client,
        Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message?.Text == null)
            return;
        
        var message = new ChannelMessage
        {
            Id = update.Message.MessageId.ToString(),
            ConversationId = $"telegram:{update.Message.Chat.Id}",
            SenderId = update.Message.From.Id.ToString(),
            SenderName = update.Message.From.FirstName,
            Content = update.Message.Text,
            SessionKey = SessionKeyBuilder.Build(
                "main", "telegram", "private",
                update.Message.Chat.Id.ToString()
            )
        };
        
        OnMessageReceived(message);
    }
    
    public override async Task SendMessageAsync(
        ChannelMessage message,
        CancellationToken cancellationToken)
    {
        var chatId = ExtractChatId(message.ConversationId);
        await _client.SendTextMessageAsync(
            chatId, message.Content,
            cancellationToken: cancellationToken
        );
    }
}
```

## Step 3: Register with Gateway

```csharp
var channel = new TelegramChannel(botToken);
await gateway.RegisterChannelAsync(channel);
await channel.StartAsync(cancellationToken);
```

## Step 4: Test It

Send a message to your Telegram bot - it should be routed through the Gateway to your agent!

See [Creating Channels](../technical/creating-channels.md) for advanced features.
