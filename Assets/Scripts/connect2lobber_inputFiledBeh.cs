using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class connect2lobber_inputFiledBeh : MonoBehaviour
{
    // Start is called before the first frame update
    private InputField thisIField;
    void Start()
    {
        thisIField = this.gameObject.GetComponent<InputField>();
        if (thisIField == null)
        {
            Debug.Log("cannot get this IField");
        }
        else
        {
            thisIField.interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void submit_connect2lobber(string s)
    {
        
    }

    
}
