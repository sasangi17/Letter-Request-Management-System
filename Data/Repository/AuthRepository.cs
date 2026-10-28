using System.Data;
using Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;
using Letter_Request_Management_System.Models.Domain;

namespace Letter_Request_Management_System.Data.Repository
{
    public class AuthRepository
    {
        private readonly BaseSqlDL _baseSqlDL;

        public AuthRepository(BaseSqlDL baseSqlDL)
        {
            _baseSqlDL = baseSqlDL;
        }

        public UserMl? Login(string email, string password)
        {
            const string query = @"
                SELECT
                    u.UserId,
                    u.EmpNo,
                    u.Email,
                    u.Password,
                    u.FullName,
                    u.Department,
                    u.Position,
                    u.JoinDate,
                    u.MobileNo,
                    r.Role
                FROM dbo.Users AS u
                INNER JOIN dbo.Role AS r
                    ON u.UserId = r.UserId
                WHERE u.Email = @Email
                  AND u.Password = @Password
                ORDER BY r.Role;";

            var parameters =
                new List<DbParametersSql.QueryParameters>
                {
                    DbParametersSql.SetValues("@Email",SqlDbType.NVarChar,email.Trim()),
                    DbParametersSql.SetValues("@Password",SqlDbType.NVarChar,password)
                };

            DataTable table = _baseSqlDL.ExecuteReader(query, parameters);

            if (table.Rows.Count == 0)
            {
                return null;
            }

            UserMl? user = null;

            foreach (DataRow row in table.Rows)
            {
                if (user == null)
                {
                    user = new UserMl
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        EmpNo = row["EmpNo"]?.ToString()?? string.Empty,
                        Email = row["Email"]?.ToString()?? string.Empty,
                        Password = row["Password"]?.ToString()?? string.Empty,
                        FullName = row["FullName"]?.ToString()?? string.Empty,
                        Department = row.IsNull("Department")? null: row["Department"].ToString(),
                        Position = row.IsNull("Position")? null: row["Position"].ToString(),
                        JoinDate = row.IsNull("JoinDate")? null: Convert.ToDateTime(row["JoinDate"]),
                        MobileNo = row.IsNull("MobileNo")? null: row["MobileNo"].ToString()
                    };
                }

                string role = row["Role"]?.ToString()?? string.Empty;

                if (!string.IsNullOrWhiteSpace(role) &&!user.Roles.Contains(role,StringComparer.OrdinalIgnoreCase))
                {
                    user.Roles.Add(role);
                }
            }

            return user;
        }
    }
}
