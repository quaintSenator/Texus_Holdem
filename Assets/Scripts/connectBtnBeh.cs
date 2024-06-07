using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Texus;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class connectBtnBeh : MonoBehaviour
{
    [SerializeField] private Image btnImage;
    [SerializeField] private Network network;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private InputField inputField4PlayerIDRef;
    [SerializeField] private InputField inputField4RoomIDRef;
    // Start is called before the first frame update
    public static bool IsConnecting;

    private void OnEnable()
    {
        EventModule.Instance.AddNetEvent((int)SERVER_CMD.ServerJudgeJoinRsp, OnJoinRoomRsp);
    }

    private void OnDisable()
    {
        EventModule.Instance.RemoveNetEvent((int)SERVER_CMD.ServerJudgeJoinRsp, OnJoinRoomRsp);
    }

    void Start()
    {
        if (btnImage == null)
        {
            Debug.Log("connectBtnBeh::btnImage not binded");
            btnImage = this.gameObject.GetComponent<Image>();
            if (!btnImage)
            {
                Debug.Log("ERROR: connectBtnBeh::btnImage not found");
            }
        }
        if (audioSource == null)
        {
            Debug.Log("connectBtnBeh::audioSource not binded");
            audioSource = this.gameObject.GetComponent<AudioSource>();
            if (!audioSource)
            {
                Debug.Log("ERROR: connectBtnBeh::audioSource not found");
            }
        }
        IsConnecting = false;
    }
    private void OnMouseDown()
    {
        btnImage.color = Color.gray;
        audioSource.PlayOneShot(audioSource.clip);
        if (!IsConnecting)
        {
            IsConnecting = true;
            Debug.Log("New Connection Launching");
            PlayerTryJoin myTry = new PlayerTryJoin();
            Debug.Log("PlayerID = " + inputField4PlayerIDRef.text);
            myTry.PlayerID = inputField4PlayerIDRef.text;
            myTry.RoomID = int.Parse(inputField4RoomIDRef.text);
            Debug.Log("sending: " + myTry.ToString());
            Network.instance.SendMsg((int)CLIENT_CMD.ClientJoinRoomReq, myTry);
        }
    }
    private void OnMouseUp()
    {
        btnImage.color = Color.white;
    }
    public void OnJoinRoomRsp(int cmd, IMessage msg)
    {
        Debug.Log("OnJoinRoomRsp");
        PlayerJoinResult rsp = msg as PlayerJoinResult;
        if (rsp == null)
        {
            Debug.Log("JoinRoomRsp null");
        }
        else
        {
            switch (rsp.JoinResult)
            {
                case (int)PROTO_RESULT_CODE.JoinroomResultOk:
                    Debug.Log("Join Room OK.");
                    //留存房间号、玩家ID和座位号
                    PlayerMgr.PlayerID = inputField4PlayerIDRef.text;
                    PlayerMgr.NowInRoomID = int.Parse(inputField4RoomIDRef.text);
                    bool hasFoundMySeat = false;
                    foreach (SeatTableItem it in rsp.SeatTable)
                    {
                        PlayerMgr.seatTable.Add(it);
                        if (it.PlayerId == PlayerMgr.PlayerID)
                        {
                            PlayerMgr.SeatNo = it.SeatNumber;
                            hasFoundMySeat = true;
                        }
                    }
                    if (!hasFoundMySeat)
                    {
                        Debug.Log("My seat not in table!");
                    }
                    EventModule.Instance.networkRef = network;
                    SceneManager.LoadScene("Scenes/GameBoardScene");
                    break;
                
                default:
                    Debug.Log("Join Room Req was rejected: " + rsp.JoinResult);
                    break;
            }
        }
    }
    
}
