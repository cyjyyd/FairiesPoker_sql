using Microsoft.Extensions.Logging;

namespace FPServer.Database
{
    /// <summary>
    /// 数据库初始化器
    /// </summary>
    public class DbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(ILogger<DbInitializer> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 初始化数据库表
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // 用户表
                var createUserTable = @"
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    avatar_id INT DEFAULT 0,
    nickname VARCHAR(50),
    beans INT DEFAULT 1000,
    win_count INT DEFAULT 0,
    lose_count INT DEFAULT 0,
    run_count INT DEFAULT 0,
    level INT DEFAULT 1,
    exp INT DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_login DATETIME NULL,
    is_online TINYINT DEFAULT 0,
    INDEX idx_username (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

                await DbHelper.Instance.ExecuteNonQueryAsync(createUserTable);
                _logger.LogInformation("用户表创建成功");

                // 用户会话表
                var createSessionTable = @"
CREATE TABLE IF NOT EXISTS user_sessions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    session_token VARCHAR(64) NOT NULL,
    login_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_active DATETIME NULL,
    ip_address VARCHAR(45),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_token (session_token),
    INDEX idx_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

                await DbHelper.Instance.ExecuteNonQueryAsync(createSessionTable);
                _logger.LogInformation("会话表创建成功");

                // 游戏记录表
                var createGameRecordTable = @"
CREATE TABLE IF NOT EXISTS game_records (
    id INT AUTO_INCREMENT PRIMARY KEY,
    room_id VARCHAR(50),
    player_ids VARCHAR(200),
    landlord_id INT,
    winner_ids VARCHAR(200),
    base_score INT DEFAULT 1,
    multiple INT DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_room (room_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

                await DbHelper.Instance.ExecuteNonQueryAsync(createGameRecordTable);
                _logger.LogInformation("游戏记录表创建成功");

                _logger.LogInformation("数据库初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据库初始化失败");
                throw;
            }
        }
    }
}