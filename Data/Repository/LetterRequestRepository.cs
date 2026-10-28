using System.Data;
using Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;
using Letter_Request_Management_System.Models.Dto;

namespace Letter_Request_Management_System.Data.Repository
{
    public class LetterRequestRepository
    {
        private readonly BaseSqlDL _baseSqlDL;

        public LetterRequestRepository(BaseSqlDL baseSqlDL)
        {
            _baseSqlDL = baseSqlDL;
        }

        public List<AdminLetterRequestDto> GetAllRequests()
        {
            const string query = @"
                SELECT
                    lr.LetterRequestId,
                    u.UserId,
                    u.EmpNo,
                    u.FullName AS EmployeeName,
                    u.Email,
                    u.Department,
                    lt.LetterTypeName,
                    lr.RequestDate,
                    lr.Status,
                    lr.LetterContentSnapshot AS LetterContent
                FROM dbo.LetterRequests AS lr
                INNER JOIN dbo.Users AS u
                    ON lr.UserId = u.UserId
                INNER JOIN dbo.LetterTypes AS lt
                    ON lr.LetterTypeId = lt.LetterTypeId
                ORDER BY lr.RequestDate DESC;";

            DataTable table = _baseSqlDL.ExecuteReader(
                query,
                new List<DbParametersSql.QueryParameters>());

            var requests = new List<AdminLetterRequestDto>();

            foreach (DataRow row in table.Rows)
            {
                requests.Add(new AdminLetterRequestDto
                {
                    LetterRequestId = Convert.ToInt32(row["LetterRequestId"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    EmpNo = row["EmpNo"]?.ToString() ?? string.Empty,
                    EmployeeName = row["EmployeeName"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString() ?? string.Empty,
                    Department = row["Department"]?.ToString() ?? string.Empty,
                    LetterTypeName = row["LetterTypeName"]?.ToString() ?? string.Empty,
                    RequestDate = Convert.ToDateTime(row["RequestDate"]),
                    Status = row["Status"]?.ToString() ?? string.Empty,
                    LetterContent = row["LetterContent"]?.ToString() ?? string.Empty
                });
            }

            return requests;
        }

        public int CreateRequest(int userId, int letterTypeId)
        {
            const string query = @"
                INSERT INTO dbo.LetterRequests
                (
                    UserId,
                    LetterTypeId,
                    Status,
                    LetterContentSnapshot
                )
                OUTPUT INSERTED.LetterRequestId
                SELECT
                    u.UserId,
                    lt.LetterTypeId,
                    'Pending',
                    REPLACE(
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(
                                        REPLACE(
                                            lt.TemplateContent,
                                            '{{FullName}}', u.FullName
                                        ),
                                        '{{EmpNo}}', u.EmpNo
                                    ),
                                    '{{Email}}', u.Email
                                ),
                                '{{Department}}',
                                COALESCE(u.Department, '')
                            ),
                            '{{Position}}',
                            COALESCE(u.Position, '')
                        ),
                        '{{JoinDate}}',
                        COALESCE(
                            CONVERT(NVARCHAR(10), u.JoinDate, 23),
                            ''
                        )
                    )
                FROM dbo.Users AS u
                CROSS JOIN dbo.LetterTypes AS lt
                WHERE u.UserId = @UserId
                  AND lt.LetterTypeId = @LetterTypeId;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@UserId", SqlDbType.Int, userId),
                DbParametersSql.SetValues("@LetterTypeId", SqlDbType.Int, letterTypeId)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return 0;

            return Convert.ToInt32(table.Rows[0]["LetterRequestId"]);
        }

