using ProtoBuf;
using System.IO;

namespace FPServer.Network
{
    /// <summary>
    /// 编码工具类 - 与客户端保持兼容
    /// </summary>
    public static class EncodeTool
    {
        #region 粘包拆包

        /// <summary>
        /// 构造消息包
        /// </summary>
        public static byte[] EncodePacket(byte[] data)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write(data.Length);
            bw.Write(data);
            byte[] byteArray = new byte[(int)ms.Length];
            Buffer.BlockCopy(ms.GetBuffer(), 0, byteArray, 0, (int)ms.Length);
            return byteArray;
        }

        /// <summary>
        /// 解析消息包
        /// </summary>
        public static byte[] DecodePacket(ref List<byte> dataCache)
        {
            if (dataCache.Count < 4)
                return null;

            using var ms = new MemoryStream(dataCache.ToArray());
            using var br = new BinaryReader(ms);
            int length = br.ReadInt32();
            int dataRemainLength = (int)(ms.Length - ms.Position);

            if (length > dataRemainLength)
                return null;

            byte[] data = br.ReadBytes(length);
            dataCache.Clear();
            dataCache.AddRange(br.ReadBytes(dataRemainLength));
            return data;
        }

        #endregion

        #region 消息序列化

        /// <summary>
        /// 序列化消息
        /// </summary>
        public static byte[] EncodeMsg(SocketMsg msg)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write(msg.OpCode);
            bw.Write(msg.SubCode);

            if (msg.Value != null)
            {
                string typeName = msg.Value.GetType().AssemblyQualifiedName;
                bw.Write(typeName);
                byte[] valueBytes = EncodeObj(msg.Value);
                bw.Write(valueBytes.Length);
                bw.Write(valueBytes);
            }

            byte[] data = new byte[ms.Length];
            Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int)ms.Length);
            return data;
        }

        /// <summary>
        /// 反序列化消息
        /// </summary>
        public static SocketMsg DecodeMsg(byte[] data)
        {
            var msg = new SocketMsg();
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            msg.OpCode = br.ReadInt32();
            msg.SubCode = br.ReadInt32();

            if (ms.Length > ms.Position)
            {
                string typeName = br.ReadString();
                msg.ValueType = typeName;
                int valueLength = br.ReadInt32();
                byte[] valueBytes = br.ReadBytes(valueLength);
                msg.Value = DecodeObj(typeName, valueBytes);
            }

            return msg;
        }

        #endregion

        #region 对象序列化

        public static byte[] EncodeObj(object value)
        {
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, value);
            return ms.ToArray();
        }

        public static object DecodeObj(string typeName, byte[] valueBytes)
        {
            using var ms = new MemoryStream(valueBytes);
            Type type = Type.GetType(typeName);
            if (type == null)
                throw new Exception($"无法加载类型: {typeName}");
            return Serializer.Deserialize(type, ms);
        }

        #endregion
    }

    /// <summary>
    /// 网络消息
    /// </summary>
    public class SocketMsg
    {
        public int OpCode { get; set; }
        public int SubCode { get; set; }
        public string ValueType { get; set; }
        public object Value { get; set; }

        public SocketMsg() { }

        public SocketMsg(int opCode, int subCode, object value)
        {
            OpCode = opCode;
            SubCode = subCode;
            Value = value;
        }
    }
}