using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Protocol.Dto
{
    /// <summary>
    /// 房间信息DTO
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class RoomDto
    {
        /// <summary>
        /// 房间ID
        /// </summary>
        [ProtoMember(1)]
        public string RoomId;

        /// <summary>
        /// 房间名称
        /// </summary>
        [ProtoMember(2)]
        public string RoomName;

        /// <summary>
        /// 房主ID
        /// </summary>
        [ProtoMember(3)]
        public int HostId;

        /// <summary>
        /// 当前玩家数量
        /// </summary>
        [ProtoMember(4)]
        public int PlayerCount;

        /// <summary>
        /// 最大玩家数量
        /// </summary>
        [ProtoMember(5)]
        public int MaxPlayers;

        /// <summary>
        /// 房间状态: 0=等待中, 1=游戏中
        /// </summary>
        [ProtoMember(6)]
        public int Status;

        /// <summary>
        /// 房间内玩家列表
        /// </summary>
        [ProtoMember(7)]
        public List<UserDto> Players;

        public RoomDto()
        {
            Players = new List<UserDto>();
        }
    }

    /// <summary>
    /// 房间列表DTO
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class RoomListDto
    {
        [ProtoMember(1)]
        public List<RoomDto> Rooms;

        public RoomListDto()
        {
            Rooms = new List<RoomDto>();
        }
    }

    /// <summary>
    /// 房间状态常量
    /// </summary>
    public static class RoomStatus
    {
        public const int WAITING = 0;
        public const int PLAYING = 1;
    }
}