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

    public Text[,] bingoNumbers = new Text[5,5];
    public Text bingoNumberText;

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
            if (IsLocalPlayer)
            {
                gameObject.GetComponent<Image>().color = new Color32(0, 0, 255, 128);
            }
        }
        if(flag == "CREATETEXT" && IsServer)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    bingoNumbers[j, i] = MyCore.NetCreateObject(1, this.Owner, this.transform.position, Quaternion.identity).GetComponent<Text>();
                    //bingoNumbers[j, i] = bingoNumberText.GetComponent<Text>();
                    bingoNumbers[j, i].transform.SetParent(this.gameObject.transform.GetChild(2).transform);
                }
            }
                    
        }
    }

    public override void NetworkedStart()
    {
        SendCommand("CREATETEXT", Owner.ToString());

        int randomNum = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                switch (i)
                {
                    case 0:
                        randomNum = Random.Range(1, 16);
                        break;
                    case 1:
                        randomNum = Random.Range(16, 31);
                        break;
                    case 2:
                        randomNum = Random.Range(31, 46);
                        break;
                    case 3:
                        randomNum = Random.Range(46, 61);
                        break;
                    case 4:
                        randomNum = Random.Range(61, 76);
                        break;
                }
                bingoNumbers[j, i].text = randomNum.ToString();
            }
        }
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


        if (temp == null)
        {
            Debug.Log("'GameCanvas' not found!");
        }
        else
        {
            this.transform.SetParent(temp.transform);
        }


        /*
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (IsClient)
                {
                    SendCommand("CREATETEXT",Owner.ToString());
                }
                bingoNumbers[j, i] = MyCore.NetCreateObject(1, this.Owner, this.transform.position, Quaternion.identity).GetComponent<Text>();
                //bingoNumbers[j, i] = bingoNumberText.GetComponent<Text>();
                bingoNumbers[j, i].transform.SetParent(this.gameObject.transform.GetChild(2).transform);
            }
        }*/
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
