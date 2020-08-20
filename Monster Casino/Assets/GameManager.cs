using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    private Queue<GameNode> gameEvents = new Queue<GameNode>();

    public bool eventInProgress = false;
    public bool ready = true;

    public GameObject dialogWindow;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

    }

    void Update()
    {
        eventInProgress = gameEvents.Count > 0;
        if (ready && eventInProgress)
        {
            ProcessQueue();
        }

        
    }

    void ProcessQueue()
    {
        GameNode curr = gameEvents.Peek();
        if (curr.nodeType == GameEventType.Dialog)
        {
            GameObject windowInstance = GameObject.Instantiate(dialogWindow);
            windowInstance.SendMessage("SetName", curr.nameText);
            windowInstance.SendMessage("SetText", curr.textText);
        }

        if (curr.nodeType == GameEventType.PlayPoker)
        {
            PokerGameManager.instance.gameObject.SetActive(true);
            PokerGameManager.instance.player = PlayerAgent.instance.gameObject;
            PokerGameManager.instance.opponent = curr.pokerOpponent.gameObject;
            PokerGameManager.instance.StartGame(PlayerAgent.instance.gameObject, curr.pokerOpponent.gameObject);
        }

        if (curr.nodeType == GameEventType.EmergencyDog)
        {
            GameObject windowInstance = GameObject.Instantiate(dialogWindow);
            windowInstance.SendMessage("SetName", curr.nameText);
            windowInstance.SendMessage("SetText", curr.textText);
            PlayerAgent.instance.GetComponent<PokerPlayer>().money = 10000;
        }

        ready = false;
    }

    public void EndPoker(PokerPlayer winner)
    {
        gameEvents.Dequeue();
        ready = true;
    }

    public void NextEvent()
    {
        gameEvents.Dequeue();
        ready = true;
    }

    public void AddEvent(GameNode node)
    {
        gameEvents.Enqueue(node);
    }
}
