using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto;
using Protocol.Constant;

namespace FairiesPoker.MG.Network.Impl
{

public class ChatHandler : HandlerBase
{
    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case ChatCode.RECEIVE_BRO:
                {
                    // 收到聊天消息广播
                    ChatDto dto = value as ChatDto;
                    if (dto != null)
                    {
                        Models.TriggerChatMessage(dto);
                    }
                    break;
                }
            case ChatCode.SEND_SRES:
                {
                    // 发送消息响应（可选处理）
                    break;
                }
            case ChatCode.GET_HISTORY_SRES:
                {
                    // 获取历史消息响应
                    var messages = value as List<ChatDto>;
                    if (messages != null)
                    {
                        // 根据第一条消息时间判断是否是今日消息加载
                        bool isAppend = false;
                        if (messages.Count > 0)
                        {
                            // 使用本地时区时间判断（与服务端一致）
                            var firstMsgTime = DateTimeOffset.FromUnixTimeMilliseconds(messages[0].Timestamp).ToLocalTime();
                            var today = DateTime.Today;
                            isAppend = firstMsgTime.Date < today;
                        }
                        Models.TriggerChatHistory(messages, isAppend);
                    }
                    break;
                }
            case ChatCode.PUSH_TODAY_SRES:
                {
                    // 登录时推送今日消息（服务端主动推送，可能早于Lobby创建）
                    var messages = value as List<ChatDto>;
                    if (messages != null)
                    {
                        Models.TriggerChatHistory(messages, false);
                    }
                    break;
                }
            case ChatCode.GET_PRIVATE_USERS_SRES:
                {
                    // 获取私聊历史用户列表响应
                    var users = value as List<UserDto>;
                    if (users != null)
                    {
                        Models.TriggerPrivateUsers(users);
                    }
                    break;
                }
            default:
                break;
        }
    }
}
}
