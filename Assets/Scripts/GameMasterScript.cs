using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class GameMasterScript : NetworkComponent
{
    public bool gameStarted;
    public Text calledNumberText;
    public GameObject temp;

    public Text temptext;

    public int randomNum;

    public override void HandleMessage(string flag, string value)
    {
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
                /*
                Debug.Log("Child 0 -   " + bp.transform.GetChild(0).gameObject.activeSelf);
                Debug.Log("Title Text -  " + bp.bingoTitleText.activeSelf);
                Debug.Log("Name Text -   " + bp.bingoNameText.gameObject.activeSelf);
                Debug.Log("BoardPanel -   " + bp.bingoBoardPanel.activeSelf);
                */
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
        }/*
        if(flag == "SETBOARD")
        {
            Debug.Log("Here  in SETBOARD");
            foreach (BingoPlayer bp in GameObject.FindObjectsOfType<BingoPlayer>())
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        bp.bingoNumbers[j, i] = MyCore.NetCreateObject(1, this.Owner, this.transform.position, Quaternion.identity).GetComponent<Text>();

                        //bingoNumbers[j, i] = Instantiate(bingoNumberText).GetComponent<Text>();
                        bp.bingoNumbers[j, i].transform.SetParent(this.gameObject.transform.GetChild(2).transform);
                    }
                }
            }
        }*/
        if (flag == "RANDNUM")
        {
            //BingoPlayer.
            if (IsServer && value == "one")
            {
                int randNum = 0;
                Debug.Log("In the SlowUpdate of GameMaster right before object renumbering");
                foreach (BingoPlayer bp in GameObject.FindObjectsOfType<BingoPlayer>())
                {
                    Debug.Log("Creating bingo card");
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
                            temptext = bp.bingoNumbers[i, j].GetComponent<Text>();
                            //bp.bingoNumbers[i, j].text = randNum.ToString();
                            SendUpdate("RANDNUM", randNum.ToString());
                        }
                    }

                }
            }
            if (IsClient)
            {
                Debug.Log("in client randNum");
                temptext.text = value;
            }
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            // SendUpdate("SETBOARD", "wow");

        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (!gameStarted && IsServer)
        {
            bool readyGo = true;
            int count = 0;
            foreach (BingoPlayer bp in GameObject.FindObjectsOfType<BingoPlayer>())
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
        if (IsServer && gameStarted)
        {
            SendUpdate("RANDNUM", "one");
        }
    

            /*
            foreach (BingoPlayer bp in GameObject.FindObjectsOfType<BingoPlayer>())
            {
                Debug.Log("Creating bingo card");
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        bp.bingoNumbers[j, i] = MyCore.NetCreateObject(1, this.Owner, this.transform.position, Quaternion.identity).GetComponent<Text>();

                        //bingoNumbers[j, i] = Instantiate(bingoNumberText).GetComponent<Text>();
                        bp.bingoNumbers[j, i].transform.SetParent(bp.gameObject.transform.GetChild(2).transform);
                    }
                }
            }
            if (IsServer)
            {
                //SendUpdate("SETBOARD", "wow");
            }
            foreach (BingoPlayer bp in GameObject.FindObjectsOfType<BingoPlayer>())
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        //temp = bp.transform.GetChild(2).gameObject;
                        //MyCore.NetCreateObject(1, bp.Owner, bp.transform.position, Quaternion.identity).transform.SetParent(temp.transform);

                    }
                }
            }*/
        
        while (IsServer)
        {
            SendUpdate("GAMESTART", gameStarted.ToString());

            if (gameStarted)
            {
                randomNum = Random.Range(1, 76);
                SendUpdate("CALL", randomNum.ToString());
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




        /*
        if(gameStarted && IsServer)
        {
            int randomNum;
            randomNum = Random.Range(1, 100);
            SendUpdate("CALL", randomNum.ToString());
            //calledNumberText.text = "Number called: " + randomNum.ToString();
            yield return new WaitForSeconds(1f);
        }*/

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
