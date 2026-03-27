using FPServer.Cache;
using FPServer.Database;
using FPServer.Network;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Protocol.Code;
using Protocol.Dto;

namespace FPServer.Handlers
{
    /// <summary>
    /// 账号处理器 - 处理登录注册
    /// </summary>
    public class AccountHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly ILogger<AccountHandler> _logger;
        private readonly OnlineUserCache _userCache;

        public AccountHandler(MessageHandler messageHandler, ILoggerFactory loggerFactory, OnlineUserCache userCache)
        {
            _messageHandler = messageHandler;
            _logger = loggerFactory.CreateLogger<AccountHandler>();
            _userCache = userCache;
        }

        /// <summary>
        /// 处理账号相关消息
        /// </summary>
        public void Handle(ClientConnection client, int subCode, object value)
        {
            switch (subCode)
            {
                case AccountCode.REGIST_CREQ:
                    HandleRegister(client, value as AccountDto);
                    break;
                case AccountCode.LOGIN:
                    HandleLogin(client, value as AccountDto);
                    break;
                default:
                    _logger.LogWarning("未知账号操作码: {SubCode}", subCode);
                    break;
            }
        }

        /// <summary>
        /// 处理注册请求
        /// </summary>
        private async void HandleRegister(ClientConnection client, AccountDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Account) || string.IsNullOrEmpty(dto.Password))
            {
                SendRegisterResponse(client, -2); // 输入不合法
                return;
            }

            string username = dto.Account.Trim();
            string password = dto.Password; // 客户端已经MD5加密

            // 验证用户名长度
            if (username.Length < 4 || username.Length > 16)
            {
                SendRegisterResponse(client, -2);
                return;
            }

            try
            {
                // 检查用户名是否已存在
                var existsObj = await DbHelper.Instance.ExecuteScalarAsync(
                    "SELECT id FROM users WHERE username = @username",
                    new MySqlParameter("@username", username));

                if (existsObj != null)
                {
                    SendRegisterResponse(client, -1); // 用户名已存在
                    return;
                }

                // 使用BCrypt加密密码（对客户端传来的MD5再加密）
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, 12);

                // 插入新用户
                await DbHelper.Instance.ExecuteNonQueryAsync(
                    @"INSERT INTO users (username, password_hash, avatar_id, nickname, beans, created_at)
                      VALUES (@username, @password, @avatarId, @nickname, 1000, NOW())",
                    new MySqlParameter("@username", username),
                    new MySqlParameter("@password", hashedPassword),
                    new MySqlParameter("@avatarId", dto.avatarid >= 0 ? dto.avatarid : 0),
                    new MySqlParameter("@nickname", username));

                _logger.LogInformation("新用户注册成功: {Username}", username);
                SendRegisterResponse(client, 0); // 注册成功
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册失败: {Username}", username);
                SendRegisterResponse(client, -3); // 服务器错误
            }
        }

        /// <summary>
        /// 处理登录请求
        /// </summary>
        private async void HandleLogin(ClientConnection client, AccountDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Account) || string.IsNullOrEmpty(dto.Password))
            {
                SendLoginErrorResponse(client, -3);
                return;
            }

            string username = dto.Account.Trim();
            string password = dto.Password;

            try
            {
                // 查询用户
                using var reader = await DbHelper.Instance.ExecuteReaderAsync(
                    "SELECT id, username, password_hash, avatar_id, nickname, beans, win_count, lose_count, run_count, level, exp, is_online FROM users WHERE username = @username",
                    new MySqlParameter("@username", username));

                if (!await reader.ReadAsync())
                {
                    SendLoginErrorResponse(client, -1); // 用户不存在
                    return;
                }

                int userId = reader.GetInt32("id");
                string storedHash = reader.GetString("password_hash");
                int avatarId = reader.GetInt32("avatar_id");
                int nicknameOrdinal = reader.GetOrdinal("nickname");
                string nickname = reader.IsDBNull(nicknameOrdinal) ? username : reader.GetString("nickname");
                int beans = reader.GetInt32("beans");
                int winCount = reader.GetInt32("win_count");
                int loseCount = reader.GetInt32("lose_count");
                int runCount = reader.GetInt32("run_count");
                int level = reader.GetInt32("level");
                int exp = reader.GetInt32("exp");
                bool isOnline = reader.GetBoolean("is_online");

                reader.Close();

                // 检查是否已在线
                if (isOnline && _userCache.IsOnline(userId))
                {
                    SendLoginErrorResponse(client, -2); // 已在线
                    return;
                }

                // 验证密码
                if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                {
                    SendLoginErrorResponse(client, -3); // 密码错误
                    return;
                }

                // 更新登录状态
                await DbHelper.Instance.ExecuteNonQueryAsync(
                    "UPDATE users SET is_online = 1, last_login = NOW() WHERE id = @id",
                    new MySqlParameter("@id", userId));

                // 创建用户数据
                var userDto = new UserDto(userId, nickname, beans, winCount, loseCount, runCount, level, exp);

                // 添加到在线缓存
                _userCache.AddUser(userId, client, userDto);

                _logger.LogInformation("用户登录成功: {UserId} {Username}", userId, username);

                // 发送登录成功响应
                var resultMsg = new SocketMsg(OpCode.ACCOUNT, AccountCode.LOGIN, 0);
                _messageHandler.Send(client, resultMsg);

                // 发送用户数据
                var userMsg = new SocketMsg(OpCode.USER, UserCode.GET_INFO_SRES, userDto);
                _messageHandler.Send(client, userMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败: {Username}", username);
                SendLoginErrorResponse(client, -3);
            }
        }

        private void SendRegisterResponse(ClientConnection client, int result)
        {
            var msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.REGIST_SRES, result);
            _messageHandler.Send(client, msg);
        }

        private void SendLoginErrorResponse(ClientConnection client, int errorCode)
        {
            var msg = new SocketMsg(OpCode.ACCOUNT, AccountCode.LOGIN, errorCode);
            _messageHandler.Send(client, msg);
        }
    }
}