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
                Console.WriteLine("  online       - 显示在线人数");
                Console.WriteLine("  avatar on    - 开启头像自动审核");
                Console.WriteLine("  avatar off   - 关闭头像自动审核");
                Console.WriteLine("  avatar       - 显示头像审核状态");
                Console.WriteLine("  avatar list  - 显示待审核头像列表");
                Console.WriteLine("  avatar ok <id>  - 通过指定头像审核");
                Console.WriteLine("  avatar no <id>  - 拒绝指定头像审核");
                Console.WriteLine("  avatar all   - 通过所有待审核头像");
                Console.WriteLine("  exit         - 停止服务器");
                Console.WriteLine();

                // 命令处理循环
                while (true)
                {
                    var input = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(input)) continue;

                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var cmd = parts[0].ToLower();

                    if (cmd == "exit" || cmd == "quit")
                    {
                        break;
                    }
                    else if (cmd == "online")
                    {
                        logger.LogInformation("当前在线人数: {Count}", server.GetOnlineCount());
                    }
                    else if (cmd == "avatar")
                    {
                        if (parts.Length == 1)
                        {
                            var status = server.GetAvatarHandler().AutoApprove ? "开启" : "关闭";
                            logger.LogInformation("头像自动审核状态: {Status}", status);
                        }
                        else if (parts[1].ToLower() == "on")
                        {
                            server.GetAvatarHandler().AutoApprove = true;
                            logger.LogInformation("头像自动审核已开启");
                        }
                        else if (parts[1].ToLower() == "off")
                        {
                            server.GetAvatarHandler().AutoApprove = false;
                            logger.LogInformation("头像自动审核已关闭，需手动审核");
                        }
                        else if (parts[1].ToLower() == "list")
                        {
                            var list = await server.GetAvatarHandler().GetPendingAvatarsAsync();
                            if (list.Count == 0)
                            {
                                Console.WriteLine("没有待审核的头像");
                            }
                            else
                            {
                                Console.WriteLine($"\n待审核头像列表 (共{list.Count}个):");
                                Console.WriteLine("----------------------------------------");
                                foreach (var avatar in list)
                                {
                                    Console.WriteLine($"ID: {avatar.Id} | 用户: {avatar.Username}({avatar.UserId}) | 时间: {avatar.UploadTime:yyyy-MM-dd HH:mm}");
                                }
                                Console.WriteLine("----------------------------------------");
                            }
                        }
                        else if (parts[1].ToLower() == "ok" && parts.Length >= 3)
                        {
                            if (int.TryParse(parts[2], out int avatarId))
                            {
                                var result = await server.GetAvatarHandler().ApproveAvatar(avatarId, 0);
                                if (result)
                                {
                                    logger.LogInformation("头像 {AvatarId} 审核通过", avatarId);
                                }
                                else
                                {
                                    Console.WriteLine($"审核失败，找不到ID为 {avatarId} 的待审核头像");
                                }
                            }
                            else
                            {
                                Console.WriteLine("无效的头像ID");
                            }
                        }
                        else if (parts[1].ToLower() == "no" && parts.Length >= 3)
                        {
                            if (int.TryParse(parts[2], out int avatarId))
                            {
                                var result = await server.GetAvatarHandler().RejectAvatar(avatarId, 0);
                                if (result)
                                {
                                    logger.LogInformation("头像 {AvatarId} 已拒绝", avatarId);
                                }
                                else
                                {
                                    Console.WriteLine($"拒绝失败，找不到ID为 {avatarId} 的待审核头像");
                                }
                            }
                            else
                            {
                                Console.WriteLine("无效的头像ID");
                            }
                        }
                        else if (parts[1].ToLower() == "all")
                        {
                            var count = await server.GetAvatarHandler().ApproveAllAsync(0);
                            logger.LogInformation("已批量通过 {Count} 个待审核头像", count);
                        }
                    }
                    else if (cmd == "help")
                    {
                        Console.WriteLine("命令:");
                        Console.WriteLine("  online       - 显示在线人数");
                        Console.WriteLine("  avatar on    - 开启头像自动审核");
                        Console.WriteLine("  avatar off   - 关闭头像自动审核");
                        Console.WriteLine("  avatar       - 显示头像审核状态");
                        Console.WriteLine("  avatar list  - 显示待审核头像列表");
                        Console.WriteLine("  avatar ok <id>  - 通过指定头像审核");
                        Console.WriteLine("  avatar no <id>  - 拒绝指定头像审核");
                        Console.WriteLine("  avatar all   - 通过所有待审核头像");
                        Console.WriteLine("  exit         - 停止服务器");
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
  },
  ""Avatar"": {
    ""AutoApprove"": true
  }
}";
            File.WriteAllText(path, defaultConfig);
        }
    }
}