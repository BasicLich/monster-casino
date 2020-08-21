using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEventType
{
    Dialog,
    PlayPoker,
    SetVariable,
    EmergencyDog,
    KillOpponent,
}
public class GameNode : MonoBehaviour
{
    public string nameText;
    public string textText;
    public GameEventType nodeType;
    public NPCController pokerOpponent;

}
