using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using Texus;
using UnityEngine;
using UnityEngine.Events;
using SERVER_CMD = Texus.SERVER_CMD;

public class Network : MonoBehaviour {
    //将协议号与返回的消息类型对应上
    private readonly Dictionary<int,Type> _responseMsgDic = new Dictionary<int, Type>() {
        {(int)SERVER_CMD.ServerCreateRsp,typeof(PlayerCreateRsp)},//创建回包 = 1005
         {(int)SERVER_CMD.ServerJudgeJoinRsp,typeof(PlayerJoinResult) },//请求加入房间回包 = 1007
         {(int)SERVER_CMD.ServerQuitroomRsp,typeof(PlayerQuitRoomResult) }//请求退出房间回包 = 1008
    };
    
    static private readonly byte[] PROTO_PREFIX = System.Text.Encoding.Default.GetBytes("TC");           // 协议前置头，用于快速检验
    static private readonly int PROTO_PREFIX_LEN = PROTO_PREFIX.Length;       // 前置头的长度
    static private readonly int PROTO_SIZE_LEN = 2;           // 协议体长度所占空间
    static private readonly int PROTO_CMD_LEN = 2;           // 协议号所占空间    
    static private readonly int PROTO_HEAD_LEN = (PROTO_PREFIX_LEN + PROTO_SIZE_LEN + PROTO_CMD_LEN);     // 协议头部所占空间

    
    //接收缓冲区
    private byte[] __revBuf = new byte[1024 * 8];
    private int __revBufPos = 0;
    
    public struct NetMsg {
        public int      cmd;
        public IMessage msg;
    }
    
    private const float         DEFINE_RECEIVE_INTERVAL = 0.1f;
    
    public        string        staInfo                 = "NULL";      //状态信息
    public        string        ip                      = "127.0.0.1"; //输入ip地址
    public string localipCopy = "127.0.0.1";
    public string awayioCopy = "10.11.140.171";
    public        int           port                    = 8086;        //输入端口号
    
    private int                 recTimes  = 0;      //接收到信息的次数
    private string              recMes    = "NULL"; //接收到的消息
    private TcpClient           tcpClient = null;
    private bool                clickSend = false; //是否点击发送按钮
    private Queue<NetMsg>       receiveQueue;      //服务器消息接收队列
    private float               timeCal = 0f;
    private byte[]              _headBytes;

    public UnityAction<NetMsg> recvCallback { get; set; } = null;

    public static Network instance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start() {
        
        if (_headBytes == null)
        {
            char[] head = new[] {'T', 'C'};
            _headBytes = Encoding.Default.GetBytes(head); 
        }

        instance = this;
        recvCallback = null;
        ConnectToServer();
    }

    private void OnDestroy() {
        Debug.Log("Network was destroyed.");
        if (tcpClient.Connected) {
            tcpClient.Close();
        }
    }

    private void Update() {
        timeCal += Time.deltaTime;
        if (timeCal >= DEFINE_RECEIVE_INTERVAL) {
            timeCal -= DEFINE_RECEIVE_INTERVAL;
            RecvMsg();
        }
        
        while (receiveQueue != null && receiveQueue.Count >0) {
            NetMsg msg = receiveQueue.Dequeue();
            EventModule.Instance.Dispatch(msg.cmd, msg.msg);
        }
    }

    private void ConnectToServer() {
        timeCal = 0f;

        try {
            if (tcpClient == null) tcpClient = new TcpClient();
            IPAddress  ipaddress             = IPAddress.Parse(ip);
            IPEndPoint point                 = new IPEndPoint(ipaddress, port);
            tcpClient.Connect(point); //通过IP和端口号来定位一个所要连接的服务器端
            Debug.Log("连接成功 , " + " ip = " + ip + " port = " + port);
            staInfo = ip + ":" + port + "  连接成功";

            receiveQueue = new Queue<NetMsg>();
        }
        catch (Exception) {
            Debug.Log("IP或者端口号错误......");
            staInfo = "IP或者端口号错误......";
        }
    }

    public void SendMsg(int cmd, IMessage msg) {
        if (tcpClient.Connected == false) {
            Debug.LogError("Send Message Failed! Tcp not connected!");
            return;
        }

        byte[] body = msg.ToByteArray();

        Int16  length     = (Int16)(body.Length + 2);
        byte[] lengthByte = BitConverter.GetBytes(length);
        
        byte[] cmdByte = BitConverter.GetBytes((Int16)cmd);
        
        int    packageLength = 4 + length;
        byte[] package       = new byte[packageLength];
        Buffer.BlockCopy(_headBytes, 0, package, 0, _headBytes.Length);
        Buffer.BlockCopy(lengthByte, 0, package, 2, lengthByte.Length);
        Buffer.BlockCopy(cmdByte,    0, package, 4, cmdByte.Length);
        Buffer.BlockCopy(body,       0, package, 6, body.Length);
        var stream = tcpClient.GetStream();
        stream.Write(package);
        stream.Flush();
        //stream.Close();
    }