        public LetterRequestResponseDto? GetRequestById(
            int letterRequestId,
            int userId)
        {
            const string query = @"
                SELECT
                    lr.LetterRequestId,
                    lr.LetterTypeId,
                    lt.LetterTypeName,
                    lr.RequestDate,
                    lr.Status,
                    lr.LetterContentSnapshot AS LetterContent
                FROM dbo.LetterRequests AS lr
                INNER JOIN dbo.LetterTypes AS lt
                    ON lr.LetterTypeId = lt.LetterTypeId
                WHERE lr.LetterRequestId = @LetterRequestId
                  AND lr.UserId = @UserId;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@LetterRequestId", SqlDbType.Int, letterRequestId),
                DbParametersSql.SetValues("@UserId", SqlDbType.Int, userId)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return null;

            DataRow row = table.Rows[0];

            return new LetterRequestResponseDto
            {
                LetterRequestId = Convert.ToInt32(row["LetterRequestId"]),
                LetterTypeId = Convert.ToInt32(row["LetterTypeId"]),
                LetterTypeName = row["LetterTypeName"]?.ToString() ?? string.Empty,
                RequestDate = Convert.ToDateTime(row["RequestDate"]),
                Status = row["Status"]?.ToString() ?? string.Empty,
                LetterContent = row["LetterContent"]?.ToString() ?? string.Empty
            };
        }

        public List<LetterRequestResponseDto> GetMyRequests(int userId)
        {
            const string query = @"
                SELECT
                    lr.LetterRequestId,
                    lr.LetterTypeId,
                    lt.LetterTypeName,
                    lr.RequestDate,
                    lr.Status,
                    lr.LetterContentSnapshot AS LetterContent
                FROM dbo.LetterRequests AS lr
                INNER JOIN dbo.LetterTypes AS lt
                    ON lr.LetterTypeId = lt.LetterTypeId
                WHERE lr.UserId = @UserId
                ORDER BY lr.RequestDate DESC;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@UserId", SqlDbType.Int, userId)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            var requests = new List<LetterRequestResponseDto>();

            foreach (DataRow row in table.Rows)
            {
                requests.Add(new LetterRequestResponseDto
                {
                    LetterRequestId = Convert.ToInt32(row["LetterRequestId"]),
                    LetterTypeId = Convert.ToInt32(row["LetterTypeId"]),
                    LetterTypeName = row["LetterTypeName"]?.ToString() ?? string.Empty,
                    RequestDate = Convert.ToDateTime(row["RequestDate"]),
                    Status = row["Status"]?.ToString() ?? string.Empty,
                    LetterContent = row["LetterContent"]?.ToString() ?? string.Empty
                });
            }

            return requests;
        }

