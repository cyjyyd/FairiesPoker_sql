using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections.Generic;

public class MatchHandler : HandlerBase
{
    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case MatchCode.ENTER_SRES:
                enterResponse(value as MatchRoomDto);
                break;
            case MatchCode.ENTER_BRO:
                enterBro(value as MatchRoomDto);
                break;
            case MatchCode.LEAVE_BRO:
                leaveBro(value as MatchRoomDto);
                break;
            case MatchCode.READY_BRO:
                readyBro(value as MatchRoomDto);
                break;
            case MatchCode.START_BRO:
                startBro();
                break;
            case RoomCode.GET_ROOMS_SRES:
                getRoomsResponse(value as List<RoomDto>);
                break;
            case RoomCode.CREATE_SRES:
                createRoomResponse(value as MatchRoomDto);
                break;
            case RoomCode.JOIN_SRES:
                joinRoomResponse(value as MatchRoomDto);
                break;
            case RoomCode.UPDATE_BRO:
                roomUpdateBro(value as MatchRoomDto);
                break;
            case RoomCode.LEAVE_BRO:
                leaveRoomBro(value as MatchRoomDto);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 开始游戏的广播处理
    /// </summary>
    private void startBro()
    {
        //开始游戏 隐藏状态面板的准备文字
        Models.TriggerMatchUpdate(Models.GameModel.MatchRoomDto);
    }

    /// <summary>
    /// 准备的广播处理
    /// </summary>
    private void readyBro(MatchRoomDto matchRoom)
    {
        if (matchRoom != null)
        {
            Models.GameModel.MatchRoomDto = matchRoom;
            matchRoom.ResetPosition(Models.GameModel.UserDto.Id);
            Models.TriggerMatchUpdate(matchRoom);
        }
    }

    /// <summary>
    /// 离开的广播处理（匹配模式）
    /// </summary>
    private void leaveBro(MatchRoomDto matchRoom)
    {
        if (matchRoom != null)
        {
            Models.GameModel.MatchRoomDto = matchRoom;
            matchRoom.ResetPosition(Models.GameModel.UserDto.Id);
            Models.TriggerMatchUpdate(matchRoom);
        }
    }

    /// <summary>
    /// 自身进入的服务器响应
    /// </summary>
    private void enterResponse(MatchRoomDto matchRoom)
    {
        Models.GameModel.MatchRoomDto = matchRoom;
        if (matchRoom != null)
        {
            matchRoom.ResetPosition(Models.GameModel.UserDto.Id);
        }
        // 触发更新事件
        Models.TriggerMatchUpdate(matchRoom);
    }

    /// <summary>
    /// 他人进入的广播处理
    /// </summary>
    private void enterBro(MatchRoomDto matchRoom)
    {
        if (matchRoom != null)
        {
            Models.GameModel.MatchRoomDto = matchRoom;
            matchRoom.ResetPosition(Models.GameModel.UserDto.Id);
            Models.TriggerMatchUpdate(matchRoom);
        }
    }

    /// <summary>
    /// 重置位置
    ///  更新左右玩家显示
    /// </summary>
    private void resetPosition()
    {
        GameModel gModel = Models.GameModel;
        MatchRoomDto matchRoom = gModel.MatchRoomDto;

        //重置一下玩家的位置
        if (matchRoom != null)
        {
            matchRoom.ResetPosition(gModel.UserDto.Id);
        }
    }

    #region 自定义房间处理

    /// <summary>
    /// 获取房间列表响应
    /// </summary>
    private void getRoomsResponse(List<RoomDto> rooms)
    {
        Models.TriggerRoomListUpdate(rooms ?? new List<RoomDto>());
    }

    /// <summary>
    /// 创建房间响应
    /// </summary>
    private void createRoomResponse(MatchRoomDto matchRoom)
    {
        if (matchRoom != null)
        {
            Models.GameModel.MatchRoomDto = matchRoom;
            matchRoom.ResetPosition(Models.GameModel.UserDto.Id);
            Models.TriggerMatchUpdate(matchRoom);
        }
    }

    /// <summary>
    /// 加入房间响应
    /// </summary>
    private void joinRoomResponse(MatchRoomDto matchRoom)
    {
        if (matchRoom != null)
        {
            Models.GameModel.MatchRoomDto = matchRoom;
            matchRoom.ResetPosition(Models.GameModel.UserDto.Id);
            Models.TriggerMatchUpdate(matchRoom);
        }
    }

    /// <summary>
    /// 房间更新广播（玩家加入/离开）
    /// </summary>
    private void roomUpdateBro(MatchRoomDto matchRoom)
    {
        if (matchRoom != null)
        {
            Models.GameModel.MatchRoomDto = matchRoom;
            matchRoom.ResetPosition(Models.GameModel.UserDto.Id);
            Models.TriggerMatchUpdate(matchRoom);
        }
    }

    /// <summary>
    /// 离开房间广播
    /// </summary>
    private void leaveRoomBro(MatchRoomDto matchRoom)
    {
        if (matchRoom != null)
        {
            Models.GameModel.MatchRoomDto = matchRoom;
            matchRoom.ResetPosition(Models.GameModel.UserDto.Id);
            Models.TriggerMatchUpdate(matchRoom);
        }
        else
        {
            // 房间为空，清空房间数据
            Models.GameModel.MatchRoomDto = null;
            Models.TriggerMatchUpdate(null);
        }
    }

    #endregion
}
