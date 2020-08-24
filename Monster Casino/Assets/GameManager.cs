using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    private Queue<GameNode> gameEvents = new Queue<GameNode>();

    public bool eventInProgress = false;
    public bool ready = true;

    public GameObject dialogWindow, choiceWindow;

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
            PokerGameManager.instance.tutorial = curr.tutorial;
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

        if(curr.nodeType == GameEventType.ChoiceBranch)
        {
            GameObject windowInstance = GameObject.Instantiate(choiceWindow);
            windowInstance.SendMessage("SetName", curr.nameText);
            windowInstance.SendMessage("SetText", curr.textText);
            windowInstance.SendMessage("SetChoiceA", curr.choiceA);
            windowInstance.SendMessage("SetChoiceB", curr.choiceB);
            windowInstance.SendMessage("SetBranchA", curr.branchA);
            windowInstance.SendMessage("SetBranchB", curr.branchB);
            //windowInstance.branchA = curr.branchA;

            //PlayerAgent.instance.GetComponent<PokerPlayer>().money = 10000;
        }

        if (curr.nodeType == GameEventType.AlterMoney)
        {
            PlayerAgent.instance.GetComponent<PokerPlayer>().money += curr.cost;
            //cha-ching sfx?
            NextEvent();
        }

        if (curr.nodeType == GameEventType.EnoughMoneyBranch)
        {
            if(PlayerAgent.instance.GetComponent<PokerPlayer>().money >= curr.cost)
            {
                foreach (GameNode n in curr.branchA)
                {
                    GameManager.instance.AddEvent(n);
                }
            } else
            {
                foreach (GameNode n in curr.branchB)
                {
                    GameManager.instance.AddEvent(n);
                }
            }
            NextEvent();
        }

        if (curr.nodeType == GameEventType.JoinParty)
        {
            PlayerAgent.instance.partyMembers.Add(curr.partyMember);
            NextEvent();
        }

        if (curr.nodeType == GameEventType.IsPartyMemberBranch)
        {
            bool isPartyMember = false;
            foreach(PartyMember partyMember in PlayerAgent.instance.partyMembers)
            {
                if (partyMember.gameObject == curr.partyMember.gameObject)
                    isPartyMember = true;
            }

            if(isPartyMember)
            {
                foreach (GameNode n in curr.branchA)
                {
                    GameManager.instance.AddEvent(n);
                }
            }
            else
            {
                foreach (GameNode n in curr.branchB)
                {
                    GameManager.instance.AddEvent(n);
                }
            }
            NextEvent();
        }

        if (curr.nodeType == GameEventType.VarBranch)
        {
            if(curr.pokerOpponent.var1)
            {
                foreach (GameNode n in curr.branchA)
                {
                    GameManager.instance.AddEvent(n);
                }
            }
            else
            {
                foreach (GameNode n in curr.branchB)
                {
                    GameManager.instance.AddEvent(n);
                }
            }
            NextEvent();
        }

        if (curr.nodeType == GameEventType.SetVar)
        {
            curr.pokerOpponent.var1 = true;
            NextEvent();
        }

        if (curr.nodeType == GameEventType.PitBossDiscount)
        {
            PokerGameManager.instance.discountBought = true;
            NextEvent();
        }

        if (curr.nodeType == GameEventType.ChangeSceneNode)
        {
            SceneManager.LoadScene(curr.sceneName);
            NextEvent();
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
