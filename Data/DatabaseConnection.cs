using MySqlConnector;

// Link to the MySQL Connector
namespace HMS.Data
{
    public class DatabaseConnection
    {
        MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            Database = "hms",
            UserID = "root",
            Password = "password",
        };

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(builder.ConnectionString);
        }
    }
}