using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokerGameManager : MonoBehaviour
{
    // Every card in the deck in this order: A 2 3 4 5 6 7 8 9 10 J Q K; Club Diamonds Hearts Spades 
    public List<GameObject> cardList;
    public List<GameObject> deck;
    public List<Transform> tableCardPlacements;

    private Canvas canvas;

    private int currentTablePlacementIndex = 0;

    private List<GameObject> tableCards;

    //UnityEngine.Random random;

    void Start()
    {
        //random = new UnityEngine.Random();
        canvas = GetComponentInChildren<Canvas>();
        deck = new List<GameObject>();
        tableCards = new List<GameObject>();
        ShuffleDeck();
    }

    void ShuffleDeck()
    {
        deck = new List<GameObject>();
        List<int> intList = System.Linq.Enumerable.Range(0, 51).ToList<int>();
        while(intList.Count > 0)
        {
            int nextCardIndex = UnityEngine.Random.Range(0, intList.Count);
            int nextCard = intList[nextCardIndex];
            deck.Add(cardList[nextCard]);
            //print(nextCard);
            //print(intList.Count);
            intList.RemoveAt(nextCardIndex);
        }
        
    }

    void PlayNextCardToTable()
    {
        currentTablePlacementIndex %= 5;
        GameObject card = GameObject.Instantiate(deck[0], canvas.transform);
        tableCards.Add(card);
        card.transform.position = tableCardPlacements[currentTablePlacementIndex].transform.position;
        deck.RemoveAt(0);

        Animator cardAnimator = card.GetComponent<Animator>();
        cardAnimator.SetBool("revealed", true);

        currentTablePlacementIndex++;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            PlayNextCardToTable();
        }
    }
}
