using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : MonoBehaviour
{
    public GameObject target;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z - 10f), Time.deltaTime);
    }
}
