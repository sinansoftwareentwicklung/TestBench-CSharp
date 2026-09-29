using Microsoft.Data.Sqlite;

namespace TestBench.DeviceServer;

public class Database
{
    private readonly string _connectionString =
    $"Data Source={Path.Combine(AppContext.BaseDirectory, "testbench.db")}";

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS TestRuns
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SerialNumber TEXT NOT NULL,
                StartTimestamp TEXT NOT NULL,
                EndTimestamp TEXT,
                Result TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS LedResults
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TestRunId INTEGER NOT NULL,
                Step TEXT NOT NULL,
                LedNumber INTEGER NOT NULL,
                Voltage REAL NOT NULL,
                Result TEXT NOT NULL,

                FOREIGN KEY (TestRunId)
                    REFERENCES TestRuns(Id)
            );
            """;

        command.ExecuteNonQuery();
    }
	public long StartTestRun(string serialNumber)
	{
		using var connection = new SqliteConnection(_connectionString);
		connection.Open();

		var command = connection.CreateCommand();

		command.CommandText = """
			INSERT INTO TestRuns
			(
				SerialNumber,
				StartTimestamp,
				Result
			)
			VALUES
			(
				$serialNumber,
				$startTimestamp,
				$result
			);

			SELECT last_insert_rowid();
			""";

		command.Parameters.AddWithValue("$serialNumber", serialNumber);
		command.Parameters.AddWithValue("$startTimestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
		command.Parameters.AddWithValue("$result", "RUNNING");

		return (long)command.ExecuteScalar()!;
	}
		
		public void SaveLedResult(
			long testRunId,
			string step,
			int ledNumber,
			double voltage,
			bool passed)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();

			var command = connection.CreateCommand();

			command.CommandText = """
				INSERT INTO LedResults
				(
					TestRunId,
					Step,
					LedNumber,
					Voltage,
					Result
				)
				VALUES
				(
					$testRunId,
					$step,
					$ledNumber,
					$voltage,
					$result
				);
				""";

			command.Parameters.AddWithValue("$testRunId", testRunId);
			command.Parameters.AddWithValue("$step", step);
			command.Parameters.AddWithValue("$ledNumber", ledNumber);
			command.Parameters.AddWithValue("$voltage", voltage);
			command.Parameters.AddWithValue("$result", passed ? "PASS" : "FAIL");

			command.ExecuteNonQuery();
	
	}
	public void CompleteTestRun(
		long testRunId,
		bool passed)
	{
		using var connection = new SqliteConnection(_connectionString);
		connection.Open();

		var command = connection.CreateCommand();

		command.CommandText = """
			UPDATE TestRuns
			SET
				EndTimestamp = $endTimestamp,
				Result = $result
			WHERE Id = $testRunId;
			""";

		command.Parameters.AddWithValue(
			"$endTimestamp",
			DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

		command.Parameters.AddWithValue(
			"$result",
			passed ? "PASS" : "FAIL");

		command.Parameters.AddWithValue("$testRunId", testRunId);

		command.ExecuteNonQuery();
	}
}