using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class GameMasterScript : NetworkComponent
{
    public bool gameStarted;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "GAMESTART")
        {
            gameStarted = true;
            Debug.Log("In GameMaster - GAMESTART");
            foreach ( BingoPlayer bp in GameObject.FindObjectsOfType<BingoPlayer>())
            {
                bp.transform.GetChild(0).gameObject.SetActive(false);
                bp.bingoTitleText.SetActive(true);
                bp.bingoNameText.gameObject.SetActive(true);
                bp.bingoBoardPanel.SetActive(true);

                Debug.Log("Child 0 -   " + bp.transform.GetChild(0).gameObject.activeSelf);
                Debug.Log("Title Text -  " + bp.bingoTitleText.activeSelf);
                Debug.Log("Name Text -   " + bp.bingoNameText.gameObject.activeSelf);
                Debug.Log("BoardPanel -   " + bp.bingoBoardPanel.activeSelf);
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(!gameStarted && IsServer)
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
            if(count < 2)
            {
                readyGo = false;
            }
            gameStarted = readyGo;
            Debug.Log(gameStarted);
            Debug.Log(GameObject.FindObjectsOfType<BingoPlayer>().Length);
            yield return new WaitForSeconds(2f);
        }
        while (IsServer)
        {
            SendUpdate("GAMESTART", gameStarted.ToString());
            if (IsDirty)
            {
                SendUpdate("GAMESTART", gameStarted.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
