using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEventType
{
    Dialog,
    PlayPoker,
    EmergencyDog,
    KillOpponent,
    JoinParty,
    ChoiceBranch,
    AlterMoney,
    EnoughMoneyBranch,
    IsPartyMemberBranch,
    VarBranch,
    SetVar,
    PitBossDiscount,
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
