using Dapper;
using Fraktal_Nuaio.Tracking.Unparented.Shared.Dtos;
using Microsoft.Data.SqlClient;

namespace Fraktal_Nuaio.Tracking.Unparented.Shared.Helpers
{
    public static class SqlServerHelper
    {
        public async static Task<List<UnparentedUcidDto>> GetUnparentedUcidList(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "SELECT Id, Origin, Ucid, RequestJson, JsonToQueue, GenerationDate FROM UnparentedUcids";

                return await connection.ExecuteScalarAsync<List<UnparentedUcidDto>>(sql);
            }
        }

        public async static Task<bool> DeleteUnparentedUcid(string connectionString, Guid id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    Id = id
                };
                var sql = "DELETE FROM UnparentedUcids WHERE Id = @Id";

                return (await connection.ExecuteAsync(sql, parameters)) > 0;

            }
        }

        public async static Task<Guid?> IsUcidInBookingProcess(string connectionString, string ucid)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var parameters = new
                {
                    ucid
                };
                var sql = "SELECT Id FROM ProcessDetail WHERE Ucid = @Ucid";

                var result = await connection.ExecuteScalarAsync<Guid?>(sql, parameters);
                return result;
            }
        }
    }
}