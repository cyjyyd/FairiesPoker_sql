using FPServer.Cache;
using FPServer.Database;
using FPServer.Game;
using FPServer.Network;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Protocol.Code;
using Protocol.Dto;

namespace FPServer.Handlers
{
    /// <summary>
    /// 聊天处理器
    /// </summary>
    public class ChatHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly ILogger<ChatHandler> _logger;
        private readonly OnlineUserCache _userCache;
        private readonly RoomManager _roomManager;

        // 30天消息保留时间（毫秒）
        private const long MESSAGE_RETENTION_MS = 30L * 24 * 60 * 60 * 1000;

        public ChatHandler(MessageHandler messageHandler, ILoggerFactory loggerFactory, OnlineUserCache userCache, RoomManager roomManager)
        {
            _messageHandler = messageHandler;
            _logger = loggerFactory.CreateLogger<ChatHandler>();
            _userCache = userCache;
            _roomManager = roomManager;
        }

        public void Handle(ClientConnection client, int subCode, object value)
        {
            _logger.LogInformation("ChatHandler收到请求: UserId={UserId}, SubCode={SubCode}", client.UserId, subCode);

            if (client.UserId <= 0)
            {
                _logger.LogWarning("未登录用户尝试聊天操作");
                return;
            }

            switch (subCode)
            {
                case ChatCode.SEND_CREQ:
                    HandleSendChat(client, value as ChatDto);
                    break;
                case ChatCode.GET_HISTORY_CREQ:
                    _logger.LogInformation("处理获取历史消息请求");
                    HandleGetHistory(client, value as HistoryRequestDto);
                    break;
                case ChatCode.GET_PRIVATE_USERS_CREQ:
                    _logger.LogInformation("处理获取私聊历史用户请求");
                    HandleGetPrivateUsers(client);
                    break;
                default:
                    _logger.LogWarning("未知聊天操作码: {SubCode}", subCode);
                    break;
            }
        }

        /// <summary>
        /// 处理发送聊天消息
        /// </summary>
        private async void HandleSendChat(ClientConnection client, ChatDto chatDto)
        {
            if (chatDto == null || string.IsNullOrEmpty(chatDto.Text))
            {
                return;
            }

            // 消息长度限制
            if (chatDto.Text.Length > 500)
            {
                chatDto.Text = chatDto.Text.Substring(0, 500);
            }

            var userDto = _userCache.GetUserData(client.UserId);
            if (userDto == null)
            {
                return;
            }

            // 填充消息信息 - 使用本地时区时间戳
            chatDto.UserId = client.UserId;
            chatDto.UserName = userDto.Name;
            chatDto.Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _logger.LogDebug("用户 {UserId} 发送聊天消息: {Text}", client.UserId, chatDto.Text);

            switch (chatDto.ChatType)
            {
                case ChatTypes.WORLD:
                    // 全服聊天 - 广播给所有在线用户
                    var worldMsg = new SocketMsg(OpCode.CHAT, ChatCode.RECEIVE_BRO, chatDto);
                    _messageHandler.Broadcast(worldMsg);
                    // 保存到数据库
                    await SaveMessageToDatabase(chatDto);
                    break;

                case ChatTypes.ROOM:
                    // 房间聊天 - 广播给房间内用户
                    var room = _roomManager.GetRoomByPlayerId(client.UserId);
                    if (room != null)
                    {
                        var roomMsg = new SocketMsg(OpCode.CHAT, ChatCode.RECEIVE_BRO, chatDto);
                        _messageHandler.BroadcastTo(room.GetPlayerIds(), roomMsg);
                    }
                    // 房间聊天不保存到数据库
                    break;

                case ChatTypes.PRIVATE:
                    // 私聊 - 发送给目标用户
                    if (chatDto.TargetUserId > 0)
                    {
                        var privateMsg = new SocketMsg(OpCode.CHAT, ChatCode.RECEIVE_BRO, chatDto);
                        var targetClient = _messageHandler.GetClient(chatDto.TargetUserId);
                        if (targetClient != null)
                        {
                            _messageHandler.Send(targetClient, privateMsg);
                        }
                        // 同时发给自己
                        _messageHandler.Send(client, privateMsg);
                        // 保存到数据库
                        await SaveMessageToDatabase(chatDto);
                    }
                    break;
            }

            // 发送成功响应
            var responseMsg = new SocketMsg(OpCode.CHAT, ChatCode.SEND_SRES, chatDto);
            _messageHandler.Send(client, responseMsg);
        }

