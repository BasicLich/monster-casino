using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAgent : MonoBehaviour
{
    public GameObject target;

    NavMeshAgent agent;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
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
}
