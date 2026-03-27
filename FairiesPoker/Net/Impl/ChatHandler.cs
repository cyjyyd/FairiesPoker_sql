using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto;
using Protocol.Constant;

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
                        foreach (var msg in messages)
                        {
                            Models.TriggerChatMessage(msg);
                        }
                    }
                    break;
                }
            default:
                break;
        }
    }
}
