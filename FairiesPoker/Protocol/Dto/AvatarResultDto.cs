using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Protocol.Dto
{
    /// <summary>
    /// 头像上传结果
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class AvatarResultDto
    {
        /// <summary>
        /// 结果码：0成功，负数失败
        /// -1: 未登录
        /// -2: 图片格式不支持
        /// -3: 图片大小超限
        /// -4: 服务器错误
        /// </summary>
        [ProtoMember(1)]
        public int Result;

        /// <summary>
        /// 头像URL（成功时返回）
        /// </summary>
        [ProtoMember(2)]
        public string AvatarUrl;

        /// <summary>
        /// 提示消息
        /// </summary>
        [ProtoMember(3)]
        public string Message;

        public AvatarResultDto()
        {
        }

        public AvatarResultDto(int result, string avatarUrl = null, string message = null)
        {
            this.Result = result;
            this.AvatarUrl = avatarUrl;
            this.Message = message;
        }
    }
}