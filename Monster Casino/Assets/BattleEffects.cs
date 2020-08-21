using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEffects : MonoBehaviour
{
    public GameObject spotlight;
    public ParticleSystem chips;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartChips()
    {
        chips.Play();
    }

    void StopChips()
    {
        chips.Stop();
    }

    void StartLight()
    {
        spotlight.SetActive(true);
    }

    void StopLight()
    {
        spotlight.SetActive(false);
    }
}
