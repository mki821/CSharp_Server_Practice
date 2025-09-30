using Microsoft.Data.Sqlite;

namespace Server
{
    class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string path = "users.db")
        {
            _connectionString = $"Data Source={path}";

            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Users (Nickname TEXT PRIMARY KET, IsOnline INTEGER)";
            cmd.ExecuteNonQuery();
        }

        public bool TryLogin(string nickname)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand checkCmd = connection.CreateCommand();
            checkCmd.CommandText = @"SELECT IsOnline FROM Users WHERE Nickname=$nick";
            checkCmd.Parameters.AddWithValue("$nick", nickname);

            var result = checkCmd.ExecuteScalar();
            if(result == null)
            {
                SqliteCommand insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"INSERT INTO Users (Nickname, IsOnline) VALUES ($nick, 1)";
                insertCmd.Parameters.AddWithValue("$nick", nickname);
                insertCmd.ExecuteNonQuery();

                return true;
            }
            else if(Convert.ToInt32(result) == 1)
            {
                return false;
            }
            else
            {
                SqliteCommand updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"UPDATE Users SET IsOnline=1 WHERE Nickname=$nick";
                updateCmd.Parameters.AddWithValue("$nick", nickname);
                updateCmd.ExecuteNonQuery();

                return true;
            }
        }

        public void Logout(string nickname)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE Users SET IsOnline=0 WHERE Nickname=$nick";
            cmd.Parameters.AddWithValue("$nick", nickname);
            cmd.ExecuteNonQuery();
        }
    }
}
