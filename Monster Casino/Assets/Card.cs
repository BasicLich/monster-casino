using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public string suit;
    public string val;
    public ulong bit;
    
    public ulong GetBit()
    {
        print("card evaluation please");
        switch (suit)
        {
            case "s":
                bit = 0x0000000000001;
                break;
            case "h":
                bit = 0x0000000000002;
                break;
            case "d":
                bit = 0x0000000000004;
                break;
            case "c":
                bit = 0x0000000000008;
                break;

        }

        switch (val)
        {
            case "2":
                //bit = bit << (4 * 0);
                break;
            case "3":
                bit = bit << (4 * 1);
                break;
            case "4":
                bit = bit << (4 * 2);
                break;
            case "5":
                bit = bit << (4 * 3);
                break;
            case "6":
                bit = bit << (4 * 4);
                break;
            case "7":
                bit = bit << (4 * 5);
                break;
            case "8":
                bit = bit << (4 * 6);
                break;
            case "9":
                bit = bit << (4 * 7);
                break;
            case "10":
                bit = bit << (4 * 8);
                break;
            case "J":
                bit = bit << (4 * 9);
                break;
            case "Q":
                bit = bit << (4 * 10);
                break;
            case "K":
                bit = bit << (4 * 11);
                break;
            case "A":
                bit = bit << (4 * 12);
                break;
        }

        return bit;
    }
    void Start()
    {
        
        
    }

    void Update()
    {
        
    }
}
