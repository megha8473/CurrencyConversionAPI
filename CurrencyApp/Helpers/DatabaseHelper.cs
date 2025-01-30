using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using CurrencyApp.Models;
using Microsoft.Data.SqlClient;

namespace CurrencyConversionAPI.Helpers
{
    public class Database: IDatabase
    {
       
        public DbProviderFactory DbProviderFactory { get; }
        public string ConnectionString { get; }

    
        public Database(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            DbProviderFactory = DbProviderFactories.GetFactory("Microsoft.Data.SqlClient");

        }

        // Prepare command for SQL Server command with DbConnection
        protected static void PrepareCommand(DbCommand command, DbConnection connection)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            command.Connection = connection;
        }

        // Prepare command for SQL Server command with DbTransaction
        protected static void PrepareCommand(DbCommand command, DbTransaction transaction)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            command.Transaction = transaction;
        }

        // Execute query to fetch data
        public async Task<DataTable> ExecuteQueryAsync(string query, DbParameter[] parameters = null)
        {
            // Ensure query is not null
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            // Create a DataTable to hold the result
            var dataTable = new DataTable();

            try
            {
                // Create and open the SQL connection
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    // Create the SQL command
                    using (var command = new SqlCommand(query, connection))
                    {
                        // Add parameters if provided
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        // Execute and fill the DataTable
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               
                throw new Exception("Error executing query.", ex);
            }

            return dataTable;
        }


        // Execute non-query command (e.g., INSERT, UPDATE, DELETE)
        public async Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters = null)
        {
            using (DbConnection connection = DbProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                await connection.OpenAsync();

                using (DbCommand command = DbProviderFactory.CreateCommand())
                {
                    PrepareCommand(command, connection);
                    command.CommandText = query;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return await command.ExecuteNonQueryAsync();  // Execute non-query asynchronously
                }
            }
        }
    }
}
