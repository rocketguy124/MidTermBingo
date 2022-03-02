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
        //Handling the Setting of the Name from the EndEdit of the InputField
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
        //Handling the Ready-ing of players from the Toggle's callback function
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
            //Marking the Local Player as  Blue to further indicate who they are (besides the name they entered, if per chance identical names)
            if (IsLocalPlayer)
            {
                gameObject.GetComponent<Image>().color = new Color32(0, 0, 255, 128);
            }
        }
        
        //Handling the Random Numbers for the board to set to the clients once decided on by the server
        if(flag == "RANDNUM")
        {
            if (IsClient)
            {
                //Takes the value sent in and splits it by commas into an array to then reassign into the clients 2D array
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
        //Hanlding the marking the Text object as red if the number on the board was called by the GameMaster
        if(flag == "MARK")
        {
            //index 0 and 2, since the comma is at 1
            int i = int.Parse(value[0].ToString());
            int j = int.Parse(value[2].ToString());

            bingoNumbers[i, j].color = Color.red;

        }
        //Handling changing the winner's color to Yellow-ish Gold for an obvious win
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
        //Making the other players objects untouchable
        if (!IsLocalPlayer)
        {
            readyToggle.interactable = false;
            nameField.interactable = false;
        }
        //
        if (IsServer)
        {
            int randNum = 0;
            message = "";

            Debug.Log("In the SlowUpdate of GameMaster right before object creation");
            for (int i = 0; i < 5; i++)
            {
                //This will be what handles the prevention of duplicate numbers
                //The Enumerable will make a List of 1-15, 16-30, 31-45, 46-60, 61-75 (Each one at a time) values and place a random value on all of them by index
                //The List is then organized by their weight/value
                //This means that technically, the random numbers are already decided and not random each time, but they will be random every instance ran
                //Since only 1-15 (or the other ranges) are made, there will be no duplicated numbers
                //Only the first five numbers of this ordered List are taken and assigned to the board
                var randomNumbers = Enumerable.Range(i*15+1, 15).OrderBy(x => Random.value).Take(5).ToList();

                for (int j = 0; j < 5; j++)
                {
                    randNum = randomNumbers[j];
                    //setting the text to the random number from the List
                    bingoNumbers[i, j].text = randNum.ToString();
                    //creating a long string (combined with commas to be split) to then send to be handled from Server to send the correct numbers to the clients
                    message += randNum.ToString() + "," ;
                }
                
            }
            //This is how the Server will relay the correct numbers to the Clients
            SendUpdate("RANDNUM",message);
        }
        while (IsConnected)
        {
            //Checking for a name before allowing the player to ready up
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
        //My prefab is only a Panel, so I will find a Canvas premade in the scene to assign the players to (in the else)
        GameObject temp = GameObject.Find("GameCanvas");

        if (temp == null)
        {
            Debug.Log("'GameCanvas' not found!");
        }
        else
        {
            this.transform.SetParent(temp.transform);
        }
        
        //Standard nested loop to instantiate 25 Text objects on the object that has this script (Essentially the player)
        //It also assigns them onto a premade panel that will have a  grid layout to better organize the board
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

    //Callback function for the Name that will send it across Server to Client
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
    //Callback function for the Toggle
    public void SetReady(bool r)
    {
        if(IsLocalPlayer && MyId.IsInit  && isNamed)
        {
            SendCommand("READY", r.ToString());
            Debug.Log(r);
        }
    }
    //Function to Find a value if it matches the number that  was called by the Gamemaster
    public void FindValue(string s)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                //if the called number matches to what is in the board, it will send the MARK  flag to change the color said match as well as perform a check for a winner
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
    //Function to Check for a winner once a value has been matched to the number called (This will run every time a value is found)
    public void CheckWinCondition(int i, int j)
    {
        //All 4 ways to win (No free space and no 4 corners)
        // XXXXX
        bool horizontal = true;
        //X
        //X
        //X
        //X
        //X
        bool vertical = true;
        //X
        //  X
        //    X
        //      X
        //        X
        bool Diag1 = true;
        //        X
        //      X
        //    X
        //  X
        //X
        bool Diag2 = true;

        //for each row/column do the check
        //Instead of creating an array that held each coordinate and checking if  the coordinates were in the right  position,
        //I check if they are just red and consider that a marked number (as it is) and compare its  neighbors through the loop
        //This comparison is just having the "Innocent until proven guilty" or in this "A Winner until I don't see a red in the right spot"
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
        //If any of the variables are true, then that means that their check showed that all of the numbers were marked (red) 
        //Thus, there is a win condition and that player wins.
        if (horizontal || vertical || Diag1 || Diag2)
        {
            SendUpdate("WIN", "you da winnar");
            //Updates a Static variable to allow interaction between scripts 
            GameMasterScript.winnerFound = true;
        }
    }
}
