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
    JoinParty,
    ChoiceBranch,
    AlterMoney,
    EnoughMoneyBranch,
    IsPartyMemberBranch,
}
public class GameNode : MonoBehaviour
{
    public string nameText;
    public string textText;
    public GameEventType nodeType;
    public NPCController pokerOpponent;
    public List<GameNode> branchA, branchB;
    public PartyMember partyMember;
    public int cost;
    public string choiceA, choiceB;
}
