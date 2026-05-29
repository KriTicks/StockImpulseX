using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using StockImpulseX.Models;

namespace StockImpulseX.Services
{
    public class DatabaseService
    {
        private string connectionString;

        public DatabaseService()
        {
            string dbPath = Path.Combine("DB", "store.db");
            Directory.CreateDirectory("DB");
            connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT,
                    ManufactureDate TEXT NOT NULL,
                    ExpiryDate TEXT NOT NULL,
                    Location TEXT NOT NULL,
                    Quantity INTEGER NOT NULL
                )";
            command.ExecuteNonQuery();
        }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Products";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    ManufactureDate = DateTime.Parse(reader.GetString(3)),
                    ExpiryDate = DateTime.Parse(reader.GetString(4)),
                    Location = reader.GetString(5),
                    Quantity = reader.GetInt32(6)
                });
            }
            return products;
        }

        public void DeleteProduct(int id)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Products WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
        public void AddProduct(Product product)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        INSERT INTO Products (Name, Category, ManufactureDate, ExpiryDate, Location, Quantity) 
        VALUES ($name, $category, $manDate, $expDate, $loc, $qty)";

            command.Parameters.AddWithValue("$name", product.Name);
            command.Parameters.AddWithValue("$category", product.Category ?? "");
            command.Parameters.AddWithValue("$manDate", product.ManufactureDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("$expDate", product.ExpiryDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("$loc", product.Location);
            command.Parameters.AddWithValue("$qty", product.Quantity);

            command.ExecuteNonQuery();
        }
    }
}