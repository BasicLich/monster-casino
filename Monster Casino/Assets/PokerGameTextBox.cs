using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerGameTextBox : MonoBehaviour
{
    public int behavior = 0;
    public float lifespan = 3f;
    public Vector3 movementSpeed = Vector3.zero;
    public void LaunchText(string text, Vector3 startPoint, Vector3 movementSpeed, float lifespan)
    {
        //print("launched text: " + text);
        transform.localPosition = startPoint;
        this.GetComponentInChildren<Text>().text = text;
        this.movementSpeed = movementSpeed;
        this.lifespan = lifespan;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lifespan -= Time.deltaTime;
        transform.position += movementSpeed * Time.deltaTime;
        //print("lifespan " + lifespan);

        if(lifespan < 0)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
