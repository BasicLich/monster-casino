using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StakeController : MonoBehaviour
{
    public GameObject playerStakes, opponentStakes, pot;

    private int playerStakeAmtAsync = 0;
    private int opponentStakeAmtAsync = 0;
    private int potAsync = 0;

    private List<GameObject> playerStakesChips;
    private List<GameObject> opponentStakesChips;
    private List<GameObject> potChips;

    public int chipValue = 25;

    // each stack is 32
    // 16 stacks (32 for pot)
    // 512 images (1024 for pot)

    // Start is called before the first frame update
    void Start()
    {
        playerStakesChips = new List<GameObject>();
        opponentStakesChips = new List<GameObject>();
        potChips = new List<GameObject>();

        foreach(Transform stack in playerStakes.transform)
        {
            foreach(Transform chip in stack)
            {
                playerStakesChips.Add(chip.gameObject);
            }
        }

        foreach (Transform stack in opponentStakes.transform)
        {
            foreach (Transform chip in stack)
            {
                opponentStakesChips.Add(chip.gameObject);
            }
        }

        foreach (Transform stack in pot.transform)
        {
            foreach (Transform chip in stack)
            {
                potChips.Add(chip.gameObject);
            }
        }

        foreach(GameObject chip in playerStakesChips)
        {
            chip.SetActive(false);
        }

        foreach (GameObject chip in opponentStakesChips)
        {
            chip.SetActive(false);
        }

        foreach (GameObject chip in potChips)
        {
            chip.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerStakeAmtAsync != Mathf.Max(PokerGameManager.instance.playerStakeSliderAmt, PokerGameManager.instance.playerStakeAmt))
        {
            foreach (GameObject chip in playerStakesChips)
            {
                chip.SetActive(false);
            }

            for (int i = 0; i < Mathf.Min(512, Mathf.Max(PokerGameManager.instance.playerStakeSliderAmt, PokerGameManager.instance.playerStakeAmt) / chipValue); i++)
            {
                playerStakesChips[i].SetActive(true);
            }

            playerStakeAmtAsync = Mathf.Max(PokerGameManager.instance.playerStakeSliderAmt, PokerGameManager.instance.playerStakeAmt);
        }

        if (opponentStakeAmtAsync != PokerGameManager.instance.opponentStakeAmt)
        {
            foreach (GameObject chip in opponentStakesChips)
            {
                chip.SetActive(false);
            }

            for (int i = 0; i < Mathf.Min(512, PokerGameManager.instance.opponentStakeAmt / chipValue); i++)
            {
                opponentStakesChips[i].SetActive(true);
            }

            opponentStakeAmtAsync = PokerGameManager.instance.opponentStakeAmt;
        }

        if (potAsync != PokerGameManager.instance.pot - PokerGameManager.instance.opponentStakeAmt - PokerGameManager.instance.playerStakeAmt)
        {
            foreach (GameObject chip in potChips)
            {
                chip.SetActive(false);
            }

            for (int i = 0; i < Mathf.Min(1024, (PokerGameManager.instance.pot - PokerGameManager.instance.opponentStakeAmt - PokerGameManager.instance.playerStakeAmt) / chipValue); i++)
            {
                potChips[i].SetActive(true);
            }

            potAsync = PokerGameManager.instance.pot;
        }

        
        
    }
}
