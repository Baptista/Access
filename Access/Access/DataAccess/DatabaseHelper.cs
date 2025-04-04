using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Access.DataAccess
{
    public static class DatabaseHelper
    {
        public static async Task<int> ExecuteNonQueryAsync(
            string connectionString,
            string storedProcedure,
            IEnumerable<SqlParameter> parameters = null,
            SqlTransaction transaction = null)
        {
            if (transaction != null)
            {
                using (SqlCommand command = new SqlCommand(storedProcedure, transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                    }
                    return await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                    }
                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Executes a stored procedure that returns a result set.
        // If a transaction is provided, its connection is used.
        public static async Task<DataTable> ExecuteQueryAsync(
            string connectionString,
            string storedProcedure,
            IEnumerable<SqlParameter> parameters = null,
            SqlTransaction transaction = null)
        {
            if (transaction != null)
            {
                using (SqlCommand command = new SqlCommand(storedProcedure, transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                    }
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                    }
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
        }
    }
}
