using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAgent : MonoBehaviour
{
    // this is really bad. added singleton on non-manager because priorities lie elsewhere.
    public static PlayerAgent instance = null;

    public GameObject target;

    NavMeshAgent agent;
    Animator animator;

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

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetFloat("speed", agent.velocity.magnitude);
        if(target && Vector3.Distance(transform.position, target.transform.position) < 2f)
        {
            target.SendMessage("InteractWith");
            target = null;
            agent.SetDestination(transform.position);
        }
    }

    void Poke()
    {
        animator.SetTrigger("poke");
    }
}
