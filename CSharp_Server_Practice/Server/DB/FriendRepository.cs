using Microsoft.Data.Sqlite;

namespace Server.DB
{
    class FriendRepository
    {
        private readonly string _connectionString;

        public FriendRepository(string path = "users.db")
        {
            _connectionString = $"Data Source={path}";

            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Friends (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                User1 TEXT NOT NULL,
                User2 TEXT NOT NULL,
                Status TEXT NOT NULL,
                FOREIGN KEY (User1) REFERENCES Users(Nickname),
                FOREIGN KEY (User2) REFERENCES Users(Nickname)
            )";
            cmd.ExecuteNonQuery();
        }

        public void SendFriendRequest(string from, string to)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Friends (User1, User2, Status) VALUES ($u1, $u2, 'Pending')";
            cmd.Parameters.AddWithValue("$u1", from);
            cmd.Parameters.AddWithValue("$u2", to);
            cmd.ExecuteNonQuery();
        }

        public void AcceptFriendRequest(string from, string to)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE Friends SET Status='Accepted' WHERE USER1=$u1 AND USER2=$u2 AND Status='Pending'";
            cmd.Parameters.AddWithValue("$u1", from);
            cmd.Parameters.AddWithValue("$u2", to);
            cmd.ExecuteNonQuery();
        }

        public List<string> GetFriends(string nickname)
        {
            List<string> friends = new List<string>();

            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT User2 From Friends WHERE User1=$nick AND Status='Accepted' UNION SELECT User1 FROM Friends WHERE User2=$nick AND Status='Accepted'";
            cmd.Parameters.AddWithValue("$nick", nickname);

            SqliteDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                friends.Add(reader.GetString(0));
            }

            return friends;
        }

        public void RemoveFriendRequest(string from, string to)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"DELETE FROM Friends WHERE (User1=$u1 AND User2=$u2) OR (User1=$u2 AND User2=$u2";
            cmd.Parameters.AddWithValue("$u1", from);
            cmd.Parameters.AddWithValue("$u2", to);
            cmd.ExecuteNonQuery();
        }

        public void BlockFriendRequest(string from, string to)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE Friends SET Status='Blocked' WHERE User1=$u1 AND User2=$u2";
            cmd.Parameters.AddWithValue("$u1", from);
            cmd.Parameters.AddWithValue("$u2", to);
            cmd.ExecuteNonQuery();
        }
    }
}