        /// <summary>
        /// 处理获取历史消息请求
        /// </summary>
        private async void HandleGetHistory(ClientConnection client, HistoryRequestDto requestDto)
        {
            if (requestDto == null)
            {
                requestDto = new HistoryRequestDto();
            }

            // 历史消息仅支持全服和私聊
            if (requestDto.ChatType != ChatTypes.WORLD && requestDto.ChatType != ChatTypes.PRIVATE)
            {
                _logger.LogWarning("不支持的历史消息类型: {ChatType}", requestDto.ChatType);
                return;
            }

            try
            {
                List<ChatDto> messages;

                // 如果 beforeTimestamp = 0 且是全服消息，返回今日消息
                if (requestDto.BeforeTimestamp == 0 && requestDto.ChatType == ChatTypes.WORLD)
                {
                    messages = await GetTodayMessages(client.UserId);
                    _logger.LogDebug("用户 {UserId} 获取今日消息: {Count}条", client.UserId, messages.Count);
                }
                else
                {
                    messages = await GetHistoryMessagesAsync(client.UserId, requestDto);
                    _logger.LogDebug("用户 {UserId} 获取历史消息: {Count}条", client.UserId, messages.Count);
                }

                var responseMsg = new SocketMsg(OpCode.CHAT, ChatCode.GET_HISTORY_SRES, messages);
                _messageHandler.Send(client, responseMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取历史消息失败");
            }
        }

        /// <summary>
        /// 处理获取私聊历史用户列表请求
        /// </summary>
        private async void HandleGetPrivateUsers(ClientConnection client)
        {
            try
            {
                var privateUsers = await GetPrivateHistoryUsersAsync(client.UserId);
                var responseMsg = new SocketMsg(OpCode.CHAT, ChatCode.GET_PRIVATE_USERS_SRES, privateUsers);
                _messageHandler.Send(client, responseMsg);
                _logger.LogDebug("用户 {UserId} 获取私聊历史用户列表: {Count}人", client.UserId, privateUsers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取私聊历史用户列表失败");
            }
        }

        /// <summary>
        /// 保存消息到数据库
        /// </summary>
        private async Task SaveMessageToDatabase(ChatDto chatDto)
        {
            try
            {
                await DbHelper.Instance.ExecuteNonQueryAsync(
                    @"INSERT INTO chat_messages (chat_type, user_id, user_name, target_user_id, text, timestamp)
                      VALUES (@chatType, @userId, @userName, @targetUserId, @text, @timestamp)",
                    new MySqlParameter("@chatType", chatDto.ChatType),
                    new MySqlParameter("@userId", chatDto.UserId),
                    new MySqlParameter("@userName", chatDto.UserName ?? ""),
                    new MySqlParameter("@targetUserId", chatDto.TargetUserId),
                    new MySqlParameter("@text", chatDto.Text),
                    new MySqlParameter("@timestamp", chatDto.Timestamp)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存聊天消息失败");
            }
        }

        /// <summary>
        /// 获取历史消息
        /// </summary>
        private async Task<List<ChatDto>> GetHistoryMessagesAsync(int userId, HistoryRequestDto requestDto)
        {
            var messages = new List<ChatDto>();
            // 使用本地时区的截止时间
            var cutoffTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - MESSAGE_RETENTION_MS;

            try
            {
                string sql;
                MySqlParameter[] parameters;

                if (requestDto.ChatType == ChatTypes.PRIVATE && requestDto.TargetUserId > 0)
                {
                    // 私聊历史：获取与目标用户的所有私聊消息
                    sql = @"SELECT user_id, user_name, target_user_id, text, timestamp
                            FROM chat_messages
                            WHERE chat_type = ?chatType
                            AND ((user_id = ?userId AND target_user_id = ?targetUserId)
                                 OR (user_id = ?targetUserId AND target_user_id = ?userId))
                            AND timestamp > ?cutoffTime
                            AND timestamp < ?beforeTimestamp
                            ORDER BY timestamp DESC
                            LIMIT ?limit";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("?chatType", MySqlDbType.Int32) { Value = ChatTypes.PRIVATE },
                        new MySqlParameter("?userId", MySqlDbType.Int32) { Value = userId },
                        new MySqlParameter("?targetUserId", MySqlDbType.Int32) { Value = requestDto.TargetUserId },
                        new MySqlParameter("?cutoffTime", MySqlDbType.Int64) { Value = cutoffTime },
                        new MySqlParameter("?beforeTimestamp", MySqlDbType.Int64) { Value = requestDto.BeforeTimestamp > 0 ? requestDto.BeforeTimestamp : DateTimeOffset.Now.ToUnixTimeMilliseconds() },
                        new MySqlParameter("?limit", MySqlDbType.Int32) { Value = requestDto.Limit > 0 ? requestDto.Limit : 20 }
                    };
                }
                else
                {
                    // 全服历史
                    sql = @"SELECT user_id, user_name, target_user_id, text, timestamp
                            FROM chat_messages
                            WHERE chat_type = ?chatType
                            AND timestamp > ?cutoffTime
                            AND timestamp < ?beforeTimestamp
                            ORDER BY timestamp DESC
                            LIMIT ?limit";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("?chatType", MySqlDbType.Int32) { Value = ChatTypes.WORLD },
                        new MySqlParameter("?cutoffTime", MySqlDbType.Int64) { Value = cutoffTime },
                        new MySqlParameter("?beforeTimestamp", MySqlDbType.Int64) { Value = requestDto.BeforeTimestamp > 0 ? requestDto.BeforeTimestamp : DateTimeOffset.Now.ToUnixTimeMilliseconds() },
                        new MySqlParameter("?limit", MySqlDbType.Int32) { Value = requestDto.Limit > 0 ? requestDto.Limit : 20 }
                    };
                }

                using var reader = await DbHelper.Instance.ExecuteReaderAsync(sql, parameters);
                while (await reader.ReadAsync())
                {
                    var chatDto = new ChatDto
                    {
                        ChatType = requestDto.ChatType,
                        UserId = reader.GetInt32("user_id"),
                        UserName = reader.GetString("user_name"),
                        TargetUserId = reader.GetInt32("target_user_id"),
                        Text = reader.GetString("text"),
                        Timestamp = reader.GetInt64("timestamp")
                    };
                    messages.Add(chatDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询历史消息失败");
            }

            // 按时间正序返回（从旧到新）
            messages.Reverse();
            return messages;
        }

        /// <summary>
        /// 获取今日消息（用于登录时推送）
        /// </summary>
        public async Task<List<ChatDto>> GetTodayMessages(int userId)
        {
            var messages = new List<ChatDto>();
            // 使用本地时区的今日零点时间戳
            var now = DateTimeOffset.Now;
            var todayStart = new DateTimeOffset(now.Date, now.Offset);
            var todayStartMs = todayStart.ToUnixTimeMilliseconds();

            _logger.LogInformation("获取今日消息: 本地今日零点时间戳={TodayStartMs}, 当前本地时间戳={NowMs}, 当前时间={Now}",
                todayStartMs, now.ToUnixTimeMilliseconds(), now.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                // 获取今日全服消息 - 明确指定参数类型
                var sql = @"SELECT id, chat_type, user_id, user_name, target_user_id, text, timestamp
                            FROM chat_messages
                            WHERE chat_type = ?chatType
                            AND timestamp >= ?todayStart
                            ORDER BY timestamp ASC
                            LIMIT 100";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("?chatType", MySqlDbType.Int32) { Value = ChatTypes.WORLD },
                    new MySqlParameter("?todayStart", MySqlDbType.Int64) { Value = todayStartMs }
                };

                _logger.LogInformation("执行SQL: {Sql}, chatType={ChatType}, todayStart={TodayStart}",
                    sql, ChatTypes.WORLD, todayStartMs);

                using var reader = await DbHelper.Instance.ExecuteReaderAsync(sql, parameters);
                while (await reader.ReadAsync())
                {
                    var chatDto = new ChatDto
                    {
                        ChatType = reader.GetInt32("chat_type"),
                        UserId = reader.GetInt32("user_id"),
                        UserName = reader.GetString("user_name"),
                        TargetUserId = reader.GetInt32("target_user_id"),
                        Text = reader.GetString("text"),
                        Timestamp = reader.GetInt64("timestamp")
                    };
                    messages.Add(chatDto);
                    _logger.LogInformation("读取到消息: id={Id}, timestamp={Timestamp}, text={Text}",
                        reader.GetInt32("id"), chatDto.Timestamp, chatDto.Text);
                }
                _logger.LogInformation("获取到全服今日消息 {Count} 条", messages.Count);

                // 获取今日与该用户相关的私聊消息
                sql = @"SELECT id, chat_type, user_id, user_name, target_user_id, text, timestamp
                        FROM chat_messages
                        WHERE chat_type = ?chatType
                        AND (user_id = ?userId OR target_user_id = ?userId)
                        AND timestamp >= ?todayStart
                        ORDER BY timestamp ASC
                        LIMIT 50";
                parameters = new MySqlParameter[]
                {
                    new MySqlParameter("?chatType", MySqlDbType.Int32) { Value = ChatTypes.PRIVATE },
                    new MySqlParameter("?userId", MySqlDbType.Int32) { Value = userId },
                    new MySqlParameter("?todayStart", MySqlDbType.Int64) { Value = todayStartMs }
                };

                using var reader2 = await DbHelper.Instance.ExecuteReaderAsync(sql, parameters);
                while (await reader2.ReadAsync())
                {
                    var chatDto = new ChatDto
                    {
                        ChatType = reader2.GetInt32("chat_type"),
                        UserId = reader2.GetInt32("user_id"),
                        UserName = reader2.GetString("user_name"),
                        TargetUserId = reader2.GetInt32("target_user_id"),
                        Text = reader2.GetString("text"),
                        Timestamp = reader2.GetInt64("timestamp")
                    };
                    messages.Add(chatDto);
                }

                // 按时间排序
                messages.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
                _logger.LogInformation("获取今日消息总数: {Count}条", messages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取今日消息失败");
            }

            return messages;
        }

        /// <summary>
        /// 获取私聊历史用户列表（与当前用户有过私聊的用户）
        /// </summary>
        private async Task<List<UserDto>> GetPrivateHistoryUsersAsync(int userId)
        {
            var users = new List<UserDto>();
            var userIdSet = new HashSet<int>();
            var cutoffTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - MESSAGE_RETENTION_MS;

            try
            {
                // 查询与当前用户有过私聊的所有用户ID
                var sql = @"SELECT DISTINCT
                            CASE
                                WHEN user_id = ?userId THEN target_user_id
                                ELSE user_id
                            END AS other_user_id
                            FROM chat_messages
                            WHERE chat_type = ?chatType
                            AND (user_id = ?userId OR target_user_id = ?userId)
                            AND timestamp > ?cutoffTime
                            AND target_user_id > 0";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("?chatType", MySqlDbType.Int32) { Value = ChatTypes.PRIVATE },
                    new MySqlParameter("?userId", MySqlDbType.Int32) { Value = userId },
                    new MySqlParameter("?cutoffTime", MySqlDbType.Int64) { Value = cutoffTime }
                };

                using var reader = await DbHelper.Instance.ExecuteReaderAsync(sql, parameters);
                while (await reader.ReadAsync())
                {
                    var otherUserId = reader.GetInt32("other_user_id");
                    if (otherUserId > 0 && otherUserId != userId)
                    {
                        userIdSet.Add(otherUserId);
                    }
                }

                // 获取这些用户的详细信息
                foreach (var otherUserId in userIdSet)
                {
                    var userDto = _userCache.GetUserData(otherUserId);
                    if (userDto != null)
                    {
                        // 在线用户，使用缓存数据
                        users.Add(userDto);
                    }
                    else
                    {
                        // 离线用户，从数据库获取
                        var userSql = "SELECT id, nickname, beans, win_count, lose_count, run_count, level, exp FROM users WHERE id = ?uid";
                        var userParams = new MySqlParameter[] { new MySqlParameter("?uid", MySqlDbType.Int32) { Value = otherUserId } };
                        using var userReader = await DbHelper.Instance.ExecuteReaderAsync(userSql, userParams);
                        if (await userReader.ReadAsync())
                        {
                            users.Add(new UserDto(
                                userReader.GetInt32("id"),
                                userReader.GetString("nickname"),
                                userReader.GetInt32("beans"),
                                userReader.GetInt32("win_count"),
                                userReader.GetInt32("lose_count"),
                                userReader.GetInt32("run_count"),
                                userReader.GetInt32("level"),
                                userReader.GetInt32("exp")
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取私聊历史用户列表失败");
            }

            return users;
        }
    }
}