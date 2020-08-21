using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PartyAbility
{
    Peek = 0,
    Switcharoo = 1,
    Inebriate = 2,
    Intimidate = 3,
}
public class PartyMember : MonoBehaviour
{
    public string partyMemberName;
    public PartyAbility ability;
    public Texture abilitySprite;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int AbilityCost(bool discount)
    {
        int discountAmt = discount ? 5 : 0;
        switch(ability)
        {
            case PartyAbility.Peek:
                return 20 - discountAmt;
            case PartyAbility.Switcharoo:
                return 60 - discountAmt;
            case PartyAbility.Inebriate:
                return 10 - discountAmt;
            case PartyAbility.Intimidate:
                return 20 - discountAmt;
        }
        return 0;
    }

    public string AbilityTooltip(bool discount)
    {
        switch (ability)
        {
            case PartyAbility.Peek:
                return "Peek: Steals a glance at your opponent's cards. Cost: " + AbilityCost(discount) + "%";
            case PartyAbility.Switcharoo:
                return "Switcharoo: Swaps your cards with your opponent's. Hilarious! Cost: " + AbilityCost(discount) + "%";
            case PartyAbility.Inebriate:
                return "Inebriate: Gets your opponent drunk. Drunkards fall prey to overconfidence. Cost: " + AbilityCost(discount) + "%";
            case PartyAbility.Intimidate:
                return "Intimidate: Makes your opponent more likely to fold. Cost: " + AbilityCost(discount) + "%";
        }
        return "SoMeThInG wEnT wRoNgggg";
    }
}
