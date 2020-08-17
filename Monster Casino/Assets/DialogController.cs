using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    public Text nameText, textText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject.Destroy(this.gameObject);
            GameManager.instance.NextEvent();
        }
    }

    public void SetName(string name)
    {
        this.nameText.text = name;
    }

    public void SetText(string text)
    {
        this.textText.text = text;
    }
}
