using FPServer.Database;
using FPServer.Network;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FPServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 设置控制台编码
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("====================================");
            Console.WriteLine("  FairiesPoker Server v1.0");
            Console.WriteLine("====================================");
            Console.WriteLine();

            // 创建日志工厂
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var logger = loggerFactory.CreateLogger<Program>();

            // 检查并创建配置文件
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            if (!File.Exists(configPath))
            {
                logger.LogInformation("配置文件不存在，正在创建默认配置文件...");
                CreateDefaultConfig(configPath);
                logger.LogInformation("默认配置文件已创建: {Path}", configPath);
            }

            try
            {
                // 加载配置
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                // 初始化数据库
                logger.LogInformation("正在初始化数据库...");
                var dbInitializer = new DbInitializer(loggerFactory.CreateLogger<DbInitializer>());
                await dbInitializer.InitializeAsync();

                // 测试数据库连接
                if (await DbHelper.Instance.TestConnectionAsync())
                {
                    logger.LogInformation("数据库连接成功");
                }
                else
                {
                    logger.LogError("数据库连接失败，请检查配置");
                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();
                    return;
                }

                // 启动服务器
                var server = new ServerPeer(configuration, loggerFactory);
                server.Start();

                logger.LogInformation("服务器已启动，按任意键停止...");
                Console.WriteLine();
                Console.WriteLine("命令:");
                Console.WriteLine("  online - 显示在线人数");
                Console.WriteLine("  exit   - 停止服务器");
                Console.WriteLine();

                // 命令处理循环
                while (true)
                {
                    var input = Console.ReadLine()?.Trim().ToLower();

                    if (input == "exit" || input == "quit")
                    {
                        break;
                    }
                    else if (input == "online")
                    {
                        logger.LogInformation("当前在线人数: {Count}", server.GetOnlineCount());
                    }
                    else if (input == "help")
                    {
                        Console.WriteLine("命令:");
                        Console.WriteLine("  online - 显示在线人数");
                        Console.WriteLine("  exit   - 停止服务器");
                    }
                }

                // 停止服务器
                server.Stop();
                logger.LogInformation("服务器已关闭");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "服务器启动失败");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// 创建默认配置文件
        /// </summary>
        private static void CreateDefaultConfig(string path)
        {
            var defaultConfig = @"{
  ""Server"": {
    ""Host"": ""0.0.0.0"",
    ""Port"": 40960,
    ""MaxConnections"": 1000
  },
  ""Database"": {
    ""Host"": ""localhost"",
    ""Port"": 3306,
    ""Database"": ""fairiespoker"",
    ""Username"": ""your_username"",
    ""Password"": ""your_password""
  },
  ""Game"": {
    ""InitialBeans"": 1000,
    ""BeansPerWin"": 100,
    ""BeansPerLose"": 50
  }
}";
            File.WriteAllText(path, defaultConfig);
        }
    }
}