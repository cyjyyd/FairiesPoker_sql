using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Protocol.Dto
{
    /// <summary>
    /// 头像上传数据传输对象
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class AvatarDto
    {
        /// <summary>
        /// 用户ID（服务端使用）
        /// </summary>
        [ProtoMember(1)]
        public int UserId;

        /// <summary>
        /// 图片二进制数据
        /// </summary>
        [ProtoMember(2)]
        public byte[] ImageData;

        /// <summary>
        /// 原始文件名
        /// </summary>
        [ProtoMember(3)]
        public string FileName;

        /// <summary>
        /// 头像URL（下载请求时使用）
        /// </summary>
        [ProtoMember(4)]
        public string AvatarUrl;

        public AvatarDto()
        {
        }

        public AvatarDto(byte[] imageData, string fileName)
        {
            this.ImageData = imageData;
            this.FileName = fileName;
        }

        /// <summary>
        /// 创建下载请求
        /// </summary>
        public static AvatarDto CreateDownloadRequest(string avatarUrl)
        {
            return new AvatarDto { AvatarUrl = avatarUrl };
        }
    }
}