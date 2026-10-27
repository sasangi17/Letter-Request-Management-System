using Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Letter_Request_Management_System.Data.Repository.BaseSQLDbConnection;

public class BaseSqlDL
{
    private readonly IConfiguration configuration;

    public BaseSqlDL(IConfiguration configuration)
    {
        this.configuration = configuration.GetSection("ConnectionStrings");
        this.connection = this.configuration["EmployeeLetterDbContextConnection"];

    }

    public string connection;
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(connection);
    }
    public object ExecuteScalar(string query, List<DbParametersSql.QueryParameters> parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(connection))
        {
            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var para in parameters)
                            cmd.Parameters.Add(para.name, para.SqlDbType).Value = para.value;
                    }

                    return cmd.ExecuteScalar(); // Returns the first column of the first row
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    public int ExecuteNonQuery(string query, List<DbParametersSql.QueryParameters> parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(connection))
        {
            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var para in parameters)
                            cmd.Parameters.Add(para.name, para.SqlDbType).Value = para.value;
                    }

                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    public async Task<DataTable> ExecuteReaderAsync(string query, List<DbParametersSql.QueryParameters> parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(connection))
        {
            try
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var para in parameters)
                            cmd.Parameters.Add(para.name, para.SqlDbType).Value = para.value;
                    }

                    var reader = await cmd.ExecuteReaderAsync();
                    var dt = new DataTable();
                    dt.Load(reader);

                    return dt;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    public DataTable ExecuteReader(string query, List<DbParametersSql.QueryParameters> parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(connection))
        {
            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var para in parameters)
                            cmd.Parameters.Add(para.name, para.SqlDbType).Value = para.value;
                    }

                    var reader = cmd.ExecuteReader();
                    var dt = new DataTable();
                    dt.Load(reader);

                    return dt;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}