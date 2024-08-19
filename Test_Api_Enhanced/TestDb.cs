using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using MySql.Data.MySqlClient;
using Xunit.Abstractions;

namespace Test_Api_Enhanced;
public class TestDb
{
    private MySqlConnection _connection = new();

	private string _conn_string;
    private ITestOutputHelper _output;

    public TestDb(ITestOutputHelper output)
    {
        _output = output;

		string username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
		string password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

		_output.WriteLine($"database username is {username} ");
		_output.WriteLine($"database password is: {password}");

		_conn_string = $"server=localhost;user={username};database=api_enhanced;port=3306;password={password}";

		MySqlConnection conn = new(_conn_string);
		_connection = conn;
    }

    [Fact]
    public void DatabaseConn()
    {
		try
		{
			_connection.Open();
			_output.WriteLine("Database opened.");

			var create_table = @"
				CREATE TABLE IF NOT EXISTS VoiceActors 
				(
					ActorID INT AUTO_INCREMENT PRIMARY KEY,
					last_name VARCHAR(50),
					first_name VARCHAR(50),
					popularity INT
				)";

			var create_table_command = new MySqlCommand(create_table, _connection);

			if (create_table_command != null)
			{
				create_table_command.ExecuteNonQuery();
				_output.WriteLine("Successfully created VoiceActors table");
			}
			else
			{
				_output.WriteLine("Either table is already created or something happned.");
			}
		}
		catch (Exception ex)
		{
			_output.WriteLine(ex.Message);
		}
	}
}
