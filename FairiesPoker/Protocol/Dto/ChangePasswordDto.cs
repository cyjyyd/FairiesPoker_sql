using System;
using ProtoBuf;

namespace Protocol.Dto
{
    /// <summary>
    /// 修改密码请求DTO
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class ChangePasswordDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [ProtoMember(1)]
        public string Account;

        /// <summary>
        /// 旧密码（MD5加密）
        /// </summary>
        [ProtoMember(2)]
        public string OldPassword;

        /// <summary>
        /// 新密码（MD5加密）
        /// </summary>
        [ProtoMember(3)]
        public string NewPassword;

        public ChangePasswordDto()
        {
        }

        public ChangePasswordDto(string account, string oldPassword, string newPassword)
        {
            Account = account;
            OldPassword = oldPassword;
            NewPassword = newPassword;
        }
    }
}