    private int Check_pack(byte[] data,int len)
    {
        int msg_len = 0;
        if (len < PROTO_PREFIX_LEN + PROTO_SIZE_LEN)
        {  // 基本长度不够，表示包不完整，继续等待
            return -1;
        }

        if (!ByteIsSame(data, PROTO_PREFIX, PROTO_PREFIX_LEN))
        {    // 快速检验不通过，返回异常
            return -1;
        }

        msg_len = bytesToInt16(data, PROTO_PREFIX_LEN);// 获取消息体的长度
        if (len >= msg_len + PROTO_PREFIX_LEN + PROTO_SIZE_LEN)
        {   // 数据包长度够了
            return (int)(msg_len + PROTO_PREFIX_LEN + PROTO_SIZE_LEN);
        }
        else
        {
            return 0;           // 长度不够，继续等待
        }
    }
    
    private void RecvMsg() {
        if (tcpClient.Connected == false) {
            Debug.LogError("Receive Message Failed! Tcp not connected!");
            return;
        }

        int    readSoFar = 0;
        var    stream    = tcpClient.GetStream();
        byte[] buffer    = new byte[1024];
        
        while (stream.DataAvailable) {
            int readCount = stream.Read(buffer, 0, buffer.Length);
            //拷贝进成员缓冲区，和上次遗留的数据组成包
            Buffer.BlockCopy(buffer, 0, __revBuf, __revBufPos, readCount);
            __revBufPos += readCount;
            while(true)
            {
                int packPos = Check_pack(__revBuf,__revBufPos);
                //没有完整的包
                if (packPos <= 0) break;
                //数据包完整了
                //处理一个完整的包
                EnqueueMsg(__revBuf);
                //EnqueueMsg(buffer);
                /*Buffer.BlockCopy(__revBuf, __revBufPos, __revBuf, 0, __revBufPos - packPos);*/
                Buffer.BlockCopy(__revBuf, packPos, __revBuf, 0, __revBufPos - packPos);
                __revBufPos -= packPos;
                Debug.Log(__revBufPos);
            }
        }
    }

    private bool ByteIsSame(byte[] b2, byte[] b1, int len)
    {
        for(int i=0;i<len;i++)
        {
            if (b1[i] != b2[i]) return false;
        }
        return true;
    }
    
    private void EnqueueMsg(byte[] buffer) {
        byte[] headBytes   = new byte[2];
        byte[] lengthBytes = new byte[2];
        byte[] cmdBytes    = new byte[2];
        
        Buffer.BlockCopy(buffer, 0, headBytes, 0, 2);
        if (headBytes[0] == this._headBytes[0] && headBytes[1] == this._headBytes[1])
        {
            Buffer.BlockCopy(buffer, 2, lengthBytes, 0, 2);
            int length = bytesToInt16(lengthBytes, 0);
            Buffer.BlockCopy(buffer, 4, cmdBytes, 0, 2);
            int cmd = bytesToInt16(cmdBytes, 0);
                
            byte[] body = new byte[length -2];
            Buffer.BlockCopy(buffer, 6, body, 0, body.Length);

            Type tp;
            if (_responseMsgDic.TryGetValue(cmd, out tp))
            {
                IMessage msg = (IMessage)Activator.CreateInstance(tp);
                msg.MergeFrom(body);
                NetMsg netMsg;
                netMsg.cmd = cmd;
                netMsg.msg = msg;
                Debug.Log("Receiving: " + msg.ToString());
                receiveQueue.Enqueue(netMsg);
            }
            else
            {
                Debug.Log("Cannot recognizing: " + cmd.ToString());
            }
        }
    }

    public static Int16 bytesToInt16(byte[] src, int offset) {  
        Int16 value;    
        value = (Int16) ((src[offset] & 0xFF)   
                         | ((src[offset +1] & 0xFF) <<8));  
        return value;  
    }
    public void printBytes(byte[] bytearray)
    {
        string s = "";
        foreach (byte b in bytearray)
        {
            s += " ";
            s += b.ToString("X");
        }
        Debug.Log(s);
    }
}
