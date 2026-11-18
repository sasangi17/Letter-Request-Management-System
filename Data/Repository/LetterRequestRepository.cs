using System.Data;
using Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;
using Letter_Request_Management_System.Models.Domain;
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

        // GET all letter requests (Admin)


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
            lr.LetterTypeName,
            lr.RequestDate,
            lr.Status,
            lr.LetterContentSnapshot AS LetterContent,
            lr.CopyType,
            lr.IsPrinted,
            lr.PrintedByUserId,
            lr.PrintedAt
        FROM dbo.LetterRequests AS lr
        INNER JOIN dbo.Users AS u
            ON lr.UserId = u.UserId
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
                    LetterContent = row["LetterContent"]?.ToString() ?? string.Empty,

                    CopyType = row["CopyType"]?.ToString() ?? "Soft Copy",
                    IsPrinted = row["IsPrinted"] != DBNull.Value &&
                                Convert.ToBoolean(row["IsPrinted"]),
                    PrintedByUserId = row["PrintedByUserId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(row["PrintedByUserId"]),
                    PrintedAt = row["PrintedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(row["PrintedAt"])
                });
            }

            return requests;
        }



        // GET employee details for letter generation (Employee)

        public UserMl? GetEmployeeForLetter(int userId)
        {
            const string query = @"
                SELECT
                    UserId,
                    EmpNo,
                    Email,
                    FullName,
                    Department,
                    Position,
                    JoinDate,
                    Salary
                FROM dbo.Users
                WHERE UserId = @UserId;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues(
                    "@UserId", SqlDbType.Int, userId)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return null;

            DataRow row = table.Rows[0];

            return new UserMl
            {
                UserId = Convert.ToInt32(row["UserId"]),
                EmpNo = row["EmpNo"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString() ?? string.Empty,
                FullName = row["FullName"]?.ToString() ?? string.Empty,
                Department = row["Department"]?.ToString() ?? string.Empty,
                Position = row["Position"]?.ToString() ?? string.Empty,
                JoinDate = row["JoinDate"] == DBNull.Value? null: Convert.ToDateTime(row["JoinDate"]),
                Salary = row["Salary"] == DBNull.Value? null: Convert.ToDecimal(row["Salary"])
            };
        }


        // Create a new letter request (Employee)


        public int CreateRequest(
            int userId,
            string letterTypeName,
            string letterContent,
            string copyType = "Soft Copy")
        {
            if (copyType != "Soft Copy" && copyType != "Hard Copy")
                throw new ArgumentException("Invalid copy type.");

            const string query = @"
        SET XACT_ABORT ON;

        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO dbo.LetterRequests
            (
                UserId,
                LetterTypeName,
                Status,
                LetterContentSnapshot,
                CopyType
            )
            OUTPUT INSERTED.LetterRequestId
            VALUES
            (
                @UserId,
                @LetterTypeName,
                'Pending',
                @LetterContentSnapshot,
                @CopyType
            );

            COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            THROW;
        END CATCH;";

            var parameters = new List<DbParametersSql.QueryParameters>
    {
        DbParametersSql.SetValues("@UserId", SqlDbType.Int, userId),
        DbParametersSql.SetValues("@LetterTypeName", SqlDbType.NVarChar, letterTypeName),
        DbParametersSql.SetValues("@LetterContentSnapshot", SqlDbType.NVarChar, letterContent),
        DbParametersSql.SetValues("@CopyType", SqlDbType.NVarChar, copyType)
    };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return 0;

            return Convert.ToInt32(table.Rows[0]["LetterRequestId"]);
        }



        // Get letter request by ID (Employee)

        public LetterRequestResponseDto? GetRequestById(
            int letterRequestId,
            int userId)
        {
            const string query = @"
                SELECT
    lr.LetterRequestId,
    lr.LetterTypeName,
    lr.RequestDate,
    lr.Status,
    lr.LetterContentSnapshot AS LetterContent,
    lr.CopyType,
    lr.IsPrinted,
    lr.PrintedByUserId,
    lr.PrintedAt
FROM dbo.LetterRequests AS lr
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
                LetterTypeName = row["LetterTypeName"]?.ToString() ?? string.Empty,
                RequestDate = Convert.ToDateTime(row["RequestDate"]),
                Status = row["Status"]?.ToString() ?? string.Empty,
                LetterContent = row["LetterContent"]?.ToString() ?? string.Empty,
                CopyType = row["CopyType"]?.ToString() ?? "Soft Copy",

                IsPrinted = row["IsPrinted"] != DBNull.Value &&
            Convert.ToBoolean(row["IsPrinted"]),

                PrintedByUserId = row["PrintedByUserId"] == DBNull.Value
    ? null
    : Convert.ToInt32(row["PrintedByUserId"]),

                PrintedAt = row["PrintedAt"] == DBNull.Value
    ? null
    : Convert.ToDateTime(row["PrintedAt"])
            };
        }


        // GET all my letter requests (Employee)

        public List<LetterRequestResponseDto> GetMyRequests(int userId)
        {
            const string query = @"
                SELECT
    lr.LetterRequestId,
    lr.LetterTypeName,
    lr.RequestDate,
    lr.Status,
    lr.LetterContentSnapshot AS LetterContent,
    lr.CopyType,
    lr.IsPrinted,
    lr.PrintedByUserId,
    lr.PrintedAt
FROM dbo.LetterRequests AS lr
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
                    LetterTypeName = row["LetterTypeName"]?.ToString() ?? string.Empty,
                    RequestDate = Convert.ToDateTime(row["RequestDate"]),
                    Status = row["Status"]?.ToString() ?? string.Empty,
                    LetterContent = row["LetterContent"]?.ToString() ?? string.Empty,

                    CopyType = row["CopyType"]?.ToString() ?? "Soft Copy",
                    IsPrinted = row["IsPrinted"] != DBNull.Value &&
                Convert.ToBoolean(row["IsPrinted"]),
                    PrintedByUserId = row["PrintedByUserId"] == DBNull.Value
        ? null
        : Convert.ToInt32(row["PrintedByUserId"]),
                    PrintedAt = row["PrintedAt"] == DBNull.Value
        ? null
        : Convert.ToDateTime(row["PrintedAt"])
                });
            }

            return requests;
        }


        // GET letter request by ID (Admin)

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
                    lr.LetterTypeName,
                    lr.RequestDate,
                    lr.Status,
                    lr.CopyType,
                    lr.IsPrinted,
                    lr.PrintedByUserId,
                    lr.PrintedAt,
                    lr.LetterContentSnapshot AS LetterContent
                FROM dbo.LetterRequests AS lr
                INNER JOIN dbo.Users AS u
                    ON lr.UserId = u.UserId
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
                LetterContent = row["LetterContent"]?.ToString() ?? string.Empty,
                CopyType = row["CopyType"]?.ToString() ?? "Soft Copy",
                IsPrinted = row["IsPrinted"] != DBNull.Value &&
            Convert.ToBoolean(row["IsPrinted"]),
                PrintedByUserId = row["PrintedByUserId"] == DBNull.Value
    ? null
    : Convert.ToInt32(row["PrintedByUserId"]),
                PrintedAt = row["PrintedAt"] == DBNull.Value
    ? null
    : Convert.ToDateTime(row["PrintedAt"])
            };
        }


        // Approve a letter request (Admin)

        public int ApproveRequest(
            int letterRequestId,
            int approvedByUserId,
            byte[] signatureImage,
            string contentType,
            string pdfReference)
        {
            const string query = @"
                
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
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
        UserId,
        ApprovedByUserId,
        ApprovedAt,
        PdfReference,
        SignatureImageSnapshot
    )
    SELECT
        LR.LetterRequestId,
        LR.UserId,
        @ApprovedByUserId,
        SYSDATETIME(),
        @PdfReference,
        @SignatureImageSnapshot
    FROM dbo.LetterRequests LR
    WHERE LR.LetterRequestId = @LetterRequestId;

    COMMIT TRANSACTION;

    SELECT 1 AS Result;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;";

            var parameters = new List<DbParametersSql.QueryParameters>
            {
                DbParametersSql.SetValues("@LetterRequestId", SqlDbType.Int, letterRequestId),
                DbParametersSql.SetValues("@ApprovedByUserId", SqlDbType.Int, approvedByUserId),
                DbParametersSql.SetValues("@PdfReference", SqlDbType.NVarChar, pdfReference),
                DbParametersSql.SetValues("@SignatureImageSnapshot", SqlDbType.VarBinary, signatureImage)
            };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
                return 0;

            return Convert.ToInt32(table.Rows[0]["Result"]);
        }



        // Reject a letter request (Admin)
        public int RejectRequest(int letterRequestId,int rejectedByUserId,string rejectionReason)
        {
            const string query = @"
        SET NOCOUNT ON;
        SET XACT_ABORT ON;

        BEGIN TRY
            BEGIN TRANSACTION;

            UPDATE dbo.LetterRequests
            SET
                Status = 'Rejected',
                RejectionReason = @RejectionReason,
                RejectedByUserId = @RejectedByUserId,
                RejectedAt = SYSDATETIME()
            WHERE LetterRequestId = @LetterRequestId
              AND Status = 'Pending';

            IF @@ROWCOUNT = 0
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 0;
                RETURN;
            END;

            COMMIT TRANSACTION;

            SELECT 1;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            THROW;
        END CATCH;";

            var parameters = new List<DbParametersSql.QueryParameters>
    {
        DbParametersSql.SetValues("@LetterRequestId", SqlDbType.Int, letterRequestId),
        DbParametersSql.SetValues("@RejectedByUserId", SqlDbType.Int, rejectedByUserId),
        DbParametersSql.SetValues("@RejectionReason", SqlDbType.NVarChar, rejectionReason)
    };

            object result = _baseSqlDL.ExecuteScalar(query, parameters);

            return result != null && result != DBNull.Value
                ? Convert.ToInt32(result)
                : 0;
        }


        // Cancel a letter request (Employee)
        public int CancelRequest(int letterRequestId,int userId)
        {
            const string query = @"
        SET NOCOUNT ON;
        SET XACT_ABORT ON;

        BEGIN TRY
            BEGIN TRANSACTION;

            UPDATE dbo.LetterRequests
            SET
                Status = 'Cancelled',
                CancelledByUserId = @UserId,
                CancelledAt = SYSDATETIME()
            WHERE LetterRequestId = @LetterRequestId
              AND UserId = @UserId
              AND Status = 'Pending';

            IF @@ROWCOUNT = 0
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 0;
                RETURN;
            END;

            COMMIT TRANSACTION;

            SELECT 1;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            THROW;
        END CATCH;";

            var parameters = new List<DbParametersSql.QueryParameters>
    {
        DbParametersSql.SetValues("@LetterRequestId", SqlDbType.Int, letterRequestId),
        DbParametersSql.SetValues("@UserId", SqlDbType.Int, userId)
    };

            object result = _baseSqlDL.ExecuteScalar(query, parameters);

            return result != null && result != DBNull.Value
                ? Convert.ToInt32(result)
                : 0;
        }


        // GET approved letter's PDF reference for download (Employee)

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


        // GET user's full name by user ID (Admin)

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


        public int MarkAsPrinted(int letterRequestId, int printedByUserId)
        {
            const string query = @"
        SET NOCOUNT ON;
        SET XACT_ABORT ON;

        UPDATE dbo.LetterRequests
        SET
            IsPrinted = 1,
            PrintedByUserId = @PrintedByUserId,
            PrintedAt = SYSDATETIME()
        WHERE LetterRequestId = @LetterRequestId
          AND Status = 'Approved'
          AND CopyType = 'Hard Copy'
          AND IsPrinted = 0;

        SELECT @@ROWCOUNT;";

            var parameters = new List<DbParametersSql.QueryParameters>
    {
        DbParametersSql.SetValues(
            "@LetterRequestId", SqlDbType.Int, letterRequestId),

        DbParametersSql.SetValues(
            "@PrintedByUserId", SqlDbType.Int, printedByUserId)
    };

            object result = _baseSqlDL.ExecuteScalar(query, parameters);

            return result != null && result != DBNull.Value
                ? Convert.ToInt32(result)
                : 0;
        }


        // GET approved letter content and signature image for download (Employee)

        public ApprovedLetterDownloadDto? GetApprovedLetterForDownload(int letterRequestId,int userId)
        {
            const string query = @"
        SELECT
            LR.LetterContentSnapshot AS LetterContent,
            LR.LetterTypeName,
            EmpUser.FullName AS EmployeeName,
            LA.SignatureImageSnapshot,
            AdminUser.FullName AS ApproverName,
            LA.ApprovedAt
        FROM dbo.LetterRequests AS LR
        INNER JOIN dbo.LetterApprovals AS LA
            ON LR.LetterRequestId = LA.LetterRequestId
        INNER JOIN dbo.Users AS AdminUser
            ON LA.ApprovedByUserId = AdminUser.UserId
        INNER JOIN dbo.Users AS EmpUser
            ON LR.UserId = EmpUser.UserId
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
                row["SignatureImageSnapshot"] == DBNull.Value ||
                row["ApproverName"] == DBNull.Value ||
                row["ApprovedAt"] == DBNull.Value)
            {
                return null;
            }

            return new ApprovedLetterDownloadDto
            {
                LetterContent = row["LetterContent"].ToString() ?? string.Empty,
                LetterTypeName = row["LetterTypeName"]?.ToString() ?? string.Empty,
                EmployeeName = row["EmployeeName"]?.ToString() ?? string.Empty,
                SignatureImage = (byte[])row["SignatureImageSnapshot"],
                ApproverName = row["ApproverName"].ToString() ?? string.Empty,
                ApprovedAt = Convert.ToDateTime(row["ApprovedAt"])
            };
        }
    }
}
