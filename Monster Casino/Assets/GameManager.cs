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

    public GameObject explosion;

    public AudioSource music;

    public List<GameNode> gameStartNodes;

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

        foreach (GameNode n in gameStartNodes)
        {
            GameManager.instance.AddEvent(n);
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
        ready = false;

        if (curr.nodeType == GameEventType.Dialog)
        {
            GameObject windowInstance = GameObject.Instantiate(dialogWindow);
            windowInstance.SendMessage("SetName", curr.nameText);
            windowInstance.SendMessage("SetText", curr.textText);
        }

        if (curr.nodeType == GameEventType.PlayPoker)
        {
            music.Stop();
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

        if (curr.nodeType == GameEventType.KillOpponent)
        {
            StartCoroutine(KillGameObject(2f, curr.pokerOpponent.gameObject));
        }

        
    }

    IEnumerator KillGameObject(float waitTime, GameObject deadGuy)
    {
        GameObject expl = GameObject.Instantiate(explosion);
        expl.transform.position = deadGuy.transform.position;
        GameObject.Destroy(expl, 2f);
        GameObject.Destroy(deadGuy);
        yield return new WaitForSeconds(waitTime);
        gameEvents.Dequeue();
        ready = true;
    }

    public void EndPoker(PokerPlayer winner)
    {
        GameNode curr = gameEvents.Peek();
        if(winner.human)
            curr.pokerOpponent.PlayerWon();
        gameEvents.Dequeue();

        music.Play();

        ready = true;
    }

    public void NextEvent()
    {
        StartCoroutine(DequeueAndReady(0.1f));
    }

    IEnumerator DequeueAndReady(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        gameEvents.Dequeue();
        ready = true;
    }

    public void AddEvent(GameNode node)
    {
        gameEvents.Enqueue(node);
    }
}
