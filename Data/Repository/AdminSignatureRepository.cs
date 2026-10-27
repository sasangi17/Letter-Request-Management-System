using System.Data;
using Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;

namespace Letter_Request_Management_System.Data.Repository
{
    public class AdminSignatureRepository
    {
        private readonly BaseSqlDL _baseSqlDL;

        public AdminSignatureRepository(BaseSqlDL baseSqlDL)
        {
            _baseSqlDL = baseSqlDL;
        }

        // Save a new signature reference or update the existing one.
        public bool SaveOrUpdateSignatureReference(
            int userId,
            string signatureReference)
        {
            const string query = @"
                UPDATE dbo.AdminSignatures
                SET
                    SignatureReference = @SignatureReference,
                    UpdatedAt = SYSDATETIME()
                WHERE UserId = @UserId;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO dbo.AdminSignatures
                    (
                        UserId,
                        SignatureReference
                    )
                    VALUES
                    (
                        @UserId,
                        @SignatureReference
                    );
                END;

                SELECT UserId
                FROM dbo.AdminSignatures
                WHERE UserId = @UserId;";

            var parameters =
                new List<DbParametersSql.QueryParameters>
                {
                    DbParametersSql.SetValues(
                        "@UserId",
                        SqlDbType.Int,
                        userId),

                    DbParametersSql.SetValues(
                        "@SignatureReference",
                        SqlDbType.NVarChar,
                        signatureReference)
                };

            DataTable table =
                _baseSqlDL.ExecuteReader(query, parameters);

            return table.Rows.Count > 0;
        }

        // Retrieve the saved signature reference for an admin.
        public string? GetSignatureReference(int userId)
        {
            const string query = @"
                SELECT SignatureReference
                FROM dbo.AdminSignatures
                WHERE UserId = @UserId;";

            var parameters =
                new List<DbParametersSql.QueryParameters>
                {
                    DbParametersSql.SetValues(
                        "@UserId",
                        SqlDbType.Int,
                        userId)
                };

            DataTable table =
                _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
            {
                return null;
            }

            return table.Rows[0]["SignatureReference"]
                ?.ToString();
        }
    }
}
