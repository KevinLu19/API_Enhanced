using MySql.Data.MySqlClient;

namespace Api_Enhanced.Database;

public class MySqlDB
{
    private MySqlConnection _connection;
    private string _conn_string;

    public MySqlDB()
    {
		string? username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
		string? password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

        _conn_string = $"server=localhost;user={username};database=api_enhanced;port=3306;password={password}";

        _connection = new(_conn_string);

        try
        {
            _connection.Open();
            Console.WriteLine("Successfully openeded Database");

			var create_table = @"
				CREATE TABLE IF NOT EXISTS Popularity 
				(
					ActorID INT AUTO_INCREMENT PRIMARY KEY,
					last_name VARCHAR(50),
					first_name VARCHAR(50),
					popularity INT
				)";

			var create_table_command = new MySqlCommand(create_table, _connection);

			create_table_command.ExecuteNonQuery();
		}
		catch (Exception ex) 
		{
            Console.WriteLine(ex.Message);
        }

    }

    private void Insert()
    {

    }

    private void Update() { }

    private void Delete() { }

    public void Select()
    {

    }
}
