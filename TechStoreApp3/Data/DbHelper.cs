using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TechStoreApp3.Models;

namespace TechStoreApp3.Data
{
    public class DbHelper
    {
        // Строка подключения для базы RestaurantDB
        private const string ConnectionString = "Server=localhost;Port=3306;Database=restaurantdb;Uid=root;Pwd=vertrigo;";

        // Получить пользователя по логину и паролю
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

        // Получить все категории
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

        // Получить все блюда
        public List<Dish> GetAllDishes()
        {
            var dishes = new List<Dish>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT d.id, d.name, d.description, d.price, d.weight, d.photo, d.category_id, c.name as category_name
                    FROM dishes d
                    JOIN categories c ON d.category_id = c.id";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dishes.Add(MapDish(reader));
                    }
                }
            }
            return dishes;
        }

        // Получить блюда по категории
        public List<Dish> GetDishesByCategory(int categoryId)
        {
            var dishes = new List<Dish>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT d.id, d.name, d.description, d.price, d.weight, d.photo, d.category_id, c.name as category_name
                    FROM dishes d
                    JOIN categories c ON d.category_id = c.id
                    WHERE d.category_id = @catId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@catId", categoryId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dishes.Add(MapDish(reader));
                        }
                    }
                }
            }
            return dishes;
        }

        // Поиск блюд по названию (частичное совпадение)
        public List<Dish> SearchDishes(string searchText)
        {
            var dishes = new List<Dish>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT d.id, d.name, d.description, d.price, d.weight, d.photo, d.category_id, c.name as category_name
                    FROM dishes d
                    JOIN categories c ON d.category_id = c.id
                    WHERE d.name LIKE @search";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dishes.Add(MapDish(reader));
                        }
                    }
                }
            }
            return dishes;
        }

        // Получить блюдо по Id
        public Dish GetDishById(int id)
        {
            Dish dish = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT d.id, d.name, d.description, d.price, d.weight, d.photo, d.category_id, c.name as category_name
                    FROM dishes d
                    JOIN categories c ON d.category_id = c.id
                    WHERE d.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dish = MapDish(reader);
                        }
                    }
                }
            }
            return dish;
        }

        // Добавить блюдо
        public int AddDish(Dish dish)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO dishes (name, description, price, weight, photo, category_id)
                    VALUES (@name, @description, @price, @weight, @photo, @catId);
                    SELECT LAST_INSERT_ID();";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", dish.Name);
                    cmd.Parameters.AddWithValue("@description", dish.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", dish.Price);
                    cmd.Parameters.AddWithValue("@weight", dish.Weight ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", dish.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", dish.CategoryId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // Обновить блюдо
        public void UpdateDish(Dish dish)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE dishes
                    SET name = @name, description = @description, price = @price, weight = @weight, photo = @photo, category_id = @catId
                    WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", dish.Id);
                    cmd.Parameters.AddWithValue("@name", dish.Name);
                    cmd.Parameters.AddWithValue("@description", dish.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", dish.Price);
                    cmd.Parameters.AddWithValue("@weight", dish.Weight ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", dish.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", dish.CategoryId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Удалить блюдо
        public void DeleteDish(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM dishes WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Получить заказы для блюда
        public List<Order> GetOrdersByDish(int dishId)
        {
            var orders = new List<Order>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT o.id, o.dish_id, o.client_name, o.order_date, o.quantity,
                           d.id as dish_id, 
                           d.name as dish_name, 
                           d.description as dish_description,
                           d.price as dish_price, 
                           d.weight as dish_weight, 
                           d.photo as dish_photo, 
                           d.category_id as dish_category_id,
                           c.name as dish_category_name
                    FROM orders o
                    JOIN dishes d ON o.dish_id = d.id
                    JOIN categories c ON d.category_id = c.id
                    WHERE o.dish_id = @dishId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@dishId", dishId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dish = MapDish(reader, "dish_");
                            var order = new Order
                            {
                                Id = reader.GetInt32("id"),
                                DishId = reader.GetInt32("dish_id"),
                                ClientName = reader.GetString("client_name"),
                                OrderDate = reader.GetDateTime("order_date"),
                                Quantity = reader.GetInt32("quantity"),
                                Dish = dish
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        // Получить заказ по Id
        public Order GetOrderById(int id)
        {
            Order order = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT o.id, o.dish_id, o.client_name, o.order_date, o.quantity,
                           d.id as dish_id, 
                           d.name as dish_name, 
                           d.description as dish_description,
                           d.price as dish_price, 
                           d.weight as dish_weight, 
                           d.photo as dish_photo, 
                           d.category_id as dish_category_id,
                           c.name as dish_category_name
                    FROM orders o
                    JOIN dishes d ON o.dish_id = d.id
                    JOIN categories c ON d.category_id = c.id
                    WHERE o.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var dish = MapDish(reader, "dish_");
                            order = new Order
                            {
                                Id = reader.GetInt32("id"),
                                DishId = reader.GetInt32("dish_id"),
                                ClientName = reader.GetString("client_name"),
                                OrderDate = reader.GetDateTime("order_date"),
                                Quantity = reader.GetInt32("quantity"),
                                Dish = dish
                            };
                        }
                    }
                }
            }
            return order;
        }

        // Добавить заказ
        public void AddOrder(Order order)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO orders (dish_id, client_name, order_date, quantity)
                    VALUES (@dish_id, @client_name, @order_date, @quantity)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@dish_id", order.DishId);
                    cmd.Parameters.AddWithValue("@client_name", order.ClientName);
                    cmd.Parameters.AddWithValue("@order_date", order.OrderDate);
                    cmd.Parameters.AddWithValue("@quantity", order.Quantity);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Обновить заказ
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

        // Удалить заказ
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

        // Получить список клиентов (логины пользователей с ролью client)
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

        // Проверить, есть ли заказы у блюда
        public bool HasOrders(int dishId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT EXISTS(SELECT 1 FROM orders WHERE dish_id = @dishId)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@dishId", dishId);
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

        // Вспомогательный метод для маппинга Dish из DataReader
        private Dish MapDish(MySqlDataReader reader, string prefix = "")
        {
            return new Dish
            {
                Id = reader.GetInt32($"{prefix}id"),
                Name = reader.GetString($"{prefix}name"),
                Description = reader.IsDBNull(reader.GetOrdinal($"{prefix}description")) ? null : reader.GetString($"{prefix}description"),
                Price = reader.GetDecimal($"{prefix}price"),
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