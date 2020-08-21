using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActionUIController : MonoBehaviour
{
    public Slider betSlider;
    public Button checkButton, callButton, betButton, raiseButton, foldButton;
    public Text betAmtText;

    public int minBetAmount, maxBetAmount;

    public int betAmt;

    public bool noRaise = false;

    public void ResetBetSlider()
    {
        maxBetAmount = noRaise ? PokerGameManager.instance.callAmt : Mathf.Min(PokerGameManager.instance.opponent.GetComponent<PokerPlayer>().money, PokerGameManager.instance.player.GetComponent<PokerPlayer>().money);
        minBetAmount = PokerGameManager.instance.callAmt; //responding ? PokerGameManager.instance.callAmt : 0;
        maxBetAmount = Mathf.Max(maxBetAmount, minBetAmount);
        betSlider.value = minBetAmount;
    }

    void Start()
    {
        
    }

    void Update()
    {
        bool responding = PokerGameManager.instance.player.GetComponent<PokerPlayer>().responding;
        maxBetAmount = noRaise ? PokerGameManager.instance.callAmt : Mathf.Min(PokerGameManager.instance.opponent.GetComponent<PokerPlayer>().money, PokerGameManager.instance.player.GetComponent<PokerPlayer>().money);
        minBetAmount = Mathf.Min(PokerGameManager.instance.player.GetComponent<PokerPlayer>().money, PokerGameManager.instance.callAmt); //responding ? PokerGameManager.instance.callAmt : 0;
        maxBetAmount = Mathf.Max(maxBetAmount, minBetAmount);

        betSlider.minValue = minBetAmount;
        betSlider.maxValue = maxBetAmount;

        betSlider.enabled = !noRaise;

        PokerGameManager.instance.playerStakeSliderAmt = (int)betSlider.value;


        if (betSlider.value == 0)
        {
            checkButton.gameObject.SetActive(true);

            callButton.gameObject.SetActive(false);
            betButton.gameObject.SetActive(false);
            raiseButton.gameObject.SetActive(false);
            
        } else if (betSlider.value == minBetAmount)
        {
            callButton.gameObject.SetActive(true);

            checkButton.gameObject.SetActive(false);
            betButton.gameObject.SetActive(false);
            raiseButton.gameObject.SetActive(false);
        }

        if (betSlider.value > minBetAmount)
        {
            if (responding)
            {
                raiseButton.gameObject.SetActive(true);

                betButton.gameObject.SetActive(false);
                callButton.gameObject.SetActive(false);
                checkButton.gameObject.SetActive(false);

            }
            else
            {
                betButton.gameObject.SetActive(true);

                raiseButton.gameObject.SetActive(false);
                callButton.gameObject.SetActive(false);
                checkButton.gameObject.SetActive(false);
            }
        }

        if (betSlider.value > 0)
        {
            betAmtText.text = betSlider.value.ToString();
            betAmtText.enabled = true;
        } else
        {
            betAmtText.text = "";
            betAmtText.enabled = false;
        }

        betAmt = (int)betSlider.value;
        if (responding)
        {
            PokerGameManager.instance.raiseAmt = betAmt;
        } else
        {
            PokerGameManager.instance.betAmt = betAmt;

        }
    }
}
