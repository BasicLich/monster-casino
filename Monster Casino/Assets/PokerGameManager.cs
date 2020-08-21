using SnapCall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PokerGameManager : MonoBehaviour
{
    public static PokerGameManager instance = null;

    // Every card in the deck in this order: A 2 3 4 5 6 7 8 9 10 J Q K; Club Diamonds Hearts Spades 
    public List<GameObject> cardList;

    // spots on the table to put the cards
    public List<Transform> tableCardPlacements;

    public GameObject player;
    public GameObject opponent;

    

    public List<GameObject> playerCardsPlacements;
    public List<GameObject> opponentCardPlacements;

    public GameObject dealer;
    public GameObject nonDealer;

    public Text potText, playerMoneyText, opponentMoneyText;

    public PlayerActionUIController playerUI;

    public GameObject textBox;

    public Transform playerTextLocation, opponentTextLocation, middleTextLocation, spawnedCardLocation;

    private Animator animator;

    private List<GameObject> deck;
    private Canvas canvas;
    private int currentTablePlacementIndex = 0;
    private int currentPlayerPlacementIndex = 0;
    private int currentOpponentPlacementIndex = 0;
    private List<GameObject> tableCards;
    private List<GameObject> playerCards;
    private List<GameObject> opponentCards;


    public int pot = 0;
    public int callAmt = 0;
    public int betAmt = 0;
    public int raiseAmt = 0;

    public int playerStakeAmt = 0;
    public int opponentStakeAmt = 0;
    public int playerStakeSliderAmt = 0;

    public int bigBlind = 50;
    public int smallBlind = 25;
    public int blindDifference = 0;

    public int baseBigBlind = 50;
    public int baseSmallBlind = 25;

    public int bigBlindPaid = 0;
    public int smallBlindPaid = 0;

    public int turn = 0;
    public int gameCount = 1;

    private Evaluator fiveCardEvaluator;

    private int opponentConfidence = 0;

    private bool dealerResponded = false;

    public List<AudioSource> cardSounds;
    public List<AudioSource> chipSounds;
    public AudioSource shuffleSound;

    private bool opponentAllIn, playerAllIn; //unused for now, just a reminder for future me

    public int securityLevel = 0;
    public Text securityText;
    public Image securityImage;

    private bool cardsRevealed = false;

    void Start()
    {
        bigBlind = baseBigBlind;
        smallBlind = baseSmallBlind;

        //uncomment to generate new evaluator table
        //Evaluator fiveCardEvaluator = new Evaluator(null, true, false, false, 1.25, true, false);
        //fiveCardEvaluator.SaveToFile("./eval_tables/five_card.ser");

        fiveCardEvaluator = new Evaluator(fileName: "./eval_tables/five_card.ser");

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        playerUI.gameObject.SetActive(false);
        playerStakeSliderAmt = 0;

        animator = GetComponent<Animator>();

        canvas = GetComponentInChildren<Canvas>();
        deck = new List<GameObject>();
        tableCards = new List<GameObject>();
        playerCards = new List<GameObject>();
        opponentCards = new List<GameObject>();

        

        //ResetGame(opponent, player);
    }

    public void StartGame(GameObject player, GameObject opponent)
    {
        bigBlind = baseBigBlind;
        smallBlind = baseSmallBlind;

        gameCount = 1;
        turn = 0;
        playerUI.gameObject.SetActive(false);
        playerStakeSliderAmt = 0;
        animator.SetTrigger("start");
        print("speed 1 activated");
        animator.speed = 1;

        opponent.GetComponentInChildren<Camera>().enabled = true;

        ShuffleDeck();
        ClearTable();

        dealer = opponent;
        nonDealer = player;

        dealer.GetComponent<PokerPlayer>().dealer = true;
        nonDealer.GetComponent<PokerPlayer>().dealer = false;
        dealer.GetComponent<PokerPlayer>().responding = false;
        nonDealer.GetComponent<PokerPlayer>().responding = false;

        //player.GetComponentInChildren<ParticleSystem>().Play();
        //opponent.GetComponentInChildren<ParticleSystem>().Play();
    }

    public void EndGame()
    {
        opponent.GetComponentInChildren<Camera>().enabled = false;
        PokerPlayer winner = player.GetComponent<PokerPlayer>().money > 0 ? player.GetComponent<PokerPlayer>() : opponent.GetComponent<PokerPlayer>();
        opponent.GetComponent<PokerPlayer>().battleSong.Stop();
        animator.SetTrigger("end");
        GameManager.instance.EndPoker(winner);
    }

    public void NextGame(GameObject player, GameObject opponent)
    {
        if (securityLevel > 0)
        {
            securityLevel = Mathf.Max(0, securityLevel - 5);
        }
        UpdateSecurityVisuals();

        playerUI.gameObject.SetActive(false);
        playerStakeSliderAmt = 0;

        GameObject oldDealer = dealer;
        dealer = nonDealer;
        nonDealer = oldDealer;

        ShuffleDeck();
        ClearTable();

        if (player.GetComponent<PokerPlayer>().money <= 0 || opponent.GetComponent<PokerPlayer>().money <= 0)
        {
            EndGame();
            return;
        }

        gameCount++;
        bigBlind *= 2;
        smallBlind *= 2;
        LaunchTextbox("Big blind and small blind have doubled.", 2);
        StartTurnOne();
        

        print("speed 1 activated");
        animator.speed = 1;

        dealer.GetComponent<PokerPlayer>().dealer = true;
        nonDealer.GetComponent<PokerPlayer>().dealer = false;
        dealer.GetComponent<PokerPlayer>().responding = false;
        nonDealer.GetComponent<PokerPlayer>().responding = false;
    }

    void ShuffleDeck()
    {
        deck.Clear();
        List<int> intList = System.Linq.Enumerable.Range(0, 51).ToList<int>();
        while(intList.Count > 0)
        {
            int nextCardIndex = UnityEngine.Random.Range(0, intList.Count);
            int nextCard = intList[nextCardIndex];
            deck.Add(cardList[nextCard]);
            intList.RemoveAt(nextCardIndex);
        }

        //shuffleSound.Play();
    }

    void ClearTable()
    {
        tableCards.Clear();
        playerCards.Clear();
        opponentCards.Clear();

        currentTablePlacementIndex = 0;
        currentPlayerPlacementIndex = 0;
        currentOpponentPlacementIndex = 0;

        callAmt = 0;
        raiseAmt = 0;
        betAmt = 0;
        playerStakeAmt = 0;
        opponentStakeAmt = 0;
        blindDifference = 0;
        bigBlindPaid = 0;
        smallBlindPaid = 0;
        pot = 0;

        dealerResponded = false;
        cardsRevealed = false;

        foreach (Transform childCard in spawnedCardLocation.transform)
        {
            GameObject.Destroy(childCard.gameObject);
        }
    }

    void PlayNextCardToTable()
    {
        currentTablePlacementIndex %= 5;
        GameObject card = GameObject.Instantiate(deck[0], spawnedCardLocation.transform);
        tableCards.Add(card);
        card.transform.position = tableCardPlacements[currentTablePlacementIndex].transform.position;
        deck.RemoveAt(0);

        Animator cardAnimator = card.GetComponent<Animator>();
        cardAnimator.SetBool("revealed", true);

        currentTablePlacementIndex++;

        GameObject cardSound = GameObject.Instantiate(cardSounds[UnityEngine.Random.Range(0, cardSounds.Count - 1)].gameObject);
        cardSound.SetActive(true);
        GameObject.Destroy(cardSound, 1f);
    }

    void PlayNextCardToDealer()
    {
        if(player == dealer)
        {
            PlayNextCardToPlayer();
        } else
        {
            PlayNextCardToOpponent();
        }
    }

    void PlayNextCardToNonDealer()
    {
        if (player == dealer)
        {
            PlayNextCardToOpponent();
        }
        else
        {
            PlayNextCardToPlayer();
        }
    }

    void PlayNextCardToPlayer()
    {
        GameObject card = GameObject.Instantiate(deck[0], spawnedCardLocation.transform);
        playerCards.Add(card);
        card.transform.position = playerCardsPlacements[currentPlayerPlacementIndex].transform.position;
        deck.RemoveAt(0);

        Animator cardAnimator = card.GetComponent<Animator>();
        cardAnimator.SetBool("revealed", true);

        currentPlayerPlacementIndex++;

        GameObject cardSound = GameObject.Instantiate(cardSounds[UnityEngine.Random.Range(0, cardSounds.Count - 1)].gameObject);
        cardSound.SetActive(true);
        GameObject.Destroy(cardSound, 1f);
    }

    void PlayNextCardToOpponent()
    {
        GameObject card = GameObject.Instantiate(deck[0], spawnedCardLocation.transform);
        opponentCards.Add(card);
        card.transform.position = opponentCardPlacements[currentOpponentPlacementIndex].transform.position;
        deck.RemoveAt(0);

        Animator cardAnimator = card.GetComponent<Animator>();
        //cardAnimator.SetBool("revealed", true);

        currentOpponentPlacementIndex++;

        GameObject cardSound = GameObject.Instantiate(cardSounds[UnityEngine.Random.Range(0, cardSounds.Count - 1)].gameObject);
        cardSound.SetActive(true);
        GameObject.Destroy(cardSound, 1f);
    }

    void DealerPaysSmallBlind()
    {
        
        smallBlindPaid = Mathf.Min(smallBlind, dealer.GetComponent<PokerPlayer>().money);
        LaunchTextbox("Dealer " + dealer.GetComponent<PokerPlayer>().playerName + " pays small blind of " + smallBlindPaid + ".", dealer.GetComponent<PokerPlayer>().human ? 0 : 1);
        dealer.GetComponent<PokerPlayer>().money -= smallBlindPaid;
        pot += smallBlindPaid;
    }

    void NonDealerPaysBigBlind()
    {
        bigBlindPaid = Mathf.Min(bigBlind, nonDealer.GetComponent<PokerPlayer>().money);
        LaunchTextbox(nonDealer.GetComponent<PokerPlayer>().playerName + " pays big blind of " + bigBlindPaid + ".", nonDealer.GetComponent<PokerPlayer>().human ? 0 : 1);
        smallBlindPaid = 0;
        nonDealer.GetComponent<PokerPlayer>().money -= bigBlindPaid;
        pot += bigBlindPaid;
        //blindDifference = bigBlind - smallBlind;
        blindDifference = bigBlindPaid - smallBlindPaid;
    }

    void DealerCalls()
    {
        pot += callAmt;
    }

    void DealerActs()
    {
        print("DealerActs");
        if (dealerResponded)
            return;

        dealer.GetComponentInChildren<BattleEffects>().SendMessage("StartLight");
        nonDealer.GetComponentInChildren<BattleEffects>().SendMessage("StopLight");

        if (blindDifference > 0)
        {
            callAmt += blindDifference;
            blindDifference = 0;
        }
        dealer.SendMessage("Poke");
        if(player == dealer)
        {
            PlayerActs();
        } else
        {
            OpponentActs();
        }
    }


    void NonDealerActs()
    {
        print("NonDealerActs");
        nonDealer.SendMessage("Poke");

        nonDealer.GetComponentInChildren<BattleEffects>().SendMessage("StartLight");
        dealer.GetComponentInChildren<BattleEffects>().SendMessage("StopLight");

        if (player == dealer)
        {
            OpponentActs();
        }
        else
        {
            PlayerActs();
        }
    }

    void NextTurn()
    {
        print("NextTurn");
        dealerResponded = false;

        cardsRevealed = false;

        if (turn < 5)
            turn++;
        else
            Showdown();
    }

    void StartTurnOne()
    {
        if(!opponent.GetComponent<PokerPlayer>().battleSong.isPlaying)
            opponent.GetComponent<PokerPlayer>().battleSong.Play();

        animator.SetTrigger("startturn1");
        turn = 1;
    }


    void PlayerActs()
    {
        print("player acts");
        player.GetComponent<PokerPlayer>().responding = false;
        opponent.GetComponent<PokerPlayer>().responding = false;
        animator.speed = 0;
        playerUI.noRaise = false;
        print("ui active");
        playerUI.gameObject.SetActive(true);
        playerUI.ResetBetSlider();
    }

    IEnumerator PlayerResponds()
    {
        if (player.GetComponent<PokerPlayer>().dealer)
            dealerResponded = true;

        player.GetComponentInChildren<BattleEffects>().SendMessage("StartLight");
        opponent.GetComponentInChildren<BattleEffects>().SendMessage("StopLight");

        player.SendMessage("Poke");
        if (blindDifference > 0)
        {
            callAmt += blindDifference;
            blindDifference = 0;
        }
        player.GetComponent<PokerPlayer>().responding = true;
        opponent.GetComponent<PokerPlayer>().responding = false;
        animator.speed = 0;

        playerUI.noRaise = false;
        print("ui active");
        playerUI.gameObject.SetActive(true);
        playerUI.ResetBetSlider();
        yield return "success";
    }

    void PlayerRespondsRaise()
    {
        if (player.GetComponent<PokerPlayer>().dealer)
            dealerResponded = true;

        player.GetComponentInChildren<BattleEffects>().SendMessage("StartLight");
        opponent.GetComponentInChildren<BattleEffects>().SendMessage("StopLight");

        player.SendMessage("Poke");
        player.GetComponent<PokerPlayer>().responding = true;
        opponent.GetComponent<PokerPlayer>().responding = false;
        animator.speed = 0;

        playerUI.noRaise = true;
        print("ui active");
        playerUI.gameObject.SetActive(true);
        playerUI.ResetBetSlider();
    }

    public void PlayerCalls()
    {
        print("player calls " + callAmt);
        player.SendMessage("Poke");
        LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " calls " + callAmt + ".", 0);
        player.GetComponent<PokerPlayer>().money -= callAmt;
        pot += callAmt;
        callAmt = 0;
        playerStakeAmt = 0;
        opponentStakeAmt = 0;

        // animation of chips going into pot?

        playerUI.gameObject.SetActive(false);
        playerStakeSliderAmt = 0;

        animator.speed = 0;
        StartCoroutine(Wait(2f));
    }

    public void PlayerChecks()
    {
        print("player checks");
        player.SendMessage("Poke");
        LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " checks.", 0);
        callAmt = 0;
        playerStakeAmt = 0;
        opponentStakeAmt = 0;
        playerUI.gameObject.SetActive(false);
        playerStakeSliderAmt = 0;

        animator.speed = 0;
        StartCoroutine(Wait(2f));
    }

    public void PlayerBets()
    {
        print("player bets " + betAmt);
        player.SendMessage("Poke");

        betAmt = betAmt - callAmt;
        playerStakeAmt = betAmt + callAmt;
        playerStakeSliderAmt = 0;

        if (callAmt > 0)
        {
            LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " calls " + callAmt + " and raises " + betAmt + ".", 0);
        } else
        {
            LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " bets " + betAmt + ".", 0);
        }

        
        player.GetComponent<PokerPlayer>().money -= (betAmt + callAmt);
        pot += betAmt;
        callAmt = betAmt;
        betAmt = 0;

        playerUI.gameObject.SetActive(false);
        playerStakeSliderAmt = 0;

        animator.speed = 0;
        StartCoroutine(Wait(2f, OpponentResponds()));

        //print("animator speed 1");
        //animator.speed = 1;
    }

    public void PlayerFolds()
    {
        print("player folds");
        player.SendMessage("Poke");
        LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " folds. " + opponent.GetComponent<PokerPlayer>().playerName + " wins the pot.", 0);
        opponent.GetComponentInChildren<BattleEffects>().SendMessage("StartChips");

        print("opponent wins pot");
        opponent.GetComponent<PokerPlayer>().money += pot;
        pot = 0;

        playerUI.gameObject.SetActive(false);
        playerStakeSliderAmt = 0;
        //print("animator speed 1");
        //animator.speed = 1;
        animator.speed = 0;
        StartCoroutine(Wait(2f, RestartGame()));
    }

    IEnumerator RestartGame()
    {
        player.GetComponentInChildren<BattleEffects>().SendMessage("StopChips");
        opponent.GetComponentInChildren<BattleEffects>().SendMessage("StopChips");
        NextGame(player, opponent);
        yield return "success";
    }

    public void PlayerRaises()
    {
        print(player.GetComponent<PokerPlayer>().playerName + " raises " + raiseAmt + ".");
        player.SendMessage("Poke");
        LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " raises " + raiseAmt + ".", 0);
        player.GetComponent<PokerPlayer>().money -= raiseAmt;
        pot += raiseAmt;
        callAmt = raiseAmt;
        raiseAmt = 0;
        playerStakeAmt = raiseAmt;

        animator.speed = 0;
        StartCoroutine(Wait(2f, OpponentRespondsRaise()));

        playerUI.gameObject.SetActive(false);
        playerStakeSliderAmt = 0;
        //print("animator speed 1");
        //animator.speed = 1;
    }

    void OpponentActs()
    {
        print("opponent acts");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " is thinking...", 1);
        CalculateOpponentConfidence();
        print(opponentConfidence);

        opponent.GetComponentInChildren<BattleEffects>().SendMessage("StartLight");
        player.GetComponentInChildren<BattleEffects>().SendMessage("StopLight");

        player.GetComponent<PokerPlayer>().responding = false;
        opponent.GetComponent<PokerPlayer>().responding = false;

        int maxBetAmount = Mathf.Min(opponent.GetComponent<PokerPlayer>().money, player.GetComponent<PokerPlayer>().money);
        int minBetAmount = 0;

        animator.speed = 0;

        if (opponentConfidence < 1000)
        {
            if (callAmt > 0)
            {
                if (callAmt > opponent.GetComponent<PokerPlayer>().money / 1000)
                    StartCoroutine(Wait(2f, OpponentFolds()));
                else
                    StartCoroutine(Wait(2f, OpponentCalls()));
            }
            else
            {
                StartCoroutine(Wait(2f, OpponentChecks()));
            }
        }
        else if (opponentConfidence < 3000)
        {
            if (callAmt > 0)
            {
                StartCoroutine(Wait(2f, OpponentCalls()));
            }
            else
            {
                StartCoroutine(Wait(2f, OpponentChecks()));
            }
        } else
        {
            betAmt = Mathf.Max((int)Mathf.Lerp(minBetAmount, maxBetAmount, ((float)opponentConfidence) / 6000f), maxBetAmount);
            if (betAmt > 0)
            {
                StartCoroutine(Wait(2f, OpponentBets()));
            } else
            {
                if (callAmt > 0)
                {
                    StartCoroutine(Wait(2f, OpponentCalls()));
                }
                else
                {
                    StartCoroutine(Wait(2f, OpponentChecks()));
                }
            }
        }
        

    }

    void CalculateOpponentConfidence()
    {
        ulong t1,t2,t3,t4,t5,o1,o2;
        opponentConfidence = 0;

        switch (turn)
        {
            case 1:
                opponentConfidence += opponentCards[0].GetComponent<Card>().suit == opponentCards[1].GetComponent<Card>().suit ? 2 : 0;
                opponentConfidence += opponentCards[0].GetComponent<Card>().val == opponentCards[1].GetComponent<Card>().val ? 2 : 0;
                opponentConfidence += opponentCards[0].GetComponent<Card>().val == "A" ? 3 : 0;
                opponentConfidence += opponentCards[1].GetComponent<Card>().val == "A" ? 3 : 0;
                opponentConfidence += opponentCards[0].GetComponent<Card>().val == "K" ? 2 : 0;
                opponentConfidence += opponentCards[1].GetComponent<Card>().val == "K" ? 2 : 0;
                opponentConfidence += opponentCards[0].GetComponent<Card>().val == "Q" ? 2 : 0;
                opponentConfidence += opponentCards[1].GetComponent<Card>().val == "Q" ? 2 : 0;
                opponentConfidence += opponentCards[0].GetComponent<Card>().val == "J" ? 1 : 0;
                opponentConfidence += opponentCards[1].GetComponent<Card>().val == "J" ? 1 : 0;
                opponentConfidence += opponentCards[0].GetComponent<Card>().val == "10" ? 1 : 0;
                opponentConfidence += opponentCards[1].GetComponent<Card>().val == "10" ? 1 : 0;
                opponentConfidence *= 339;
                break;
            case 2:
                t1 = tableCards[0].GetComponent<Card>().GetBit();
                t2 = tableCards[1].GetComponent<Card>().GetBit();
                t3 = tableCards[2].GetComponent<Card>().GetBit();

                o1 = opponentCards[0].GetComponent<Card>().GetBit();
                o2 = opponentCards[1].GetComponent<Card>().GetBit();

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o1 | o2));
                break;
            case 3:
                t1 = tableCards[0].GetComponent<Card>().GetBit();
                t2 = tableCards[1].GetComponent<Card>().GetBit();
                t3 = tableCards[2].GetComponent<Card>().GetBit();
                t4 = tableCards[3].GetComponent<Card>().GetBit();

                o1 = opponentCards[0].GetComponent<Card>().GetBit();
                o2 = opponentCards[1].GetComponent<Card>().GetBit();

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o1 | o2));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | o1));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | o2));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o1 | o2));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | o1 | t4 | o2));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o1 | t3 | t4 | o2));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o1 | t2 | t3 | t4 | o2));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o2 | o1));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | o2 | t4 | o1));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o2 | t3 | t4 | o1));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o2 | t2 | t3 | t4 | o1));
                break;
            case 4:
                t1 = tableCards[0].GetComponent<Card>().GetBit();
                t2 = tableCards[1].GetComponent<Card>().GetBit();
                t3 = tableCards[2].GetComponent<Card>().GetBit();
                t4 = tableCards[3].GetComponent<Card>().GetBit();
                t5 = tableCards[4].GetComponent<Card>().GetBit();

                o1 = opponentCards[0].GetComponent<Card>().GetBit();
                o2 = opponentCards[1].GetComponent<Card>().GetBit();

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | t5));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o1 | t2 | t3 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o1 | o2 | t3 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o1 | t2 | o2 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o1 | t2 | t3 | o2 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o1 | t2 | t3 | t4 | o2));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o1 | t3 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o1 | o2 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o1 | t3 | o2 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o1 | t3 | t4 | o2));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | o1 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | o1 | o2 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | o1 | t4 | o2));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o1 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o1 | o2));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | o1));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o2 | t2 | t3 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o2 | o1 | t3 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o2 | t2 | o1 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o2 | t2 | t3 | o1 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(o2 | t2 | t3 | t4 | o1));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o2 | t3 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o2 | o1 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o2 | t3 | o1 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | o2 | t3 | t4 | o1));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | o2 | t4 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | o2 | o1 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | o2 | t4 | o1));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o2 | t5));
                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o2 | o1));

                opponentConfidence = Mathf.Max(opponentConfidence, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | o2));
                break;
        }
        
    }

    IEnumerator OpponentResponds()
    {
        if (opponent.GetComponent<PokerPlayer>().dealer)
            dealerResponded = true;

        opponent.GetComponentInChildren<BattleEffects>().SendMessage("StartLight");
        player.GetComponentInChildren<BattleEffects>().SendMessage("StopLight");

        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " is thinking...", 1);
        player.GetComponent<PokerPlayer>().responding = false;
        opponent.GetComponent<PokerPlayer>().responding = true;
        CalculateOpponentConfidence();
        print(opponentConfidence);

        if (blindDifference > 0)
        {
            callAmt += blindDifference;
            blindDifference = 0;
        }

        int maxBetAmount = Mathf.Min(opponent.GetComponent<PokerPlayer>().money, player.GetComponent<PokerPlayer>().money);
        int minBetAmount = Mathf.Min(opponent.GetComponent<PokerPlayer>().money, callAmt);

        animator.speed = 0;
        if (opponentConfidence < 1000)
        {
            yield return Wait(2f, OpponentFolds());
        } else if (opponentConfidence < 3000)
        {
            yield return Wait(2f, OpponentCalls());
        } else
        {
            int maxRaiseAmount = maxBetAmount - callAmt;
            raiseAmt = Mathf.Max((int)Mathf.Lerp(0, maxRaiseAmount, ((float)opponentConfidence) / 6000f), maxRaiseAmount);
            if (raiseAmt > 0)
            {
                yield return Wait(2f, OpponentRaises());
            } else
            {
                yield return Wait(2f, OpponentCalls());
            }
            
        }
            

        // if confident, raise or call, else fold
    }

    IEnumerator OpponentRespondsRaise()
    {
        if (opponent.GetComponent<PokerPlayer>().dealer)
            dealerResponded = true;

        opponent.GetComponentInChildren<BattleEffects>().SendMessage("StartLight");
        player.GetComponentInChildren<BattleEffects>().SendMessage("StopLight");

        CalculateOpponentConfidence();
        print(opponentConfidence);

        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " responds to your raise.", 1);
        player.GetComponent<PokerPlayer>().responding = false;
        opponent.GetComponent<PokerPlayer>().responding = true;

        int maxBetAmount = callAmt;
        int minBetAmount = callAmt;

        animator.speed = 0;

        if (opponentConfidence < 2000)
        {
            yield return Wait(2f, OpponentFolds());
        }
        else
        {
            yield return Wait(2f, OpponentCalls());
        }
        //StartCoroutine(Wait(2f, OpponentCalls()));

        // if confident, call, else fold
    }

    IEnumerator OpponentCalls()
    {
        print("opponent calls " + callAmt);
        opponent.SendMessage("Poke");

        if(callAmt > opponent.GetComponent<PokerPlayer>().money)
        {
            LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " is all in!", 1);
            pot += opponent.GetComponent<PokerPlayer>().money;
            opponent.GetComponent<PokerPlayer>().money = 0;
        } else
        {
            LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " calls " + callAmt + ".", 1);
            opponent.GetComponent<PokerPlayer>().money -= callAmt;
            pot += callAmt;
        }

        
        callAmt = 0;
        playerStakeAmt = 0;
        opponentStakeAmt = 0;

        animator.speed = 0;
        yield return Wait(2f);
        
        
    }

    IEnumerator Wait(float seconds, IEnumerator callback = null)
    {
        print("waiting " + seconds + " seconds");
        animator.speed = 0;
        yield return new WaitForSeconds(seconds);
        animator.speed = 1;
        print("finished waiting");
        if(callback != null)
            yield return callback;
    }

    IEnumerator OpponentChecks()
    {
        print("opponent checks");
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " checks.", 1);
        callAmt = 0;
        playerStakeAmt = 0;
        opponentStakeAmt = 0;

        animator.speed = 0;
        yield return Wait(2f);
    }

    IEnumerator OpponentBets()
    {
        print("opponent bets " + betAmt);
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " bets " + betAmt + ".", 1);
        opponent.GetComponent<PokerPlayer>().money -= betAmt;
        pot += betAmt;
        callAmt = betAmt;
        opponentStakeAmt = callAmt;
        betAmt = 0;

        yield return Wait(2f, PlayerResponds());

    }

    IEnumerator OpponentFolds()
    {
        print("opponent folds");
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " folds. " + player.GetComponent<PokerPlayer>().playerName + " wins the pot.", 1);
        player.GetComponentInChildren<BattleEffects>().SendMessage("StartChips");

        print("player wins pot");
        player.GetComponent<PokerPlayer>().money += pot;
        pot = 0;

        
        animator.speed = 0;
        yield return Wait(2f, RestartGame());

    }

    IEnumerator OpponentRaises()
    {
        print("opponent raises " + raiseAmt);
        opponent.SendMessage("Poke");

        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " calls " + callAmt + " and raises " + raiseAmt + ".", 1);
        opponent.GetComponent<PokerPlayer>().money -= raiseAmt;
        pot += raiseAmt;
        callAmt = raiseAmt;
        raiseAmt = 0;
        opponentStakeAmt = raiseAmt;

        PlayerRespondsRaise();
        yield return "success";
    }

    void Showdown()
    {
        print("showdown");
        player.SendMessage("Poke");
        opponent.SendMessage("Poke");
        LaunchTextbox("SHOWDOWN", 2);


        opponentCards[0].GetComponent<Animator>().SetBool("revealed", true);
        opponentCards[1].GetComponent<Animator>().SetBool("revealed", true);

        // i don't know how to code combinations so i'm writing the worst code in the world instead
        // calculate player's hand value (check every possible 5 card combo and pick best score)
        ulong t1 = tableCards[0].GetComponent<Card>().GetBit();
        ulong t2 = tableCards[1].GetComponent<Card>().GetBit();
        ulong t3 = tableCards[2].GetComponent<Card>().GetBit();
        ulong t4 = tableCards[3].GetComponent<Card>().GetBit();
        ulong t5 = tableCards[4].GetComponent<Card>().GetBit();

        ulong p1 = playerCards[0].GetComponent<Card>().GetBit();
        ulong p2 = playerCards[1].GetComponent<Card>().GetBit();

        ulong o1 = opponentCards[0].GetComponent<Card>().GetBit();
        ulong o2 = opponentCards[1].GetComponent<Card>().GetBit();

        int playerScore = 0;
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate( t1 | t2 | t3 | t4 | t5 ));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p1 | t2 | t3 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p1 | p2 | t3 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p1 | t2 | p2 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p1 | t2 | t3 | p2 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p1 | t2 | t3 | t4 | p2));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | p1 | t3 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | p1 | p2 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | p1 | t3 | p2 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | p1 | t3 | t4 | p2));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | p1 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | p1 | p2 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | p1 | t4 | p2));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | p1 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | p1 | p2));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | p1));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p2 | t2 | t3 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p2 | p1 | t3 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p2 | t2 | p1 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p2 | t2 | t3 | p1 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(p2 | t2 | t3 | t4 | p1));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | p2 | t3 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | p2 | p1 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | p2 | t3 | p1 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | p2 | t3 | t4 | p1));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | p2 | t4 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | p2 | p1 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | p2 | t4 | p1));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | p2 | t5));
        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | p2 | p1));

        playerScore = Mathf.Max(playerScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | p2));

        print("player score: " + playerScore);



        // calculate opponent's hand value
        int opponentScore = 0;
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | t5));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o1 | t2 | t3 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o1 | o2 | t3 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o1 | t2 | o2 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o1 | t2 | t3 | o2 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o1 | t2 | t3 | t4 | o2));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | o1 | t3 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | o1 | o2 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | o1 | t3 | o2 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | o1 | t3 | t4 | o2));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | o1 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | o1 | o2 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | o1 | t4 | o2));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o1 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o1 | o2));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | o1));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o2 | t2 | t3 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o2 | o1 | t3 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o2 | t2 | o1 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o2 | t2 | t3 | o1 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(o2 | t2 | t3 | t4 | o1));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | o2 | t3 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | o2 | o1 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | o2 | t3 | o1 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | o2 | t3 | t4 | o1));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | o2 | t4 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | o2 | o1 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | o2 | t4 | o1));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o2 | t5));
        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | o2 | o1));

        opponentScore = Mathf.Max(opponentScore, fiveCardEvaluator.Evaluate(t1 | t2 | t3 | t4 | o2));

        print("opponent score: " + opponentScore);

        StartCoroutine(Wait(1f, ShowdownResults(playerScore, opponentScore)));
    }

    IEnumerator ShowdownResults(int playerScore, int opponentScore)
    {
        if (playerScore > opponentScore)
        {
            print("player wins the pot");
            PotToPlayer();
            LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " wins the pot!", 0);
            
            player.GetComponentInChildren<BattleEffects>().SendMessage("StartChips");
        }
        else if (playerScore == opponentScore)
        {
            print("draw: player splits the pot");
            SplitPot();
            LaunchTextbox("Draw... " + player.GetComponent<PokerPlayer>().playerName + " and " + opponent.GetComponent<PokerPlayer>().playerName + " split the pot.", 2);
            player.GetComponentInChildren<BattleEffects>().SendMessage("StartChips");
            opponent.GetComponentInChildren<BattleEffects>().SendMessage("StartChips");
        }
        else
        {
            print("opponent wins the pot");
            PotToOpponent();
            LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " wins the pot!", 0);

            opponent.GetComponentInChildren<BattleEffects>().SendMessage("StartChips");
        }

        yield return Wait(4f);

        player.GetComponentInChildren<BattleEffects>().SendMessage("StopChips");
        opponent.GetComponentInChildren<BattleEffects>().SendMessage("StopChips");

        NextGame(player, opponent);
    }

    void PotToPlayer()
    {
        PokerPlayer pp = player.GetComponent<PokerPlayer>();
        pp.money += pot;
        pot = 0;
    }

    void PotToOpponent()
    {
        PokerPlayer pp = opponent.GetComponent<PokerPlayer>();
        pp.money += pot;
        pot = 0;
    }

    void SplitPot()
    {
        if (pot % 2 == 1)
            pot--;

        PokerPlayer pp = player.GetComponent<PokerPlayer>();
        pp.money += pot/2;

        PokerPlayer op = opponent.GetComponent<PokerPlayer>();
        pp.money += pot/2;
        pot = 0;
    }

    void LaunchTextbox(string text, int mode)
    {
        Transform parentTransform = mode == 0 ? playerTextLocation : opponentTextLocation;
        if(mode == 2)
        {
            parentTransform = middleTextLocation;
        }
        Vector3 textMovement = mode == 0 ? Vector3.down * 130f : Vector3.down * 130f;
        GameObject launchedTextbox = GameObject.Instantiate(textBox, parentTransform);
        launchedTextbox.GetComponent<PokerGameTextBox>().LaunchText(text, Vector3.zero, textMovement, 7f);
        launchedTextbox.SetActive(true);
    }



    void Update()
    {
        animator.SetInteger("turn", turn);
        potText.text = pot.ToString();
        if(player && opponent)
        {
            playerMoneyText.text = player.GetComponent<PokerPlayer>().money.ToString();
            opponentMoneyText.text = opponent.GetComponent<PokerPlayer>().money.ToString();
        }
        
    }

    public void UsePartyAbility(int partyMember)
    {
        if(cardsRevealed)
        {
            LaunchTextbox("No need to peek.", 0);
        } else if (securityLevel <= 75)
        {
            securityLevel = Mathf.Min(100, securityLevel + 25);
            UpdateSecurityVisuals();
            LaunchTextbox("Dog used Peek!", 0);
            opponentCards[0].GetComponent<Animator>().SetBool("revealed", true);
            opponentCards[1].GetComponent<Animator>().SetBool("revealed", true);
            cardsRevealed = true;
        } else
        {
            LaunchTextbox("Dog says: Sorry, I've got too much heat on me.", 0);
        }
        


    }

    private void UpdateSecurityVisuals()
    {
        if(securityLevel > 0)
        {
            securityText.gameObject.SetActive(true);
            securityText.text = securityLevel.ToString() + "%";
            securityImage.gameObject.SetActive(true);
        } else
        {
            securityText.gameObject.SetActive(false);
            securityText.text = securityLevel.ToString() + "%";
            securityImage.gameObject.SetActive(false);
        }
    }
}
