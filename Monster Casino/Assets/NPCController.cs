using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    Animator animator;

    public List<GameNode> interactWithNodes;

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
        print("You wnna go???");
        foreach(GameNode n in interactWithNodes)
        {
            GameManager.instance.AddEvent(n);
        }
    }
}
