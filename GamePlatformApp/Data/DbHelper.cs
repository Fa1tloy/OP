using GamePlatformApp.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace GamePlatformApp.Data
{
    public class DbHelper
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=gameplatformdb;Uid=root;Pwd=vertrigo;";

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

        public List<Game> GetAllGames()
        {
            var games = new List<Game>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT g.id, g.name, g.developer, g.release_year, g.price, g.power, g.photo, g.category_id, c.name as category_name
                    FROM games g
                    JOIN categories c ON g.category_id = c.id";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        games.Add(MapGame(reader));
                    }
                }
            }
            return games;
        }

        public List<Game> GetGamesByCategory(int categoryId)
        {
            var games = new List<Game>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT g.id, g.name, g.developer, g.release_year, g.price, g.power, g.photo, g.category_id, c.name as category_name
                    FROM games g
                    JOIN categories c ON g.category_id = c.id
                    WHERE g.category_id = @catId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@catId", categoryId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            games.Add(MapGame(reader));
                        }
                    }
                }
            }
            return games;
        }

        public List<Game> SearchGames(string searchText)
        {
            var games = new List<Game>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT g.id, g.name, g.developer, g.release_year, g.price, g.power, g.photo, g.category_id, c.name as category_name
                    FROM games g
                    JOIN categories c ON g.category_id = c.id
                    WHERE g.name LIKE @search";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            games.Add(MapGame(reader));
                        }
                    }
                }
            }
            return games;
        }

        public Game GetGameById(int id)
        {
            Game game = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT g.id, g.name, g.developer, g.release_year, g.price, g.power, g.photo, g.category_id, c.name as category_name
                    FROM games g
                    JOIN categories c ON g.category_id = c.id
                    WHERE g.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            game = MapGame(reader);
                        }
                    }
                }
            }
            return game;
        }

        public int AddGame(Game game)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO games (name, developer, release_year, price, power, photo, category_id)
                    VALUES (@name, @developer, @release_year, @price, @power, @photo, @catId);
                    SELECT LAST_INSERT_ID();";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", game.Name);
                    cmd.Parameters.AddWithValue("@developer", game.Developer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@release_year", game.ReleaseYear.HasValue ? game.ReleaseYear.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", game.Price);
                    cmd.Parameters.AddWithValue("@power", game.Power ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", game.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", game.CategoryId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateGame(Game game)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE games
                    SET name = @name, developer = @developer, release_year = @release_year, price = @price, power = @power, photo = @photo, category_id = @catId
                    WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", game.Id);
                    cmd.Parameters.AddWithValue("@name", game.Name);
                    cmd.Parameters.AddWithValue("@developer", game.Developer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@release_year", game.ReleaseYear.HasValue ? game.ReleaseYear.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", game.Price);
                    cmd.Parameters.AddWithValue("@power", game.Power ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", game.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", game.CategoryId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteGame(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM games WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Order> GetOrdersByGame(int gameId)
        {
            var orders = new List<Order>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
            SELECT o.id, o.game_id, o.client_name, o.order_date, o.quantity,
                   g.id as game_id, 
                   g.name as game_name, 
                   g.developer as game_developer, 
                   g.release_year as game_release_year, 
                   g.price as game_price, 
                   g.power as game_power, 
                   g.photo as game_photo, 
                   g.category_id as game_category_id,
                   c.name as game_category_name
            FROM orders o
            JOIN games g ON o.game_id = g.id
            JOIN categories c ON g.category_id = c.id
            WHERE o.game_id = @gameId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@gameId", gameId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = MapGame(reader, "game_");
                            var order = new Order
                            {
                                Id = reader.GetInt32("id"),
                                GameId = reader.GetInt32("game_id"),
                                ClientName = reader.GetString("client_name"),
                                OrderDate = reader.GetDateTime("order_date"),
                                Quantity = reader.GetInt32("quantity"),
                                Game = game
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        public decimal GetGamePrice(int gameId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT price FROM games WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", gameId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        public Order GetOrderById(int id)
        {
            Order order = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
            SELECT o.id, o.game_id, o.client_name, o.order_date, o.quantity,
                   g.id as game_id, 
                   g.name as game_name, 
                   g.developer as game_developer, 
                   g.release_year as game_release_year, 
                   g.price as game_price, 
                   g.power as game_power, 
                   g.photo as game_photo, 
                   g.category_id as game_category_id,
                   c.name as game_category_name
            FROM orders o
            JOIN games g ON o.game_id = g.id
            JOIN categories c ON g.category_id = c.id
            WHERE o.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var game = MapGame(reader, "game_");
                            order = new Order
                            {
                                Id = reader.GetInt32("id"),
                                GameId = reader.GetInt32("game_id"),
                                ClientName = reader.GetString("client_name"),
                                OrderDate = reader.GetDateTime("order_date"),
                                Quantity = reader.GetInt32("quantity"),
                                Game = game
                            };
                        }
                    }
                }
            }
            return order;
        }

        public void AddOrder(Order order)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO orders (game_id, client_name, order_date, quantity)
                    VALUES (@game_id, @client_name, @order_date, @quantity)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@game_id", order.GameId);
                    cmd.Parameters.AddWithValue("@client_name", order.ClientName);
                    cmd.Parameters.AddWithValue("@order_date", order.OrderDate);
                    cmd.Parameters.AddWithValue("@quantity", order.Quantity);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateOrder(Order order)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE orders
                    SET client_name = @client_name, order_date = @order_date, quantity = @quantity
                    WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", order.Id);
                    cmd.Parameters.AddWithValue("@client_name", order.ClientName);
                    cmd.Parameters.AddWithValue("@order_date", order.OrderDate);
                    cmd.Parameters.AddWithValue("@quantity", order.Quantity);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteOrder(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM orders WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
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

        public bool HasOrders(int gameId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT EXISTS(SELECT 1 FROM orders WHERE game_id = @gameId)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@gameId", gameId);
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

        private Game MapGame(MySqlDataReader reader, string prefix = "")
        {
            int? releaseYear = null;
            if (!reader.IsDBNull(reader.GetOrdinal($"{prefix}release_year")))
                releaseYear = reader.GetInt32($"{prefix}release_year");

            return new Game
            {
                Id = reader.GetInt32($"{prefix}id"),
                Name = reader.GetString($"{prefix}name"),
                Developer = reader.IsDBNull(reader.GetOrdinal($"{prefix}developer")) ? null : reader.GetString($"{prefix}developer"),
                ReleaseYear = releaseYear,
                Price = reader.GetDecimal($"{prefix}price"),
                Power = reader.IsDBNull(reader.GetOrdinal($"{prefix}power")) ? null : reader.GetString($"{prefix}power"),
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