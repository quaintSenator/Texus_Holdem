using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.EventSystems;

enum CardSuit
{
    Spide,
    Heart,
    Diamond,
    Club
}
class Card
{
    CardSuit cardSuit;
    int cardRant;
    Card(CardSuit cs, int cr)
    {
        cardSuit = cs;
        cardRant = cr;
    }

    Card(int cardOrderInDeck)
    {
        cardSuit = (CardSuit)(cardOrderInDeck % 13);
        cardRant = cardOrderInDeck - 13 * (int)cardSuit;
    }
}
public class deckbeh : MonoBehaviour
{
    private List<int> deckContent = Enumerable.Range(1, 52).ToList();
    void Shuffle()
    {
        //Fisher-Yates Shuffle
        System.Random random = new System.Random();
        for (int i = deckContent.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            int temp = deckContent[j];
            deckContent[j] = deckContent[i];
            deckContent[i] = temp;
        }
    }
    void debugingConsoleLogDeckContent()
    {
        Debug.Log("debugingConsoleLogDeckContent");
        StringBuilder line2Print = new StringBuilder();
        foreach (int card in deckContent)
        {
            line2Print.Append(card.ToString());
            line2Print.Append(" ");
        }
        Debug.Log(line2Print.ToString());
    }
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            Shuffle();
        }
    }

    private void OnMouseDown()
    {
        debugingConsoleLogDeckContent();
    }
}
