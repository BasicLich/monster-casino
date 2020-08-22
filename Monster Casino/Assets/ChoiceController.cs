using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceController : MonoBehaviour
{
    public Text nameText, textText;

    public Text choiceA, choiceB;

    public List<GameNode> branchA, branchB;
    void Start()
    {

    }

    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
            //GameObject.Destroy(this.gameObject);
            //GameManager.instance.NextEvent();
        //}
    }

    public void SetName(string name)
    {
        this.nameText.text = name;
    }

    public void SetText(string text)
    {
        this.textText.text = text;
    }

    public void SetChoiceA(string text)
    {
        choiceA.text = text;
    }

    public void SetChoiceB(string text)
    {
        choiceB.text = text;
    }

    public void SetBranchA(List<GameNode> branch)
    {
        branchA = branch;
    }

    public void SetBranchB(List<GameNode> branch)
    {
        branchB = branch;
    }

    public void ChoiceA()
    {
        foreach (GameNode n in branchA)
        {
            GameManager.instance.AddEvent(n);
        }
        GameObject.Destroy(this.gameObject);
        GameManager.instance.NextEvent();
    }

    public void ChoiceB()
    {
        foreach (GameNode n in branchB)
        {
            GameManager.instance.AddEvent(n);
        }
        GameObject.Destroy(this.gameObject);
        GameManager.instance.NextEvent();
    }
    
}
