using Microsoft.Data.Sqlite;
using System.Globalization;

namespace Pi.IMessage;

/// <summary>
/// Interface for accessing the iMessage database
/// </summary>
public interface IIMessageDatabase
{
    Task<List<IMessageRecord>> GetRecentMessagesAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<List<IMessageRecord>> GetMessagesSinceAsync(long rowId, int limit = 100, CancellationToken cancellationToken = default);
    Task<IMessageChat?> GetChatByGuidAsync(string chatGuid, CancellationToken cancellationToken = default);
    Task<IMessageHandle?> GetHandleByIdAsync(string handleId, CancellationToken cancellationToken = default);
    Task<List<IMessageHandle>> GetHandlesForChatAsync(string chatGuid, CancellationToken cancellationToken = default);
}

/// <summary>
/// SQLite-based implementation for accessing iMessage database
/// </summary>
public class IMessageDatabase : IIMessageDatabase, IDisposable
{
    private readonly string _connectionString;
    private bool _disposed;

    public IMessageDatabase(string databasePath)
    {
        var expandedPath = Environment.ExpandEnvironmentVariables(
            databasePath.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
        
        if (!File.Exists(expandedPath))
        {
            throw new FileNotFoundException($"iMessage database not found at: {expandedPath}");
        }

        _connectionString = $"Data Source={expandedPath};Mode=ReadOnly";
    }

    public async Task<List<IMessageRecord>> GetRecentMessagesAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                m.ROWID as row_id,
                m.guid,
                m.text,
                m.handle_id,
                m.service,
                m.account_guid,
                m.date,
                m.date_read,
                m.date_delivered,
                m.is_from_me,
                m.is_read,
                m.is_audio_message,
                m.subject,
                m.group_title,
                m.group_action_type,
                m.associated_message_guid,
                m.associated_message_type,
                m.balloon_bundle_id,
                m.expressive_send_style_id,
                m.thread_originator_guid,
                m.thread_originator_part,
                c.chat_identifier,
                c.display_name,
                c.guid as chat_guid
            FROM message m
            LEFT JOIN chat_message_join cmj ON m.ROWID = cmj.message_id
            LEFT JOIN chat c ON cmj.chat_id = c.ROWID
            ORDER BY m.date DESC
            LIMIT @limit";

        return await ExecuteQueryAsync(query, new { limit }, cancellationToken);
    }

    public async Task<List<IMessageRecord>> GetMessagesSinceAsync(long rowId, int limit = 100, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                m.ROWID as row_id,
                m.guid,
                m.text,
                m.handle_id,
                m.service,
                m.account_guid,
                m.date,
                m.date_read,
                m.date_delivered,
                m.is_from_me,
                m.is_read,
                m.is_audio_message,
                m.subject,
                m.group_title,
                m.group_action_type,
                m.associated_message_guid,
                m.associated_message_type,
                m.balloon_bundle_id,
                m.expressive_send_style_id,
                m.thread_originator_guid,
                m.thread_originator_part,
                c.chat_identifier,
                c.display_name,
                c.guid as chat_guid
            FROM message m
            LEFT JOIN chat_message_join cmj ON m.ROWID = cmj.message_id
            LEFT JOIN chat c ON cmj.chat_id = c.ROWID
            WHERE m.ROWID > @rowId
            ORDER BY m.date ASC
            LIMIT @limit";

        return await ExecuteQueryAsync(query, new { rowId, limit }, cancellationToken);
    }

    public async Task<IMessageChat?> GetChatByGuidAsync(string chatGuid, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                ROWID as row_id,
                guid,
                style,
                state,
                account_id,
                chat_identifier,
                service_name,
                room_name,
                account_login,
                is_archived,
                last_addressed_handle,
                display_name,
                group_id,
                is_filtered,
                successful_query
            FROM chat
            WHERE guid = @chatGuid";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.AddWithValue("@chatGuid", chatGuid);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new IMessageChat
        {
            RowId = reader.GetInt64(0),
            Guid = reader.GetString(1),
            Style = reader.GetInt32(2),
            State = reader.GetInt32(3),
            AccountId = GetStringOrEmpty(reader, 4),
            ChatIdentifier = GetStringOrNull(reader, 5),
            ServiceName = GetStringOrEmpty(reader, 6),
            RoomName = GetStringOrEmpty(reader, 7),
            AccountLogin = GetStringOrEmpty(reader, 8),
            IsArchived = reader.GetBoolean(9),
            LastAddressedHandle = GetStringOrNull(reader, 10),
            DisplayName = GetStringOrNull(reader, 11),
            GroupId = GetStringOrEmpty(reader, 12),
            IsFiltered = reader.GetBoolean(13),
            SuccessfulQuery = reader.GetInt32(14)
        };
    }

