using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public class EventModule
{
    public delegate void OnServerRsp(int cmd,IMessage msg);

    public Network networkRef;

    private EventModule()
    {
        netEventPool = new Dictionary<int, List<OnServerRsp>>();
        myPlayerID = "";
    }
    
    private static EventModule _Instance;
    public string myPlayerID;
    public static EventModule Instance
    {
        get
        {
            if(_Instance == null)
                _Instance = new EventModule();
            return _Instance;
        }
    }


    private Dictionary<int, List<OnServerRsp>> netEventPool;
        
    public void AddNetEvent(int cmd, OnServerRsp rsp)
    {
        List<OnServerRsp> rspList;

        if (!netEventPool.TryGetValue(cmd, out rspList))
        {
            rspList = new List<OnServerRsp>();
            netEventPool.Add(cmd,rspList);
        }
        rspList.Add(rsp);
    }

    public void RemoveNetEvent(int cmd, OnServerRsp rsp)
    {
        List<OnServerRsp> rspList;

        if (netEventPool.TryGetValue(cmd, out rspList))
        {
            for (int i = 0; i < rspList.Count; i++)
            {
                if (rspList[i].Equals(rsp))
                {
                    rspList.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void Dispatch(int cmd,IMessage msg)
    {
        List<OnServerRsp> rspList;

        if (netEventPool.TryGetValue(cmd, out rspList))
        {
            for(int i = 0; i < rspList.Count; i++) {
                rspList[i].Invoke(cmd, msg);
            }
        }
    }
}