        public AdminLetterRequestDto? GetAdminRequestById(int letterRequestId)
        {
            const string query = @"
                SELECT
                    lr.LetterRequestId,
                    u.UserId,
                    u.EmpNo,
                    u.FullName AS EmployeeName,
                    u.Email,
                    u.Department,
                    lt.LetterTypeName,
                    lr.RequestDate,
                    lr.Status,
                    lr.LetterContentSnapshot AS LetterContent
                FROM dbo.LetterRequests AS lr
                INNER JOIN dbo.Users AS u
                    ON lr.UserId = u.UserId
                INNER JOIN dbo.LetterTypes AS lt
                    ON lr.LetterTypeId = lt.LetterTypeId
                WHERE lr.LetterRequestId = @LetterRequestId;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@LetterRequestId", SqlDbType.Int, letterRequestId)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return null;

            DataRow row = table.Rows[0];

            return new AdminLetterRequestDto
            {
                LetterRequestId = Convert.ToInt32(row["LetterRequestId"]),
                UserId = Convert.ToInt32(row["UserId"]),
                EmpNo = row["EmpNo"]?.ToString() ?? string.Empty,
                EmployeeName = row["EmployeeName"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString() ?? string.Empty,
                Department = row["Department"]?.ToString() ?? string.Empty,
                LetterTypeName = row["LetterTypeName"]?.ToString() ?? string.Empty,
                RequestDate = Convert.ToDateTime(row["RequestDate"]),
                Status = row["Status"]?.ToString() ?? string.Empty,
                LetterContent = row["LetterContent"]?.ToString() ?? string.Empty
            };
        }

        public int ApproveRequest(
            int letterRequestId,
            int approvedByUserId,
            string signatureReference,
            string pdfReference)
        {
            const string query = @"
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;

                IF NOT EXISTS (
                    SELECT 1
                    FROM dbo.LetterRequests WITH (UPDLOCK, HOLDLOCK)
                    WHERE LetterRequestId = @LetterRequestId
                      AND Status = 'Pending'
                )
                BEGIN
                    ROLLBACK TRANSACTION;
                    SELECT 0 AS Result;
                    RETURN;
                END;

                UPDATE dbo.LetterRequests
                SET Status = 'Approved'
                WHERE LetterRequestId = @LetterRequestId
                  AND Status = 'Pending';

                INSERT INTO dbo.LetterApprovals
                (
                    LetterRequestId,
                    ApprovedByUserId,
                    ApprovedAt,
                    SignatureReference,
                    PdfReference
                )
                VALUES
                (
                    @LetterRequestId,
                    @ApprovedByUserId,
                    SYSDATETIME(),
                    @SignatureReference,
                    @PdfReference
                );

                COMMIT TRANSACTION;
                SELECT 1 AS Result;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@LetterRequestId", SqlDbType.Int, letterRequestId),
                DbParametersSql.SetValues("@ApprovedByUserId", SqlDbType.Int, approvedByUserId),
                DbParametersSql.SetValues("@SignatureReference", SqlDbType.NVarChar, signatureReference),
                DbParametersSql.SetValues("@PdfReference", SqlDbType.NVarChar, pdfReference)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return 0;

            return Convert.ToInt32(table.Rows[0]["Result"]);
        }

        public string? GetApprovedPdfReference(
            int letterRequestId,
            int userId)
        {
            const string query = @"
                SELECT LA.PdfReference
                FROM dbo.LetterRequests AS LR
                INNER JOIN dbo.LetterApprovals AS LA
                    ON LR.LetterRequestId = LA.LetterRequestId
                WHERE LR.LetterRequestId = @LetterRequestId
                  AND LR.UserId = @UserId
                  AND LR.Status = 'Approved'
                  AND LA.PdfReference IS NOT NULL;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@LetterRequestId", SqlDbType.Int, letterRequestId),
                DbParametersSql.SetValues("@UserId", SqlDbType.Int, userId)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return null;

            return table.Rows[0]["PdfReference"]?.ToString();
        }

        public string? GetUserFullName(int userId)
        {
            const string query = @"
                SELECT FullName
                FROM dbo.Users
                WHERE UserId = @UserId;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@UserId", SqlDbType.Int, userId)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return null;

            return table.Rows[0]["FullName"]?.ToString();
        }

        // Uses the current saved signature for the approver.
       
        public (
            string LetterContent,
            string SignatureReference,
            string ApproverName,
            DateTime ApprovedAt
        )? GetApprovedLetterForDownload(int letterRequestId, int userId)
        {
            const string query = @"
                SELECT
                    LR.LetterContentSnapshot AS LetterContent,
                    COALESCE(
                        NULLIF(ASig.SignatureReference, ''),
                        NULLIF(LA.SignatureReference, '')
                    ) AS SignatureReference,
                    AdminUser.FullName AS ApproverName,
                    LA.ApprovedAt
                FROM dbo.LetterRequests AS LR
                INNER JOIN dbo.LetterApprovals AS LA
                    ON LR.LetterRequestId = LA.LetterRequestId
                INNER JOIN dbo.Users AS AdminUser
                    ON LA.ApprovedByUserId = AdminUser.UserId
                LEFT JOIN dbo.AdminSignatures AS ASig
                    ON ASig.UserId = LA.ApprovedByUserId
                WHERE LR.LetterRequestId = @LetterRequestId
                  AND LR.UserId = @UserId
                  AND LR.Status = 'Approved';";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@LetterRequestId", SqlDbType.Int, letterRequestId),
                DbParametersSql.SetValues("@UserId", SqlDbType.Int, userId)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return null;

            DataRow row = table.Rows[0];

            if (row["LetterContent"] == DBNull.Value ||
                row["SignatureReference"] == DBNull.Value ||
                row["ApproverName"] == DBNull.Value ||
                row["ApprovedAt"] == DBNull.Value)
            {
                return null;
            }

            return (
                row["LetterContent"].ToString() ?? string.Empty,
                row["SignatureReference"].ToString() ?? string.Empty,
                row["ApproverName"].ToString() ?? string.Empty,
                Convert.ToDateTime(row["ApprovedAt"])
            );
        }
    }
}
