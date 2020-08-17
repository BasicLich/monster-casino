using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    Animator animator;

    public List<GameNode> interactWithNodes;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
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
