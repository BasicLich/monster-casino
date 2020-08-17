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

        ready = false;
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
