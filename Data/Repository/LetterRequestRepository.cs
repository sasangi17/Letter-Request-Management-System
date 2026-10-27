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

            var parameters =
                new List<DbParametersSql.QueryParameters>();

            DataTable table =
                _baseSqlDL.ExecuteReader(query, parameters);

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
                        '{{Department}}', COALESCE(u.Department, '')
                    ),
                    '{{Position}}', COALESCE(u.Position, '')
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
        DbParametersSql.SetValues(
            "@UserId", SqlDbType.Int, userId),

        DbParametersSql.SetValues(
            "@LetterTypeId", SqlDbType.Int, letterTypeId)
    };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
            {
                return 0;
            }

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
        DbParametersSql.SetValues(
            "@LetterRequestId",
            SqlDbType.Int,
            letterRequestId),

        DbParametersSql.SetValues(
            "@UserId",
            SqlDbType.Int,
            userId)
    };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = table.Rows[0];

            return new LetterRequestResponseDto
            {
                LetterRequestId =
                    Convert.ToInt32(row["LetterRequestId"]),

                LetterTypeId =
                    Convert.ToInt32(row["LetterTypeId"]),

                LetterTypeName =
                    row["LetterTypeName"]?.ToString() ?? string.Empty,

                RequestDate =
                    Convert.ToDateTime(row["RequestDate"]),

                Status =
                    row["Status"]?.ToString() ?? string.Empty,

                LetterContent =
                    row["LetterContent"]?.ToString() ?? string.Empty
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
        DbParametersSql.SetValues(
            "@UserId",
            SqlDbType.Int,
            userId)
    };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            var requests = new List<LetterRequestResponseDto>();

            foreach (DataRow row in table.Rows)
            {
                requests.Add(new LetterRequestResponseDto
                {
                    LetterRequestId =
                        Convert.ToInt32(row["LetterRequestId"]),

                    LetterTypeId =
                        Convert.ToInt32(row["LetterTypeId"]),

                    LetterTypeName =
                        row["LetterTypeName"]?.ToString() ?? string.Empty,

                    RequestDate =
                        Convert.ToDateTime(row["RequestDate"]),

                    Status =
                        row["Status"]?.ToString() ?? string.Empty,

                    LetterContent =
                        row["LetterContent"]?.ToString() ?? string.Empty
                });
            }

            return requests;
        }


        public AdminLetterRequestDto? GetAdminRequestById(int letterRequestId)
        {
            string query = @"
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
        FROM dbo.LetterRequests lr
        INNER JOIN dbo.Users u
            ON lr.UserId = u.UserId
        INNER JOIN dbo.LetterTypes lt
            ON lr.LetterTypeId = lt.LetterTypeId
        WHERE lr.LetterRequestId = @LetterRequestId;";

            var parameters = new List<DbParametersSql.QueryParameters>
    {
        DbParametersSql.SetValues(
            "@LetterRequestId", SqlDbType.Int, letterRequestId)
    };

            DataTable result = _baseSqlDL.ExecuteReader(query, parameters);

            if (result.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = result.Rows[0];

            return new AdminLetterRequestDto
            {
                LetterRequestId = Convert.ToInt32(row["LetterRequestId"]),
                UserId = Convert.ToInt32(row["UserId"]),
                EmpNo = Convert.ToString(row["EmpNo"]) ?? string.Empty,
                EmployeeName = Convert.ToString(row["EmployeeName"]) ?? string.Empty,
                Email = Convert.ToString(row["Email"]) ?? string.Empty,
                Department = Convert.ToString(row["Department"]) ?? string.Empty,
                LetterTypeName = Convert.ToString(row["LetterTypeName"]) ?? string.Empty,
                RequestDate = Convert.ToDateTime(row["RequestDate"]),
                Status = Convert.ToString(row["Status"]) ?? string.Empty,
                LetterContent = Convert.ToString(row["LetterContent"]) ?? string.Empty
            };
        }

        public int ApproveRequest(
            int letterRequestId,
            int approvedByUserId,
            string signatureReference)
        {
            string query = @"
        SET NOCOUNT ON;
        SET XACT_ABORT ON;

        DECLARE @ResultCode INT = 0;
        DECLARE @CurrentStatus NVARCHAR(20);

        BEGIN TRY
            BEGIN TRANSACTION;

            SELECT @CurrentStatus = Status
            FROM dbo.LetterRequests WITH (UPDLOCK, HOLDLOCK)
            WHERE LetterRequestId = @LetterRequestId;

            IF @CurrentStatus IS NULL
            BEGIN
                SET @ResultCode = 0;
            END
            ELSE IF @CurrentStatus <> 'Pending'
            BEGIN
                SET @ResultCode = 2;
            END
            ELSE
            BEGIN
                UPDATE dbo.LetterRequests
                SET Status = 'Approved'
                WHERE LetterRequestId = @LetterRequestId
                  AND Status = 'Pending';

                IF @@ROWCOUNT = 1
                BEGIN
                    INSERT INTO dbo.LetterApprovals
                    (
                        LetterRequestId,
                        ApprovedByUserId,
                        SignatureReference
                    )
                    VALUES
                    (
                        @LetterRequestId,
                        @ApprovedByUserId,
                        @SignatureReference
                    );

                    SET @ResultCode = 1;
                END
                ELSE
                BEGIN
                    SET @ResultCode = 2;
                END
            END;

            COMMIT TRANSACTION;

            SELECT @ResultCode AS ResultCode;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            THROW;
        END CATCH;";

            var parameters = new List<DbParametersSql.QueryParameters>
    {
        DbParametersSql.SetValues(
            "@LetterRequestId", SqlDbType.Int, letterRequestId),

        DbParametersSql.SetValues(
            "@ApprovedByUserId", SqlDbType.Int, approvedByUserId),

        DbParametersSql.SetValues(
            "@SignatureReference",
            SqlDbType.NVarChar,
            signatureReference.Trim())
    };

            DataTable result = _baseSqlDL.ExecuteReader(query, parameters);

            if (result.Rows.Count == 0)
            {
                return 0;
            }

            return Convert.ToInt32(result.Rows[0]["ResultCode"]);
        }


    }
}