    public async Task<IMessageHandle?> GetHandleByIdAsync(string handleId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                ROWID as row_id,
                id,
                country,
                service,
                uncanonicalized_id
            FROM handle
            WHERE id = @handleId";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.AddWithValue("@handleId", handleId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new IMessageHandle
        {
            RowId = reader.GetInt64(0),
            Id = reader.GetString(1),
            Country = GetStringOrEmpty(reader, 2),
            Service = GetStringOrEmpty(reader, 3),
            Uncanonicalized_id = GetStringOrEmpty(reader, 4)
        };
    }

    public async Task<List<IMessageHandle>> GetHandlesForChatAsync(string chatGuid, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                h.ROWID as row_id,
                h.id,
                h.country,
                h.service,
                h.uncanonicalized_id
            FROM handle h
            JOIN chat_handle_join chj ON h.ROWID = chj.handle_id
            JOIN chat c ON chj.chat_id = c.ROWID
            WHERE c.guid = @chatGuid";

        var handles = new List<IMessageHandle>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.AddWithValue("@chatGuid", chatGuid);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            handles.Add(new IMessageHandle
            {
                RowId = reader.GetInt64(0),
                Id = reader.GetString(1),
                Country = GetStringOrEmpty(reader, 2),
                Service = GetStringOrEmpty(reader, 3),
                Uncanonicalized_id = GetStringOrEmpty(reader, 4)
            });
        }

        return handles;
    }

    private async Task<List<IMessageRecord>> ExecuteQueryAsync(string query, object parameters, CancellationToken cancellationToken)
    {
        var messages = new List<IMessageRecord>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = query;

        // Add parameters
        foreach (var prop in parameters.GetType().GetProperties())
        {
            command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(parameters) ?? DBNull.Value);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            messages.Add(ParseMessageRecord(reader));
        }

        return messages;
    }

    private static IMessageRecord ParseMessageRecord(SqliteDataReader reader)
    {
        return new IMessageRecord
        {
            RowId = reader.GetInt64(0),
            Guid = reader.GetString(1),
            Text = GetStringOrNull(reader, 2),
            HandleId = reader.GetInt32(3).ToString(),
            Service = GetStringOrEmpty(reader, 4),
            AccountGuid = GetStringOrNull(reader, 5),
            Date = ConvertMacTimeToDateTime(reader.GetInt64(6)),
            DateRead = ConvertMacTimeToDateTime(reader.GetInt64(7)),
            DateDelivered = ConvertMacTimeToDateTime(reader.GetInt64(8)),
            IsFromMe = reader.GetBoolean(9),
            IsRead = reader.GetBoolean(10),
            IsAudioMessage = reader.GetBoolean(11),
            Subject = GetStringOrNull(reader, 12),
            GroupTitle = GetStringOrNull(reader, 13),
            GroupActionType = GetIntOrNull(reader, 14),
            AssociatedMessageGuid = GetStringOrNull(reader, 15),
            AssociatedMessageType = GetStringOrNull(reader, 16),
            BalloonBundleId = GetStringOrNull(reader, 17),
            ExpressiveSendStyleId = GetStringOrNull(reader, 18),
            ThreadOriginatorGuid = GetStringOrNull(reader, 19),
            ThreadOriginatorPart = GetStringOrNull(reader, 20)
        };
    }

    private static DateTime ConvertMacTimeToDateTime(long macTime)
    {
        // macOS stores timestamps as nanoseconds since 2001-01-01
        var macEpoch = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return macEpoch.AddSeconds(macTime / 1_000_000_000.0);
    }

    private static string? GetStringOrNull(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static string GetStringOrEmpty(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static int? GetIntOrNull(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
