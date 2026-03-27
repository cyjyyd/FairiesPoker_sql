using FPServer.Cache;
using FPServer.Game;
using FPServer.Network;
using Microsoft.Extensions.Logging;
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

        public ChatHandler(MessageHandler messageHandler, ILoggerFactory loggerFactory, OnlineUserCache userCache, RoomManager roomManager)
        {
            _messageHandler = messageHandler;
            _logger = loggerFactory.CreateLogger<ChatHandler>();
            _userCache = userCache;
            _roomManager = roomManager;
        }

        public void Handle(ClientConnection client, int subCode, object value)
        {
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
                default:
                    _logger.LogWarning("未知聊天操作码: {SubCode}", subCode);
                    break;
            }
        }

        /// <summary>
        /// 处理发送聊天消息
        /// </summary>
        private void HandleSendChat(ClientConnection client, ChatDto chatDto)
        {
            if (chatDto == null || string.IsNullOrEmpty(chatDto.Text))
            {
                return;
            }

            var userDto = _userCache.GetUserData(client.UserId);
            if (userDto == null)
            {
                return;
            }

            // 填充消息信息
            chatDto.UserId = client.UserId;
            chatDto.UserName = userDto.Name;
            chatDto.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            _logger.LogDebug("用户 {UserId} 发送聊天消息: {Text}", client.UserId, chatDto.Text);

            switch (chatDto.ChatType)
            {
                case ChatTypes.WORLD:
                    // 全服聊天 - 广播给所有在线用户
                    var worldMsg = new SocketMsg(OpCode.CHAT, ChatCode.RECEIVE_BRO, chatDto);
                    _messageHandler.Broadcast(worldMsg);
                    break;

                case ChatTypes.ROOM:
                    // 房间聊天 - 广播给房间内用户
                    var room = _roomManager.GetRoomByPlayerId(client.UserId);
                    if (room != null)
                    {
                        var roomMsg = new SocketMsg(OpCode.CHAT, ChatCode.RECEIVE_BRO, chatDto);
                        _messageHandler.BroadcastTo(room.GetPlayerIds(), roomMsg);
                    }
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
                    }
                    break;
            }

            // 发送成功响应
            var responseMsg = new SocketMsg(OpCode.CHAT, ChatCode.SEND_SRES, chatDto);
            _messageHandler.Send(client, responseMsg);
        }
    }
}