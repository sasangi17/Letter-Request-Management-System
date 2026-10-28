using System.Data;
using Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;
using Letter_Request_Management_System.Models.Domain;

namespace Letter_Request_Management_System.Data.Repository
{
    public class LetterTypeRepository
    {
        private readonly BaseSqlDL _baseSqlDL;

        public LetterTypeRepository(BaseSqlDL baseSqlDL)
        {
            _baseSqlDL = baseSqlDL;
        }

        public List<LetterTypeMl> GetAllLetterTypes()
        {
            string query = @"
                SELECT
                    LetterTypeId,
                    LetterTypeName,
                    Description,
                    TemplateContent
                FROM dbo.LetterTypes
                ORDER BY LetterTypeName;";

            var parameters = new List<DbParametersSql.QueryParameters>();

            DataTable dataTable = _baseSqlDL.ExecuteReader(query, parameters);

            var letterTypes = new List<LetterTypeMl>();

            foreach (DataRow row in dataTable.Rows)
            {
                letterTypes.Add(new LetterTypeMl
                {
                    LetterTypeId = Convert.ToInt32(row["LetterTypeId"]),
                    LetterTypeName = Convert.ToString(row["LetterTypeName"]) ?? string.Empty,
                    Description = row["Description"] == DBNull.Value? null: Convert.ToString(row["Description"]),
                    TemplateContent = Convert.ToString(row["TemplateContent"]) ?? string.Empty
                });
            }

            return letterTypes;
        }
    }
}