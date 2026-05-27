using Protocol.Dto;
using System.Collections;
using System.Collections.Generic;

namespace FairiesPoker.MG.Network
{

/// <summary>
/// 游戏数据的存储类
/// </summary>
public class GameModel
{
    //登录用户的数据
    public UserDto UserDto { get; set; }

    public int Id { get { return UserDto?.Id ?? 0; } }

    //匹配房间的数据
    public MatchRoomDto MatchRoomDto { get; set; }   
    
    public UserDto GetUserDto(int userId)
    {
        if (MatchRoomDto?.UIdUserDict != null &&
            MatchRoomDto.UIdUserDict.TryGetValue(userId, out var user))
        {
            return user;
        }

        return null;
    }

    public int GetRightUserId()
    {
        return MatchRoomDto?.RightId ?? -1;
    }

    public int GetLeftUserId()
    {
        return MatchRoomDto?.LeftId ?? -1;
    }
}
}
