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

    public Transform playerTextLocation, opponentTextLocation, spawnedCardLocation;

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

    public int bigBlind = 50;
    public int smallBlind = 25;

    public int turn = 0;
    public int gameCount = 1;

    private Evaluator fiveCardEvaluator;


    void Start()
    {
        //uncomment to generate new evaluator table
        //Evaluator fiveCardEvaluator = new Evaluator(null, true, false, false, 1.25, true, false);
        //fiveCardEvaluator.SaveToFile("./eval_tables/five_card.ser");
        fiveCardEvaluator = new Evaluator(fileName: "./eval_tables/five_card.ser");

        /*print(fiveCardEvaluator.Evaluate(0x00000001 | 0x00000002 | 0x00000004 | 0x00000008 | 0x00000010));
        print(fiveCardEvaluator.Evaluate(0x1000000000000 | 0x0100000000000 | 0x0010000000000 | 0x0001000000000 | 0x0000100000000));
        print(fiveCardEvaluator.Evaluate(0x00000001 | 0x00000010 | 0x00000100 | 0x00001000 | 0x00010000));
        print(fiveCardEvaluator.Evaluate(0x10000000 | 0x20000000 | 0x40000000 | 0x80000000 | 0x01000000));

        print(fiveCardEvaluator.Evaluate(0x0000000010000 | 0x0000000008000 | 0x0000000000400 | 0x0000200000000 | 0x0000000000001));*/

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        playerUI.gameObject.SetActive(false);

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
        gameCount = 1;
        turn = 0;
        playerUI.gameObject.SetActive(false);
        animator.SetTrigger("start");
        print("speed 1 activated");
        animator.speed = 1;

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

    public void NextGame(GameObject player, GameObject opponent)
    {
        
        playerUI.gameObject.SetActive(false);

        GameObject oldDealer = dealer;
        dealer = nonDealer;
        nonDealer = oldDealer;

        gameCount++;
        turn = 1;

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
    }

    void ClearTable()
    {
        tableCards.Clear();
        playerCards.Clear();
        opponentCards.Clear();
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
    }

    void DealerPaysSmallBlind()
    {
        dealer.GetComponent<PokerPlayer>().money -= smallBlind;
        pot += smallBlind;
    }

    void NonDealerPaysBigBlind()
    {
        nonDealer.GetComponent<PokerPlayer>().money -= bigBlind;
        pot += bigBlind;
        callAmt = bigBlind - smallBlind;
    }

    void DealerCalls()
    {
        pot += callAmt;
    }

    void DealerActs()
    {
        print("DealerActs");
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
        if (turn < 5)
            turn++;
        else
            Showdown();
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
        player.SendMessage("Poke");
        print("player responds");
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
        print("player responds (to raise)");
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
        playerUI.gameObject.SetActive(false);

        animator.speed = 0;
        StartCoroutine(Wait(2f));
    }

    public void PlayerChecks()
    {
        print("player checks");
        player.SendMessage("Poke");
        LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " checks.", 0);
        callAmt = 0;
        playerUI.gameObject.SetActive(false);

        animator.speed = 0;
        StartCoroutine(Wait(2f));
    }

    public void PlayerBets()
    {
        print("player bets " + betAmt);
        player.SendMessage("Poke");
        LaunchTextbox(player.GetComponent<PokerPlayer>().playerName + " bets " + betAmt + ".", 0);
        player.GetComponent<PokerPlayer>().money -= betAmt;
        pot += betAmt;
        callAmt = betAmt;
        betAmt = 0;

        playerUI.gameObject.SetActive(false);

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

        print("opponent wins pot");
        opponent.GetComponent<PokerPlayer>().money += pot;
        pot = 0;

        playerUI.gameObject.SetActive(false);
        //print("animator speed 1");
        //animator.speed = 1;
        animator.speed = 0;
        StartCoroutine(Wait(2f));
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

        animator.speed = 0;
        StartCoroutine(Wait(2f));

        OpponentRespondsRaise();

        playerUI.gameObject.SetActive(false);
        //print("animator speed 1");
        //animator.speed = 1;
    }

    void OpponentActs()
    {
        print("opponent acts");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " is thinking...", 1);
        player.GetComponent<PokerPlayer>().responding = false;
        opponent.GetComponent<PokerPlayer>().responding = false;

        int maxBetAmount = Mathf.Min(opponent.GetComponent<PokerPlayer>().money, player.GetComponent<PokerPlayer>().money);
        int minBetAmount = 0;

        animator.speed = 0;
        StartCoroutine(Wait(2f, OpponentChecks()));

    }

    IEnumerator OpponentResponds()
    {
        print("opponent responds");
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " is thinking...", 1);
        player.GetComponent<PokerPlayer>().responding = false;
        opponent.GetComponent<PokerPlayer>().responding = true;

        int maxBetAmount = Mathf.Min(opponent.GetComponent<PokerPlayer>().money, player.GetComponent<PokerPlayer>().money);
        int minBetAmount = callAmt;

        animator.speed = 0;
        yield return Wait(2f, OpponentCalls());

        // if confident, raise or call, else fold
    }

    /*IEnumerator OpponentResponseCoroutine()
    {
        print("OpponentResponseCoroutine");
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        print("waiting 1 second");
        //trigger opponent thinking animation
        yield return new WaitForSeconds(1);

        OpponentCalls();
        
        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }*/

    void OpponentRespondsRaise()
    {
        print("opponent responds (to raise)");
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " responds to your raise.", 1);
        player.GetComponent<PokerPlayer>().responding = false;
        opponent.GetComponent<PokerPlayer>().responding = true;

        int maxBetAmount = callAmt;
        int minBetAmount = callAmt;

        animator.speed = 0;
        StartCoroutine(Wait(2f, OpponentCalls()));

        // if confident, call, else fold
    }

    IEnumerator OpponentCalls()
    {
        print("opponent calls " + callAmt);
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " calls " + callAmt + ".", 1);
        opponent.GetComponent<PokerPlayer>().money -= callAmt;
        pot += callAmt;
        callAmt = 0;

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

    IEnumerator OpponentCallCoroutine()
    {
        print("waiting 1 second");
        yield return new WaitForSeconds(1f);

        print("animator speed 1");
        animator.speed = 1;
        print("1 sec later");
    }

    IEnumerator OpponentChecks()
    {
        print("opponent checks");
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " checks.", 1);
        callAmt = 0;

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
        betAmt = 0;

        yield return Wait(2f, PlayerResponds());

        //PlayerResponds();

        //print("animator speed 1");
        //animator.speed = 1;
    }

    public void OpponentFolds()
    {
        print("opponent folds");
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " folds. " + player.GetComponent<PokerPlayer>().playerName + " wins the pot.", 1);

        print("player wins pot");
        player.GetComponent<PokerPlayer>().money += pot;
        pot = 0;

        
        animator.speed = 0;
        StartCoroutine(Wait(2f));

    }

    public void OpponentRaises()
    {
        print("opponent raises " + raiseAmt);
        opponent.SendMessage("Poke");
        LaunchTextbox(opponent.GetComponent<PokerPlayer>().playerName + " raises " + raiseAmt + ".", 1);
        opponent.GetComponent<PokerPlayer>().money -= raiseAmt;
        pot += raiseAmt;
        callAmt = raiseAmt;
        raiseAmt = 0;

        PlayerRespondsRaise();
    }

    void Showdown()
    {
        player.SendMessage("Poke");
        opponent.SendMessage("Poke");
        LaunchTextbox("SHOWDOWN", 2);


        opponentCards[0].GetComponent<Animator>().SetBool("revealed", true);
        opponentCards[1].GetComponent<Animator>().SetBool("revealed", true);

        // i don't know how to code combinations so i'm writing the worst code in the world instead
        // calculate player's hand value (check every possible 5 card combo and pick best score)
        uint t1 = tableCards[0].GetComponent<Card>().GetBit();
        uint t2 = tableCards[1].GetComponent<Card>().GetBit();
        uint t3 = tableCards[2].GetComponent<Card>().GetBit();
        uint t4 = tableCards[3].GetComponent<Card>().GetBit();
        uint t5 = tableCards[4].GetComponent<Card>().GetBit();

        uint p1 = playerCards[0].GetComponent<Card>().GetBit();
        uint p2 = playerCards[1].GetComponent<Card>().GetBit();

        uint o1 = opponentCards[0].GetComponent<Card>().GetBit();
        uint o2 = opponentCards[1].GetComponent<Card>().GetBit();

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

        if (playerScore > opponentScore)
        {
            print("player wins the pot");
        } else if (playerScore == opponentScore)
        {
            print("draw: player splits the pot");
        } else
        {
            print("opponent wins the pot");
        }

    }

    void LaunchTextbox(string text, int mode)
    {
        Transform parentTransform = mode == 0 ? playerTextLocation : opponentTextLocation;
        Vector3 textMovement = mode == 0 ? Vector3.down * 130f : Vector3.down * 130f;
        GameObject launchedTextbox = GameObject.Instantiate(textBox, parentTransform);
        launchedTextbox.GetComponent<PokerGameTextBox>().LaunchText(text, Vector3.zero, textMovement, 7f);
        launchedTextbox.SetActive(true);
    }



    void Update()
    {
        animator.SetInteger("turn", turn);
        potText.text = pot.ToString();
        playerMoneyText.text = player.GetComponent<PokerPlayer>().money.ToString();
        opponentMoneyText.text = opponent.GetComponent<PokerPlayer>().money.ToString();
    }
}
