using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    Animator animator;

    public List<GameNode> interactWithNodes;
    public List<GameNode> playerWonNodes;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        
    }


    public void Poke()
    {
        animator.SetTrigger("poke");
    }

    public void InteractWith()
    {
        //print("InteractWith");
        foreach (GameNode n in interactWithNodes)
        {
            GameManager.instance.AddEvent(n);
        }
    }

    public void PlayerWon()
    {
        foreach (GameNode n in playerWonNodes)
        {
            GameManager.instance.AddEvent(n);
        }
    }
}
