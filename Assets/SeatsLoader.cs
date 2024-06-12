using System;
using System.Collections;
using System.Collections.Generic;
using Texus;
using UnityEngine;

public class SeatsLoader : MonoBehaviour
{
    private RectTransform _rectTransform;
    private List<RectTransform> seatPositions;
    private void Awake()
    {
        //If game board start loading, Seats loader must complete seats load;
        //also when client gets new player enter room broadcast, must load as well.
        Debug.Log("Game Scene loading...");
    }
    // Start is called before the first frame update
    void Start()
    {
        _rectTransform = gameObject.GetComponent<RectTransform>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RefreshSeats()
    {
        int totalPlayers = PlayerMgr.seatTable.Count;
        
        //totalPlayers
    }
    void GetNPlayerTransforms(int n, List<Rect> positions)
    {
        //n = 7，上4下3，且下方中央位置总是玩家自己
        //需要区分，在positions当中的下标 并不是玩家的 座位号
        positions.Clear();
        
    }
}
