using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;

namespace FPServer.Database
{
    /// <summary>
    /// 数据库帮助类
    /// </summary>
    public class DbHelper
    {
        private static DbHelper _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString;

        public static DbHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DbHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        private DbHelper()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var dbConfig = configuration.GetSection("Database");
            var server = dbConfig["Host"];
            var port = dbConfig["Port"];
            var database = dbConfig["Database"];
            var username = dbConfig["Username"];
            var password = dbConfig["Password"];

            _connectionString = $"Server={server};Port={port};Database={database};User Id={username};Password={password};Charset=utf8mb4;SslMode=None;AllowPublicKeyRetrieval=True;";
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public async Task<MySqlConnection> GetConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        /// <summary>
        /// 执行非查询SQL
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string sql, params MySqlParameter[] parameters)
        {
            using var connection = await GetConnectionAsync();
            using var command = new MySqlCommand(sql, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 执行查询返回单值
        /// </summary>
        public async Task<object> ExecuteScalarAsync(string sql, params MySqlParameter[] parameters)
        {
            using var connection = await GetConnectionAsync();
            using var command = new MySqlCommand(sql, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
            return await command.ExecuteScalarAsync();
        }

        /// <summary>
        /// 执行查询返回DataReader
        /// </summary>
        public async Task<MySqlDataReader> ExecuteReaderAsync(string sql, params MySqlParameter[] parameters)
        {
            var connection = await GetConnectionAsync();
            var command = new MySqlCommand(sql, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = await GetConnectionAsync();
                return connection.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}