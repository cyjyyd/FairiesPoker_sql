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
            case ChatCode.SRES:
                {
                    ChatDto dto = value as ChatDto;
                    int userId = dto.UserId;
                    int chatType = dto.ChatType;
                    string text = Constant.GetChatText(chatType);                        
                    //显示文字
                    //播放声音
                    break;
                }
            default:
                break;
        }
    }
}
