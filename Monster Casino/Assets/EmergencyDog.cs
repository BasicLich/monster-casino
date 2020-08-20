using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyDog : MonoBehaviour
{
    PokerPlayer pp;

    public List<GameNode> bankruptcyNodes;
    // Start is called before the first frame update
    void Start()
    {
        //pp = PlayerAgent.instance.GetComponent<PokerPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(pp == null)
        {
            pp = PlayerAgent.instance.GetComponent<PokerPlayer>();
        }
        else if(GameManager.instance.ready && !GameManager.instance.eventInProgress && pp.money <= 0)
        {
            foreach (GameNode n in bankruptcyNodes)
            {
                GameManager.instance.AddEvent(n);
            }
        }
    }
}
