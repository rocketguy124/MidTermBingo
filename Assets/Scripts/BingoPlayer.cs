using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class BingoPlayer : NetworkComponent
{
    public bool isNamed = false;
    public string name = "";
    public bool isReady = false;

    public InputField nameField;
    public Toggle readyToggle;
    public Text bingoNameText;
    public GameObject bingoTitleText;
    public GameObject bingoBoardPanel;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "NAME")
        {
            if (IsClient)
            {
                name = value;
                nameField.text = value;
                bingoNameText.text = value;
            }
            if (IsServer)
            {
                name = value;
                SendUpdate("NAME", value);
            }
        }
        if(flag == "READY")
        {
            isReady = bool.Parse(value);
            if (IsClient)
            {
                readyToggle.isOn = isReady;
            }
            if (IsServer)
            {
                SendUpdate("READY", value);
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        if (!IsLocalPlayer)
        {
            readyToggle.interactable = false;
            nameField.interactable = false;
        }
        while (IsConnected)
        {
            if (IsLocalPlayer)
            {
                if (!isNamed)
                {
                    readyToggle.interactable = false;
                }
                else
                {
                    readyToggle.interactable = true;
                }
            }
            if (IsServer)
            {
                if (IsDirty)
                {
                    SendUpdate("NAME", name);
                    SendUpdate("READY", isReady.ToString());
                    IsDirty = false;
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject temp = GameObject.Find("GameCanvas");
        if(temp == null)
        {
            Debug.Log("'GameCanvas' not found!");
        }
        else
        {
            this.transform.SetParent(temp.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnEndEdit(string s)
    {
        if(IsLocalPlayer && MyId.IsInit)
        {
            SendCommand("NAME", s);
            if(s.Length > 0)
            {
                isNamed = true;
            }
        }
    }
    public void SetReady(bool r)
    {
        if(IsLocalPlayer && MyId.IsInit  && isNamed)
        {
            SendCommand("READY", r.ToString());
            Debug.Log(r);
        }
    }
}
