using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Access.Models;
using Microsoft.Data.SqlClient;

namespace Access.DataAccess
{
    public interface ISecurityLogRepository
    {
        Task<bool> LogSecurityEventAsync(
            string ipAddress,
            string userEmail,
            string action,
            string description,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<List<SecurityLog>> GetSecurityLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string email = null,
            SqlConnection connection = null,
            SqlTransaction transaction = null);
    }
}
