using System;
using System.Collections.Generic;
using System.Data;
using Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;
using Letter_Request_Management_System.Models.Domain;

namespace Letter_Request_Management_System.Data.Repository
{
    public class AdminSignatureRepository
    {
        private readonly BaseSqlDL _baseSqlDL;

        public AdminSignatureRepository(BaseSqlDL baseSqlDL)
        {
            _baseSqlDL = baseSqlDL;
        }

        // Save a new admin signature or update an existing signature
        public bool SaveOrUpdateSignature(
            int adminUserId,
            byte[] signatureImage,
            string contentType,
            string? fileName)
        {
            string reference = string.IsNullOrWhiteSpace(fileName)
                ? "admin-signature"
                : fileName;

            const string query = @"
                SET NOCOUNT ON;

                UPDATE dbo.AdminSignatures
                SET
                    SignatureImage = @SignatureImage,
                    SignatureReference = @SignatureReference,
                    ContentType = @ContentType,
                    FileName = @FileName,
                    UpdatedAt = SYSDATETIME()
                WHERE AdminUserId = @AdminUserId;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO dbo.AdminSignatures
                    (
                        AdminUserId,
                        SignatureReference,
                        SignatureImage,
                        ContentType,
                        FileName,
                        UpdatedAt
                    )
                    VALUES
                    (
                        @AdminUserId,
                        @SignatureReference,
                        @SignatureImage,
                        @ContentType,
                        @FileName,
                        SYSDATETIME()
                    );
                END;

                SELECT AdminUserId
                FROM dbo.AdminSignatures
                WHERE AdminUserId = @AdminUserId;
            ";

            var parameters =
                new List<DbParametersSql.QueryParameters>
                {
                    DbParametersSql.SetValues("@AdminUserId",SqlDbType.Int,adminUserId),
                    DbParametersSql.SetValues("@SignatureReference",SqlDbType.NVarChar,reference),
                    DbParametersSql.SetValues("@SignatureImage",SqlDbType.VarBinary,signatureImage),
                    DbParametersSql.SetValues("@ContentType",SqlDbType.NVarChar,contentType),
                    DbParametersSql.SetValues("@FileName",SqlDbType.NVarChar,fileName ?? string.Empty)
                };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            return table.Rows.Count > 0;
        }

        // Retrieve the admin signature
        public AdminSignatureMl? GetSignature(int adminUserId)
        {
            const string query = @"
                SELECT
                    AdminSignatureId,
                    AdminUserId,
                    SignatureImage,
                    ContentType,
                    FileName,
                    UpdatedAt
                FROM dbo.AdminSignatures
                WHERE AdminUserId = @AdminUserId;
            ";

            var parameters =
                new List<DbParametersSql.QueryParameters>
                {
                    DbParametersSql.SetValues("@AdminUserId",SqlDbType.Int,adminUserId)
                };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = table.Rows[0];

            return new AdminSignatureMl
            {
                AdminSignatureId = Convert.ToInt32(row["AdminSignatureId"]),
                AdminUserId = Convert.ToInt32(row["AdminUserId"]),
                SignatureImage = row["SignatureImage"] == DBNull.Value? Array.Empty<byte>() : (byte[])row["SignatureImage"],
                ContentType = row["ContentType"] == DBNull.Value? string.Empty: row["ContentType"].ToString()!,
                FileName = row["FileName"] == DBNull.Value? null: row["FileName"].ToString(),
                UpdatedAt = Convert.ToDateTime(row["UpdatedAt"])
            };
        }
    }
}
