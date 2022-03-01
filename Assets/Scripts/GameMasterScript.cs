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

    public static bool winnerFound = false;
    public int randomNum;

    public override void HandleMessage(string flag, string value)
    {
        Debug.Log(flag + value);
        if (flag == "GAMESTART")
        {
            gameStarted = true;
            //Debug.Log("In GameMaster - GAMESTART");
            calledNumberText.gameObject.SetActive(true);
            foreach (BingoPlayer bp in GameObject.FindObjectsOfType<BingoPlayer>())
            {
                bp.transform.GetChild(0).gameObject.SetActive(false);
                bp.bingoTitleText.SetActive(true);
                bp.bingoNameText.gameObject.SetActive(true);
                bp.bingoBoardPanel.SetActive(true);
            }
        }
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
        if(flag == "WINF")
        {
            calledNumberText.text = "Winner has been found, Server shutting down in 20 Seconds, Thank you for Playing.";
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {

        }
    }

    public override IEnumerator SlowUpdate()
    {
        BingoPlayer[] BingoPlayerarr = FindObjectsOfType<BingoPlayer>();
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

        if (IsServer)
        {
            Debug.Log("Right before GAMESTART");
            SendUpdate("GAMESTART", gameStarted.ToString());
        }

        int index = 0;
        var randomNumbers = Enumerable.Range(1, 75).OrderBy(x => Random.value).ToList();

        while (IsServer && !winnerFound)
        {

            if (gameStarted)
            {

                randomNum = randomNumbers[index];
                index++;

                SendUpdate("CALL", randomNum.ToString());
                foreach (BingoPlayer bp in BingoPlayerarr)
                {
                    bp.FindValue(randomNum.ToString());
                }
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
