using System.Data;

namespace Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;

public class DbParametersSql
{
    public struct QueryParameters
    {
        public string name;
        public SqlDbType SqlDbType;
        public dynamic value;
    }

    public static QueryParameters SetValues(string name, SqlDbType type, dynamic value)
    {
        QueryParameters qParameters = new QueryParameters();
        qParameters.name = name;
        qParameters.SqlDbType = type;
        qParameters.value = value;
        return qParameters;
    }
}
