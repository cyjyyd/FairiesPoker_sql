using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace FairiesPoker.MG.Network
{

/// <summary>
/// 网络消息
/// 作用：发送的时候 都要发送这个类
/// </summary>
[ProtoContract]
public class SocketMsg
{
    /// <summary>
    /// 操作码
    /// </summary>
    [ProtoMember(1)]
    public int OpCode { get; set; }

    /// <summary>
    /// 子操作
    /// </summary>
    [ProtoMember(2)]
    public int SubCode { get; set; }

    /// <summary>
    /// 参数类型程序集限定名称（用于反序列化）
    /// </summary>
    [ProtoMember(3)]
    public string ValueType { get; set; }

    /// <summary>
    /// 参数（序列化后的字节数组）
    /// </summary>
    [ProtoMember(4)]
    public byte[] ValueBytes { get; set; }

    /// <summary>
    /// 参数（运行时使用）
    /// </summary>
    [ProtoIgnore]
    public object Value { get; set; }

    public SocketMsg()
    {

    }

    public SocketMsg(int opCode, int subCode, object value)
    {
        this.OpCode = opCode;
        this.SubCode = subCode;
        this.Value = value;
    }

    public void Change(int opCode, int subCode, object value)
    {
        this.OpCode = opCode;
        this.SubCode = subCode;
        this.Value = value;
    }
}}
