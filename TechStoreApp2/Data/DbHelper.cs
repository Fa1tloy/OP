using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using TechStoreApp2.Models;

namespace TechStoreApp.Data
{
    public class DbHelper
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=computerclubdb;Uid=root;Pwd=vertrigo;";

        // ---------------------- РАБОТА С ПОЛЬЗОВАТЕЛЯМИ ----------------------
        public User GetUser(string login, string password)
        {
            User user = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT u.id, u.login, u.password, u.role_id, r.role_name
                    FROM users u
                    JOIN roles r ON u.role_id = r.id
                    WHERE u.login = @login AND u.password = @password";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", password);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = reader.GetInt32("id"),
                                Login = reader.GetString("login"),
                                Password = reader.GetString("password"),
                                RoleId = reader.GetInt32("role_id"),
                                Role = new Role
                                {
                                    Id = reader.GetInt32("role_id"),
                                    Name = reader.GetString("role_name")
                                }
                            };
                        }
                    }
                }
            }
            return user;
        }

        public List<string> GetClients()
        {
            var clients = new List<string>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT login
                    FROM users
                    WHERE role_id = (SELECT id FROM roles WHERE role_name = 'client')";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(reader.GetString("login"));
                    }
                }
            }
            return clients;
        }

        // ---------------------- РАБОТА С КАТЕГОРИЯМИ ----------------------
        public List<Category> GetCategories()
        {
            var categories = new List<Category>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT id, name FROM categories";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name")
                        });
                    }
                }
            }
            return categories;
        }

        // ---------------------- РАБОТА С КОМПЬЮТЕРАМИ ----------------------
        public List<Computer> GetAllComputers()
        {
            var computers = new List<Computer>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT c.id, c.name, c.specs, c.price_per_hour, c.weight, c.photo, c.category_id, cat.name as category_name
                    FROM computers c
                    JOIN categories cat ON c.category_id = cat.id";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        computers.Add(MapComputer(reader));
                    }
                }
            }
            return computers;
        }

        public List<Computer> GetComputersByCategory(int categoryId)
        {
            var computers = new List<Computer>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT c.id, c.name, c.specs, c.price_per_hour, c.weight, c.photo, c.category_id, cat.name as category_name
                    FROM computers c
                    JOIN categories cat ON c.category_id = cat.id
                    WHERE c.category_id = @catId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@catId", categoryId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            computers.Add(MapComputer(reader));
                        }
                    }
                }
            }
            return computers;
        }

        public List<Computer> SearchComputers(string searchText)
        {
            var computers = new List<Computer>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT c.id, c.name, c.specs, c.price_per_hour, c.weight, c.photo, c.category_id, cat.name as category_name
                    FROM computers c
                    JOIN categories cat ON c.category_id = cat.id
                    WHERE c.name LIKE @search";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            computers.Add(MapComputer(reader));
                        }
                    }
                }
            }
            return computers;
        }

        public Computer GetComputerById(int id)
        {
            Computer computer = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT c.id, c.name, c.specs, c.price_per_hour, c.weight, c.photo, c.category_id, cat.name as category_name
                    FROM computers c
                    JOIN categories cat ON c.category_id = cat.id
                    WHERE c.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            computer = MapComputer(reader);
                        }
                    }
                }
            }
            return computer;
        }

        public int AddComputer(Computer computer)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO computers (name, specs, price_per_hour, weight, photo, category_id)
                    VALUES (@name, @specs, @price_per_hour, @weight, @photo, @catId);
                    SELECT LAST_INSERT_ID();";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", computer.Name);
                    cmd.Parameters.AddWithValue("@specs", computer.Specs ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price_per_hour", computer.PricePerHour);
                    cmd.Parameters.AddWithValue("@weight", computer.Weight ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", computer.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", computer.CategoryId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateComputer(Computer computer)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE computers
                    SET name = @name, specs = @specs, price_per_hour = @price_per_hour, 
                        weight = @weight, photo = @photo, category_id = @catId
                    WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", computer.Id);
                    cmd.Parameters.AddWithValue("@name", computer.Name);
                    cmd.Parameters.AddWithValue("@specs", computer.Specs ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price_per_hour", computer.PricePerHour);
                    cmd.Parameters.AddWithValue("@weight", computer.Weight ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", computer.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", computer.CategoryId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteComputer(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM computers WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool HasSessions(int computerId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT EXISTS(SELECT 1 FROM sessions WHERE computer_id = @computerId)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@computerId", computerId);
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

        public decimal GetComputerPrice(int computerId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT price_per_hour FROM computers WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", computerId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        // ---------------------- РАБОТА С СЕАНСАМИ ----------------------
        public List<Session> GetSessionsByComputer(int computerId)
        {
            var sessions = new List<Session>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
            SELECT s.id, s.computer_id, s.client_name, s.session_date, s.hours,
                   c.id as comp_id, 
                   c.name as comp_name, 
                   c.specs as comp_specs, 
                   c.price_per_hour as comp_price_per_hour, 
                   c.weight as comp_weight, 
                   c.photo as comp_photo, 
                   c.category_id as comp_category_id,
                   cat.name as comp_category_name
            FROM sessions s
            JOIN computers c ON s.computer_id = c.id
            JOIN categories cat ON c.category_id = cat.id
            WHERE s.computer_id = @computerId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@computerId", computerId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var computer = MapComputer(reader, "comp_");
                            var session = new Session
                            {
                                Id = reader.GetInt32("id"),
                                ComputerId = reader.GetInt32("computer_id"),
                                ClientName = reader.GetString("client_name"),
                                SessionDate = reader.GetDateTime("session_date"),
                                Hours = reader.GetInt32("hours"),
                                Computer = computer
                            };
                            sessions.Add(session);
                        }
                    }
                }
            }
            return sessions;
        }

        public Session GetSessionById(int id)
        {
            Session session = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
            SELECT s.id, s.computer_id, s.client_name, s.session_date, s.hours,
                   c.id as comp_id, 
                   c.name as comp_name, 
                   c.specs as comp_specs, 
                   c.price_per_hour as comp_price_per_hour, 
                   c.weight as comp_weight, 
                   c.photo as comp_photo, 
                   c.category_id as comp_category_id,
                   cat.name as comp_category_name
            FROM sessions s
            JOIN computers c ON s.computer_id = c.id
            JOIN categories cat ON c.category_id = cat.id
            WHERE s.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var computer = MapComputer(reader, "comp_");
                            session = new Session
                            {
                                Id = reader.GetInt32("id"),
                                ComputerId = reader.GetInt32("computer_id"),
                                ClientName = reader.GetString("client_name"),
                                SessionDate = reader.GetDateTime("session_date"),
                                Hours = reader.GetInt32("hours"),
                                Computer = computer
                            };
                        }
                    }
                }
            }
            return session;
        }

        public void AddSession(Session session)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO sessions (computer_id, client_name, session_date, hours)
                    VALUES (@computer_id, @client_name, @session_date, @hours)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@computer_id", session.ComputerId);
                    cmd.Parameters.AddWithValue("@client_name", session.ClientName);
                    cmd.Parameters.AddWithValue("@session_date", session.SessionDate);
                    cmd.Parameters.AddWithValue("@hours", session.Hours);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateSession(Session session)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE sessions
                    SET client_name = @client_name, session_date = @session_date, hours = @hours
                    WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", session.Id);
                    cmd.Parameters.AddWithValue("@client_name", session.ClientName);
                    cmd.Parameters.AddWithValue("@session_date", session.SessionDate);
                    cmd.Parameters.AddWithValue("@hours", session.Hours);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteSession(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM sessions WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ---------------------- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ----------------------
        private Computer MapComputer(MySqlDataReader reader, string prefix = "")
        {
            return new Computer
            {
                Id = reader.GetInt32($"{prefix}id"),
                Name = reader.GetString($"{prefix}name"),
                Specs = reader.IsDBNull(reader.GetOrdinal($"{prefix}specs")) ? null : reader.GetString($"{prefix}specs"),
                PricePerHour = reader.GetDecimal($"{prefix}price_per_hour"),
                Weight = reader.IsDBNull(reader.GetOrdinal($"{prefix}weight")) ? null : reader.GetString($"{prefix}weight"),
                Photo = reader.IsDBNull(reader.GetOrdinal($"{prefix}photo")) ? null : reader.GetString($"{prefix}photo"),
                CategoryId = reader.GetInt32($"{prefix}category_id"),
                Category = new Category
                {
                    Id = reader.GetInt32($"{prefix}category_id"),
                    Name = reader.GetString($"{prefix}category_name")
                }
            };
        }
    }
}