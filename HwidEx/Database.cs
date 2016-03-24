using MySql.Data.MySqlClient;
using System;

namespace HwidEx
{
    public sealed class Database
    {
        private readonly string m_connectionString;

        public Database(string connectionString)
        {
            m_connectionString = connectionString;
        }

        public bool Login(string user, string pass, string hwid)
        {
            bool toReturn = false;

            using (var connection = new MySqlConnection(m_connectionString))
            {
                connection.Open();

                MySqlCommand command = new MySqlCommand("SELECT expiry FROM users WHERE username =@username AND password =@password AND hwid =@hwid", connection);
                command.Parameters.AddWithValue("username", user);
                command.Parameters.AddWithValue("password", pass);
                command.Parameters.AddWithValue("hwid", hwid);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetDateTime("expiry") >= DateTime.Now)
                        {
                            toReturn = true;
                            break;
                        }
                    }
                }
            }

            return toReturn;
        }
    }
}