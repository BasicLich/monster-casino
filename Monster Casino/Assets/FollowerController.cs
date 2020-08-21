using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowerController : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;

    private int partyIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(partyIndex == -1)
        {
            for(int i = 0; i < PlayerAgent.instance.partyMembers.Count; i++)
            {
                if(PlayerAgent.instance.partyMembers[i].gameObject == this.gameObject)
                {
                    partyIndex = i;
                }
            }
        }
        else if (partyIndex == 0)
            agent.SetDestination(PlayerAgent.instance.transform.position);
        else
            agent.SetDestination(PlayerAgent.instance.partyMembers[partyIndex - 1].transform.position);

        animator.SetFloat("speed", agent.velocity.magnitude);
    }
}
