using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using TechStoreApp4.Models;

namespace TechStoreApp4.Data
{
    public class DbHelper
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=BeautySalonDB;Uid=root;Pwd=vertrigo;";

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

        public List<Service> GetAllServices()
        {
            var services = new List<Service>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT s.id, s.name, s.description, s.price, s.duration, s.photo, s.category_id, c.name as category_name
                    FROM services s
                    JOIN categories c ON s.category_id = c.id";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        services.Add(MapService(reader));
                    }
                }
            }
            return services;
        }

        public List<Service> GetServicesByCategory(int categoryId)
        {
            var services = new List<Service>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT s.id, s.name, s.description, s.price, s.duration, s.photo, s.category_id, c.name as category_name
                    FROM services s
                    JOIN categories c ON s.category_id = c.id
                    WHERE s.category_id = @catId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@catId", categoryId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            services.Add(MapService(reader));
                        }
                    }
                }
            }
            return services;
        }

        public List<Service> SearchServices(string searchText)
        {
            var services = new List<Service>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT s.id, s.name, s.description, s.price, s.duration, s.photo, s.category_id, c.name as category_name
                    FROM services s
                    JOIN categories c ON s.category_id = c.id
                    WHERE s.name LIKE @search";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            services.Add(MapService(reader));
                        }
                    }
                }
            }
            return services;
        }

        public Service GetServiceById(int id)
        {
            Service service = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT s.id, s.name, s.description, s.price, s.duration, s.photo, s.category_id, c.name as category_name
                    FROM services s
                    JOIN categories c ON s.category_id = c.id
                    WHERE s.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            service = MapService(reader);
                        }
                    }
                }
            }
            return service;
        }

        public int AddService(Service service)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO services (name, description, price, duration, photo, category_id)
                    VALUES (@name, @description, @price, @duration, @photo, @catId);
                    SELECT LAST_INSERT_ID();";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", service.Name);
                    cmd.Parameters.AddWithValue("@description", service.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", service.Price);
                    cmd.Parameters.AddWithValue("@duration", service.Duration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", service.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", service.CategoryId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateService(Service service)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE services
                    SET name = @name, description = @description, price = @price, duration = @duration, photo = @photo, category_id = @catId
                    WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", service.Id);
                    cmd.Parameters.AddWithValue("@name", service.Name);
                    cmd.Parameters.AddWithValue("@description", service.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", service.Price);
                    cmd.Parameters.AddWithValue("@duration", service.Duration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@photo", service.Photo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@catId", service.CategoryId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteService(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM services WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Appointment> GetAppointmentsByService(int serviceId)
        {
            var appointments = new List<Appointment>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT a.id, a.service_id, a.client_name, a.appointment_date, a.sessions_count,
                           s.id as serv_id, s.name as serv_name, s.description as serv_description,
                           s.price as serv_price, s.duration as serv_duration, s.photo as serv_photo,
                           s.category_id as serv_category_id, c.name as serv_category_name
                    FROM appointments a
                    JOIN services s ON a.service_id = s.id
                    JOIN categories c ON s.category_id = c.id
                    WHERE a.service_id = @servId";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@servId", serviceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var service = MapService(reader, "serv_");
                            var appointment = new Appointment
                            {
                                Id = reader.GetInt32("id"),
                                ServiceId = reader.GetInt32("service_id"),
                                ClientName = reader.GetString("client_name"),
                                AppointmentDate = reader.GetDateTime("appointment_date"),
                                SessionsCount = reader.GetInt32("sessions_count"),
                                Service = service
                            };
                            appointments.Add(appointment);
                        }
                    }
                }
            }
            return appointments;
        }

        public decimal GetServicePrice(int serviceId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT price FROM services WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", serviceId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        public Appointment GetAppointmentById(int id)
        {
            Appointment appointment = null;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT a.id, a.service_id, a.client_name, a.appointment_date, a.sessions_count,
                           s.id as serv_id, s.name as serv_name, s.description as serv_description,
                           s.price as serv_price, s.duration as serv_duration, s.photo as serv_photo,
                           s.category_id as serv_category_id, c.name as serv_category_name
                    FROM appointments a
                    JOIN services s ON a.service_id = s.id
                    JOIN categories c ON s.category_id = c.id
                    WHERE a.id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var service = MapService(reader, "serv_");
                            appointment = new Appointment
                            {
                                Id = reader.GetInt32("id"),
                                ServiceId = reader.GetInt32("service_id"),
                                ClientName = reader.GetString("client_name"),
                                AppointmentDate = reader.GetDateTime("appointment_date"),
                                SessionsCount = reader.GetInt32("sessions_count"),
                                Service = service
                            };
                        }
                    }
                }
            }
            return appointment;
        }

        public void AddAppointment(Appointment appointment)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO appointments (service_id, client_name, appointment_date, sessions_count)
                    VALUES (@service_id, @client_name, @appointment_date, @sessions_count)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@service_id", appointment.ServiceId);
                    cmd.Parameters.AddWithValue("@client_name", appointment.ClientName);
                    cmd.Parameters.AddWithValue("@appointment_date", appointment.AppointmentDate);
                    cmd.Parameters.AddWithValue("@sessions_count", appointment.SessionsCount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateAppointment(Appointment appointment)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE appointments
                    SET client_name = @client_name, appointment_date = @appointment_date, sessions_count = @sessions_count
                    WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", appointment.Id);
                    cmd.Parameters.AddWithValue("@client_name", appointment.ClientName);
                    cmd.Parameters.AddWithValue("@appointment_date", appointment.AppointmentDate);
                    cmd.Parameters.AddWithValue("@sessions_count", appointment.SessionsCount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAppointment(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM appointments WHERE id = @id";
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

        public bool HasAppointments(int serviceId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT EXISTS(SELECT 1 FROM appointments WHERE service_id = @servId)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@servId", serviceId);
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

        private Service MapService(MySqlDataReader reader, string prefix = "")
        {
            return new Service
            {
                Id = reader.GetInt32($"{prefix}id"),
                Name = reader.GetString($"{prefix}name"),
                Description = reader.IsDBNull(reader.GetOrdinal($"{prefix}description")) ? null : reader.GetString($"{prefix}description"),
                Price = reader.GetDecimal($"{prefix}price"),
                Duration = reader.IsDBNull(reader.GetOrdinal($"{prefix}duration")) ? null : reader.GetString($"{prefix}duration"),
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