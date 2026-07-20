using FPServer.Database;
using FPServer.Network;
using FPServer.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace FPServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            if (args.Length > 0 && args[0].ToLower() == "--test")
            {
                Console.WriteLine("====================================");
                Console.WriteLine("  FairiesPoker 游戏逻辑测试");
                Console.WriteLine("====================================");
                Console.WriteLine();
                MultiplayerGameTests.RunTests();
                return;
            }

            bool isInteractive = !Console.IsInputRedirected;

            if (!isInteractive)
            {
                var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(logDir);
                var logPath = Path.Combine(logDir, $"server-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                var fs = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                var sw = new StreamWriter(fs) { AutoFlush = true };
                Console.SetOut(sw);
                Console.SetError(sw);
            }

            if (isInteractive)
            {
                Console.WriteLine("====================================");
                Console.WriteLine("  FairiesPoker Server v1.0");
                Console.WriteLine("====================================");
                Console.WriteLine();
            }

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var logger = loggerFactory.CreateLogger<Program>();

            if (!isInteractive)
            {
                logger.LogInformation("非交互模式：Console 输出已重定向到日志文件");
            }

            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            if (!File.Exists(configPath))
            {
                logger.LogInformation("配置文件不存在，正在创建默认配置文件...");
                CreateDefaultConfig(configPath);
                logger.LogInformation("默认配置文件已创建: {Path}", configPath);
            }

            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                logger.LogInformation("正在初始化数据库...");
                var dbInitializer = new DbInitializer(loggerFactory.CreateLogger<DbInitializer>());
                await dbInitializer.InitializeAsync();

                if (await DbHelper.Instance.TestConnectionAsync())
                {
                    logger.LogInformation("数据库连接成功");
                }
                else
                {
                    logger.LogError("数据库连接失败，请检查配置");
                    if (isInteractive)
                    {
                        Console.WriteLine("按任意键退出...");
                        Console.ReadKey();
                    }
                    return;
                }

                var server = new ServerPeer(configuration, loggerFactory);
                server.Start();

                logger.LogInformation("服务器已启动");

                if (isInteractive)
                {
                    await RunInteractiveCommandLoop(server, logger);
                }
                else
                {
                    await RunHeadlessMode(server, logger);
                }

                server.Stop();
                logger.LogInformation("服务器已关闭");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "服务器启动失败");
                if (isInteractive)
                {
                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();
                }
            }
        }

        private static async Task RunInteractiveCommandLoop(ServerPeer server, ILogger<Program> logger)
        {
            Console.WriteLine("命令:");
            Console.WriteLine("  online       - 显示在线人数");
            Console.WriteLine("  test         - 运行游戏逻辑测试");
            Console.WriteLine("  avatar on    - 开启头像自动审核");
            Console.WriteLine("  avatar off   - 关闭头像自动审核");
            Console.WriteLine("  avatar       - 显示头像审核状态");
            Console.WriteLine("  avatar list  - 显示待审核头像列表");
            Console.WriteLine("  avatar ok <id>  - 通过指定头像审核");
            Console.WriteLine("  avatar no <id>  - 拒绝指定头像审核");
            Console.WriteLine("  avatar all   - 通过所有待审核头像");
            Console.WriteLine("  exit         - 停止服务器");
            Console.WriteLine();

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
                else if (cmd == "test")
                {
                    Console.WriteLine("\n运行游戏逻辑测试...\n");
                    MultiplayerGameTests.RunTests();
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
                    Console.WriteLine("  test         - 运行游戏逻辑测试");
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
        }

        private static async Task RunHeadlessMode(ServerPeer server, ILogger<Program> logger)
        {
            logger.LogInformation("非交互模式，命令控制台已禁用。发送 SIGTERM/Ctrl+C 停止服务器。");

            var shutdownTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                logger.LogInformation("收到 Ctrl+C，正在停止服务器...");
                shutdownTcs.TrySetResult();
            };

            try
            {
                PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx =>
                {
                    ctx.Cancel = true;
                    logger.LogInformation("收到 SIGTERM，正在停止服务器...");
                    shutdownTcs.TrySetResult();
                });
            }
            catch (PlatformNotSupportedException)
            {
                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    shutdownTcs.TrySetResult();
                };
            }

            await shutdownTcs.Task;
        }

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
    ""BaseStake"": 20,
    ""MaxMultiple"": 16,
    ""SingleGameLossLimitPercent"": 30,
    ""WeeklyReliefThreshold"": 500,
    ""WeeklyReliefBeans"": 1000,
    ""WeeklyReliefCooldownDays"": 7
  },
  ""Avatar"": {
    ""AutoApprove"": true
  }
}";
            File.WriteAllText(path, defaultConfig, System.Text.Encoding.UTF8);
        }
    }
}
