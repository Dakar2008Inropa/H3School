using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace OwnORM.Data
{
    public sealed class SqlDb : IDisposable
    {
        private readonly string _connectionString;

        private SqlConnection _connection;

        public SqlDb(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string must not be empty", nameof(connectionString));

            _connectionString = connectionString;
        }

        private async Task EnsureOpenAsync(CancellationToken cancellationToken)
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return;

            _connection ??= new SqlConnection(_connectionString);

            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        private static SqlCommand CreateCommand(SqlConnection connection, string sqlOrProcName, CommandType commandType, IDictionary<string, object> parameters, SqlTransaction tx = null)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = sqlOrProcName;
            command.CommandType = commandType;
            command.Transaction = tx;

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> kvp in parameters)
                {
                    string paramName = kvp.Key.StartsWith("@", StringComparison.Ordinal) ? kvp.Key : "@" + kvp.Key;
                    object value = kvp.Value ?? DBNull.Value;

                    command.Parameters.AddWithValue(paramName, value);
                }
            }

            return command;
        }

        public async Task<int> ExecuteAsync(string sql, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL must not be empty.", nameof(sql));

            await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);

            using (SqlCommand cmd = CreateCommand(_connection, sql, CommandType.Text, parameters))
            {
                return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, IDictionary<string, object> parameters, CancellationToken cancellationToken) where T : new()
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL must not be empty.", nameof(sql));

            await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);

            using (SqlCommand cmd = CreateCommand(_connection, sql, CommandType.Text, parameters))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {
                List<T> list = new List<T>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    T item = Data.DataMapper.MapRecord<T>(reader);
                    list.Add(item);
                }

                return list;
            }
        }

        public async Task<IReadOnlyList<T>> QueryStoredAsync<T>(string procName, IDictionary<string, object> parameters, CancellationToken cancellationToken) where T : new()
        {
            if (string.IsNullOrWhiteSpace(procName))
                throw new ArgumentException("Procedure name must not be empty.", nameof(procName));

            await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);

            using (SqlCommand cmd = CreateCommand(_connection, procName, CommandType.StoredProcedure, parameters))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {
                List<T> list = new List<T>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    T item = Data.DataMapper.MapRecord<T>(reader);
                    list.Add(item);
                }

                return list;
            }
        }

        public async Task<int> ExecuteBatchInTransactionAsync(IEnumerable<(string sql, IDictionary<string, object> Parameters)> statements, CancellationToken cancellationToken)
        {
            await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);

            SqlTransaction tx = (SqlTransaction)await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            int total = 0;
            bool committed = false;

            try
            {
                foreach (var (Sql, Parameters) in statements)
                {
                    if (string.IsNullOrWhiteSpace(Sql))
                        continue;

                    using (SqlCommand cmd = CreateCommand(_connection, Sql, CommandType.Text, Parameters, tx))
                    {
                        int affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        total += affected;
                    }
                }

                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
                committed = true;
                return total;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing batch in transaction.", ex);
            }
            finally
            {
                if (!committed)
                {
                    try
                    {
                        await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception rollbackEx)
                    {
                        Debug.WriteLine($"Rollback failed: {rollbackEx}");
                    }
                }

                await tx.DisposeAsync();
            }
        }

        public void Dispose()
        {
            if (_connection == null)
                return;

            if (_connection.State != ConnectionState.Closed)
                _connection.Close();

            _connection.Dispose();
            _connection = null;
        }
    }
}