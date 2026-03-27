using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Code
{
    /// <summary>
    /// 头像模块操作码
    /// </summary>
    public class AvatarCode
    {
        /// <summary>
        /// 上传头像请求
        /// </summary>
        public const int UPLOAD_CREQ = 0;

        /// <summary>
        /// 上传头像响应
        /// </summary>
        public const int UPLOAD_SRES = 1;

        /// <summary>
        /// 获取头像请求
        /// </summary>
        public const int GET_CREQ = 2;

        /// <summary>
        /// 获取头像响应
        /// </summary>
        public const int GET_SRES = 3;

        /// <summary>
        /// 下载头像数据请求（通过URL下载头像二进制数据）
        /// </summary>
        public const int DOWNLOAD_CREQ = 4;

        /// <summary>
        /// 下载头像数据响应
        /// </summary>
        public const int DOWNLOAD_SRES = 5;
    }
}