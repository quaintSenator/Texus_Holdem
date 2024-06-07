using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Texus;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class QuitRoomBtmBeh : MonoBehaviour
{
    [SerializeField] private Image btnImage;
    [SerializeField] private AudioSource audioSource;

    private void OnEnable()
    {
        EventModule.Instance.AddNetEvent((int)SERVER_CMD.ServerQuitroomRsp, onQuitRoomRsp);
    }

    private void OnDisable()
    {
        EventModule.Instance.RemoveNetEvent((int)SERVER_CMD.ServerQuitroomRsp, onQuitRoomRsp);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        connectBtnBeh.IsConnecting = false;
        btnImage.color = Color.gray;
        audioSource.PlayOneShot(audioSource.clip);
        //回到大厅
        PlayerTryQuitRoom req = new PlayerTryQuitRoom();
        req.PlayerID = PlayerMgr.PlayerID;
        req.RoomID = PlayerMgr.NowInRoomID;
        Debug.Log("sending: " + req.ToString());
        Network.instance.SendMsg((int)CLIENT_CMD.ClientQuitRoomReq, req);
    }
    private void onQuitRoomRsp(int cmd, IMessage msg)
    {
        Debug.Log("onQuitRoomRsp");
        PlayerQuitRoomResult rsp = msg as PlayerQuitRoomResult;
        if (rsp == null)
        {
            Debug.Log("QuitRoomRsp null");
        }
        else
        {
            switch (rsp.QuitResult)
            {
                case (int)PROTO_RESULT_CODE.QuitroomResultOk:
                    Debug.Log("Quit Room OK.");
                    SceneManager.LoadScene("Scenes/LobbyScene");
                    break;
                default:
                    Debug.Log("Quit Room Req was rejected.");
                    break;
            }
        }
        
    }
    private void OnMouseUp()
    {
        btnImage.color = Color.white;
        
    }
}
