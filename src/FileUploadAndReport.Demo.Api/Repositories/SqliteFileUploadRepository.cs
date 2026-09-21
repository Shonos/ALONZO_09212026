using System.Globalization;
using FileUploadAndReport.Demo.Api.Contracts.Requests;
using Microsoft.Data.Sqlite;

namespace FileUploadAndReport.Demo.Api.Repositories;

public sealed class SqliteFileUploadRepository : IFileUploadRepository
{
    private const int MaxStoredFiles = 100;

    private readonly string _connectionString;

    public SqliteFileUploadRepository(string connectionString)
    {
        _connectionString = connectionString;
        EnsureDataDirectory(connectionString);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS FileUploads (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EventId TEXT NOT NULL UNIQUE,
                EventType TEXT NOT NULL,
                Message TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_FileUploads_CreatedAtUtc
            ON FileUploads (CreatedAtUtc);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> AddAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction();

        await using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO FileUploads (EventId, EventType, Message, CreatedAtUtc)
                VALUES ($eventId, $eventType, $message, $createdAtUtc);
                """;
            insertCommand.Parameters.AddWithValue("$eventId", request.EventId!);
            insertCommand.Parameters.AddWithValue("$eventType", request.EventType!.Value.ToString());
            insertCommand.Parameters.AddWithValue("$message", request.Message!);
            insertCommand.Parameters.AddWithValue(
                "$createdAtUtc",
                DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var trimCommand = connection.CreateCommand())
        {
            trimCommand.Transaction = transaction;
            trimCommand.CommandText = """
                DELETE FROM FileUploads
                WHERE Id NOT IN (
                    SELECT Id
                    FROM FileUploads
                    ORDER BY Id DESC
                    LIMIT $maxStoredFiles
                );
                """;
            trimCommand.Parameters.AddWithValue("$maxStoredFiles", MaxStoredFiles);
            await trimCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        int count;
        await using (var countCommand = connection.CreateCommand())
        {
            countCommand.Transaction = transaction;
            countCommand.CommandText = "SELECT COUNT(*) FROM FileUploads;";
            count = Convert.ToInt32(
                await countCommand.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture);
        }

        await transaction.CommitAsync(cancellationToken);
        return count;
    }

    public async Task<IReadOnlyList<FileUploadRecord>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, EventId, EventType, Message, CreatedAtUtc
            FROM FileUploads
            ORDER BY Id DESC
            LIMIT $maxStoredFiles;
            """;
        command.Parameters.AddWithValue("$maxStoredFiles", MaxStoredFiles);

        var records = new List<FileUploadRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            records.Add(new FileUploadRecord(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                DateTimeOffset.Parse(
                    reader.GetString(4),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind)));
        }

        return records;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM FileUploads;";

        return Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private static void EnsureDataDirectory(string connectionString)
    {
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;

        if (string.IsNullOrWhiteSpace(dataSource)
            || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
