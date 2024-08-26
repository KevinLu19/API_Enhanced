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

// Interface for loading data
public interface IDataInsert
{ 
	public void Select();
}

public class TestDb : IDataInsert
{
    private MySqlConnection _connection;

	private string _conn_string;
    private ITestOutputHelper _output;

    public TestDb(ITestOutputHelper output)
    {
        _output = output;

		string? username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
		string? password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

		//_output.WriteLine($"database username is {username} ");
		//_output.WriteLine($"database password is: {password}");

		_conn_string = $"server=localhost;user={username};database=api_enhanced;port=3306;password={password}";

		_connection = new(_conn_string);
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

			create_table_command.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			_output.WriteLine(ex.Message);
		}
	}

	[Fact]
	public void Update() 
	{
		
	}

	[Fact]
	public void Delete()
	{

	}

	[Fact]

	// Function taken from data inserter interface.
	public void Select()
	{
		try
		{
			_connection.Open();
			_output.WriteLine("Connection opened from Select() function. Will try and print data");

			var command = _connection.CreateCommand();

			command.CommandText = "SELECT * FROM voiceactors";

			var reader = command.ExecuteReader();

			while(reader.Read()) 
			{
				var id = reader.GetInt32(0);
				var last_name = reader.GetString(1);
				var first_name = reader.GetString(2);
				var popularity = reader.GetInt32(3);

				_output.WriteLine($"ActorID: {id}, last_name: {last_name}, first_name: {first_name}, popularity: {popularity}");
			}			

			_connection.Close();
		}
		catch (Exception e)
		{
			_output.WriteLine(e.Message);
		}
	}

	//[Fact]
	public void Insert()
	{
		// Open connection 
		try
		{
			_connection.Open();

			_output.WriteLine("Connection opened from Insert() function. Will try and insert data.");

			// Insert dummy data
			var command = _connection.CreateCommand();

			command.CommandText = @"
            INSERT INTO voiceactors (last_name, first_name, popularity) VALUES 
            ('takahashi', 'rei', 58716),
            ('itou', 'miku', 6069),
            ('kitou', 'akari', 10087);
			";

			command.ExecuteNonQuery();	

			_output.WriteLine("Inserted dummy data into the database table");

			_connection.Close();
		}
		catch (Exception e)
		{
			_output.WriteLine(e.Message);
		}
		
	}
}
