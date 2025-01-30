using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace CurrencyApp.Models
{
        public interface IDatabase
        {
            DbProviderFactory DbProviderFactory { get; }
            Task<DataTable> ExecuteQueryAsync(string query, DbParameter[] parameters = null);
            Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters = null);
        }
    
}
