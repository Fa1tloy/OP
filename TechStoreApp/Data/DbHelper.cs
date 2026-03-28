using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using TechStoreApp.Models;

namespace TechStoreApp.Data
{
    public class DbHelper
    {
     
        private const string ConnectionString = "Server=localhost;Port=3306;Database=techstore;Uid=root;Pwd=vertrigo;";

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

        // Получить все товары
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT p.id, p.name, p.brand, p.price, p.power, p.photo, p.category_id, c.name as category_name
                    FROM products p
                    JOIN categories c ON p.category_id = c.id";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }
            return products;
        }

        // Получить товары по категории
        public List<Product> GetProductsByCategory(int categoryId)
        {
            var products = new List<Product>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT p.id, p.name, p.brand, p.price, p.power, p.photo, p.category_id, c.name as category_name
                    FROM products p
                    JOIN categories c ON p.category_id = c.id
                    WHERE p.category_id = @catId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@catId", categoryId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapProduct(reader));
                        }
                    }
                }
            }
            return products;
        }

        // Поиск товаров по названию (частичное совпадение)
        public List<Product> SearchProducts(string searchText)
        {
            var products = new List<Product>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT p.id, p.name, p.brand, p.price, p.power, p.photo, p.category_id, c.name as category_name
                    FROM products p
                    JOIN categories c ON p.category_id = c.id
                    WHERE p.name LIKE @search";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapProduct(reader));
                        }
                    }
                }
            }
            return products;
        }

        // Получить товар по Id
        public Product GetProductById(int id)
        {
            Product product = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT p.id, p.name, p.brand, p.price, p.power, p.photo, p.category_id, c.name as category_name
                    FROM products p
                    JOIN categories c ON p.category_id = c.id
                    WHERE p.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = MapProduct(reader);
                        }
                    }
                }
            }
            return product;
        }

        // Добавить товар
        public int AddProduct(Product product)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO products (name, brand, price, power, photo, category_id)
                    VALUES (@name, @brand, @price, @power, @photo, @catId);
                    SELECT LAST_INSERT_ID();";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", product.Name);
                    cmd.Parameters.AddWithValue("@brand", product.Brand ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", product.Price);
                    cmd.Parameters.AddWithValue("@power", product.Power ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", product.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", product.CategoryId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // Обновить товар
        public void UpdateProduct(Product product)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE products
                    SET name = @name, brand = @brand, price = @price, power = @power, photo = @photo, category_id = @catId
                    WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", product.Id);
                    cmd.Parameters.AddWithValue("@name", product.Name);
                    cmd.Parameters.AddWithValue("@brand", product.Brand ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", product.Price);
                    cmd.Parameters.AddWithValue("@power", product.Power ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", product.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", product.CategoryId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Удалить товар
        public void DeleteProduct(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM products WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Получить заказы для товара
        public List<Order> GetOrdersByProduct(int productId)
        {
            var orders = new List<Order>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
            SELECT o.id, o.product_id, o.client_name, o.order_date, o.quantity,
                   p.id as prod_id, 
                   p.name as prod_name, 
                   p.brand as prod_brand, 
                   p.price as prod_price, 
                   p.power as prod_power, 
                   p.photo as prod_photo, 
                   p.category_id as prod_category_id,
                   c.name as prod_category_name
            FROM orders o
            JOIN products p ON o.product_id = p.id
            JOIN categories c ON p.category_id = c.id
            WHERE o.product_id = @prodId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@prodId", productId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapProduct(reader, "prod_");
                            var order = new Order
                            {
                                Id = reader.GetInt32("id"),
                                ProductId = reader.GetInt32("product_id"),
                                ClientName = reader.GetString("client_name"),
                                OrderDate = reader.GetDateTime("order_date"),
                                Quantity = reader.GetInt32("quantity"),
                                Product = product
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        // Получить цену товара (для расчёта в заказах)
        public decimal GetProductPrice(int productId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT price FROM products WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", productId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        // Вспомогательный метод для маппинга Product из DataReader
        private Product MapProduct(MySqlDataReader reader, string prefix = "")
        {
            return new Product
            {
                Id = reader.GetInt32($"{prefix}id"),
                Name = reader.GetString($"{prefix}name"),
                Brand = reader.IsDBNull(reader.GetOrdinal($"{prefix}brand")) ? null : reader.GetString($"{prefix}brand"),
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
        // Data/DbHelper.cs (добавить методы)

        // Получить заказ по Id
        public Order GetOrderById(int id)
        {
            Order order = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
            SELECT o.id, o.product_id, o.client_name, o.order_date, o.quantity,
                   p.id as prod_id, 
                   p.name as prod_name, 
                   p.brand as prod_brand, 
                   p.price as prod_price, 
                   p.power as prod_power, 
                   p.photo as prod_photo, 
                   p.category_id as prod_category_id,
                   c.name as prod_category_name
            FROM orders o
            JOIN products p ON o.product_id = p.id
            JOIN categories c ON p.category_id = c.id
            WHERE o.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var product = MapProduct(reader, "prod_");
                            order = new Order
                            {
                                Id = reader.GetInt32("id"),
                                ProductId = reader.GetInt32("product_id"),
                                ClientName = reader.GetString("client_name"),
                                OrderDate = reader.GetDateTime("order_date"),
                                Quantity = reader.GetInt32("quantity"),
                                Product = product
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
            INSERT INTO orders (product_id, client_name, order_date, quantity)
            VALUES (@product_id, @client_name, @order_date, @quantity)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@product_id", order.ProductId);
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
        // Data/DbHelper.cs
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
      
        public bool HasOrders(int productId)
{
    using (var conn = new MySqlConnection(ConnectionString))
    {
        conn.Open();
        string sql = "SELECT EXISTS(SELECT 1 FROM orders WHERE product_id = @prodId)";
        using (var cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@prodId", productId);
            return Convert.ToBoolean(cmd.ExecuteScalar());
        }
    }
}
    }
}