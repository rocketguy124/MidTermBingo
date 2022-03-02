using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using System.Linq;

public class GameMasterScript : NetworkComponent
{
    public bool gameStarted;
    public Text calledNumberText;
    public GameObject temp;

    public Text temptext;

    public static bool winnerFound = false; // Static to allow interaction between scripts
    public int randomNum;

    public override void HandleMessage(string flag, string value)
    {
        Debug.Log(flag + value);
        //Handles the Starting of the Game, initiated by the GameMaster once all Clients are connected and ready up (requires >2 players)
        if (flag == "GAMESTART")
        {
            gameStarted = true;
            //Debug.Log("In GameMaster - GAMESTART");
            calledNumberText.gameObject.SetActive(true);
            //For each object with BingoPlayer Script, set the ready menu to disappear while turning on the Bingo Board and revealing the player's name
            foreach (BingoPlayer bp in GameObject.FindObjectsOfType<BingoPlayer>())
            {
                bp.transform.GetChild(0).gameObject.SetActive(false);
                bp.bingoTitleText.SetActive(true);
                bp.bingoNameText.gameObject.SetActive(true);
                bp.bingoBoardPanel.SetActive(true);
            }
        }
        //Handling the displaying of the calling of numbers 
        if (flag == "CALL")
        {
            if (IsServer)
            {
                //Debug.Log("In the Server CALL flag");
                calledNumberText.text = "Number called: " + value;
            }
            if (IsClient)
            {
                //Debug.Log("In the Client CALL flag");
                calledNumberText.text = "Number called: " + value;
            }
        }
        //Handles the Win update, just sets the text to an obvious difference for the win
        if(flag == "WINF")
        {
            calledNumberText.text = "Winner has been found, Server shutting down in 20 Seconds, Thank you for Playing.";
        }
    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        //Setting the array so that the FindObjectsOfType isn't called repeatedly 
        BingoPlayer[] BingoPlayerarr = FindObjectsOfType<BingoPlayer>();

        //Menu Phase, will not pass till all clients ready
        while (!gameStarted && IsServer)
        {
            bool readyGo = true;
            BingoPlayerarr = FindObjectsOfType<BingoPlayer>();
            int count = 0;
            foreach (BingoPlayer bp in BingoPlayerarr)
            {
                if (!bp.isReady)
                {
                    readyGo = false;
                    break;
                }
                count++;
            }
            if (count < 2)
            {
                readyGo = false;
            }
            gameStarted = readyGo;
            //Debug.Log(gameStarted);
            //Debug.Log(GameObject.FindObjectsOfType<BingoPlayer>().Length);
            yield return new WaitForSeconds(2f);
        }

        //Making sure to update that the Game has started
        if (IsServer)
        {
            Debug.Log("Right before GAMESTART");
            SendUpdate("GAMESTART", gameStarted.ToString());
        }

        //This will be what handles the prevention of duplicate numbers
        //The Enumerable will make a List of 75 values and place a random value on all of them by index
        //The List is then organized by their weight/value
        //This means that technically, the random numbers are already decided and not random each time, but they will be random every instance ran
        //Since only 75 numbers are made and all aren't duplicates, there will be no duplicated numbers in the number calls (Gameboard is on the other script)
        int index = 0;
        var randomNumbers = Enumerable.Range(1, 75).OrderBy(x => Random.value).ToList();

        //Play Phase -> call numbers, check for winner, if winner then move out to next Phase
        while (IsServer && !winnerFound)
        {

            if (gameStarted)
            {
                //Setting a variable to the number from the Enumerable to be called
                randomNum = randomNumbers[index];
                index++;

                SendUpdate("CALL", randomNum.ToString());
                //For each player, run a function in BingoPlayer to check to see if the value called is in their board (This function will call a separate funciton to check for the winner)
                foreach (BingoPlayer bp in BingoPlayerarr)
                {
                    bp.FindValue(randomNum.ToString());
                }
                //Numbers are called every 1 second
                yield return new WaitForSeconds(1f);
                //calledNumberText.text = "Number called: " + randomNum.ToString();
            }
            if (IsDirty)
            {
                SendUpdate("GAMESTART", gameStarted.ToString());
                SendUpdate("CALL", randomNum.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
        //At this point, the Play Phase is over and the winner has been found, so update WINF and wait 20 seconds before disconnecting the Server
        if (IsServer)
        {
            SendUpdate("WINF", "wow");
            yield return new WaitForSeconds(20f);
            StartCoroutine(MyCore.DisconnectServer());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStarted = false;
        //temp = GameObject.Find("BingoBoardPanel");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
