using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using System.Linq;

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
    public int countX , countY = 0;
    public string message = "";

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
        
        if(flag == "RANDNUM")
        {
            if (IsClient)
            {
                string[] strArr = value.Split(',');
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        bingoNumbers[i, j].text = strArr[i*5+j];
                    }
                }
            }
        }
        if(flag == "MARK")
        {
            int i = int.Parse(value[0].ToString());
            int j = int.Parse(value[2].ToString());

            bingoNumbers[i, j].color = Color.red;

        }
        if(flag == "WIN")
        {
            gameObject.GetComponent<Image>().color = new Color32(224, 108, 22, 128);
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
        if (IsServer)
        {
            int randNum = 0;
            message = "";


            Debug.Log("In the SlowUpdate of GameMaster right before object creation");
            for (int i = 0; i < 5; i++)
            {
                var randomNumbers = Enumerable.Range(i*15+1, 15).OrderBy(x => Random.value).Take(5).ToList();

                for (int j = 0; j < 5; j++)
                {
                    randNum = randomNumbers[j];
                    bingoNumbers[i, j].text = randNum.ToString();
                    message += randNum.ToString() + "," ;
                }
                
            }
            SendUpdate("RANDNUM",message);
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
                    SendUpdate("RANDNUM", message);
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
        
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                bingoNumbers[j, i] = Instantiate(bingoNumberText).GetComponent<Text>();
                bingoNumbers[j, i].transform.SetParent(this.gameObject.transform.GetChild(2).transform);
                Debug.Log(bingoNumbers[j, i].ToString());
            }
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
    public void FindValue(string s)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (bingoNumbers[i, j].text == s)
                {
                    SendUpdate("MARK", i + " " + j);
                    bingoNumbers[i, j].color = Color.red;
                    CheckWinCondition(i, j);
                    return;
                }
            }
        }
    }
    public void CheckWinCondition(int i, int j)
    {
        bool horizontal = true;
        bool vertical = true;
        bool Diag1 = true;
        bool Diag2 = true;
        for (int y = 0; y < 5; y++)
        {
            if (bingoNumbers[i, y].color != Color.red)
            {
                horizontal = false;
            }
            if (bingoNumbers[y, j].color != Color.red)
            {
                vertical = false;
            }
            if (bingoNumbers[y, y].color != Color.red)
            {
                Diag1 = false;
            }
            if (bingoNumbers[4-y, y].color != Color.red)
            {
                Diag2 = false;
            }
        }
        if (horizontal || vertical || Diag1 || Diag2)
        {
            SendUpdate("WIN", "you da winnar");
            GameMasterScript.winnerFound = true;
        }
    }
}
