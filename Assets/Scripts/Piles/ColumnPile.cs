using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnPile : Pile
{
    public override bool ValidToAddCardToPile(Card card, int pileLength = -1)
    {
        if (pileLength == 0 && card.value == Card.Value.King) return true;
        
        if (cards.Count == 0) return false;

        var lastCard = cards[^1];
        
        switch (card.suit)
        {
            case Card.Suit.Clubs when (lastCard.suit == Card.Suit.Clubs || lastCard.suit == Card.Suit.Spades):
            case Card.Suit.Spades when (lastCard.suit == Card.Suit.Clubs || lastCard.suit == Card.Suit.Spades):
            case Card.Suit.Heart when (lastCard.suit == Card.Suit.Heart || lastCard.suit == Card.Suit.Diamonds):
            case Card.Suit.Diamonds when (lastCard.suit == Card.Suit.Heart || lastCard.suit == Card.Suit.Diamonds):
                return false;
        }
        
        return ((int)card.value < (int)lastCard.value) && ((int)lastCard.value - (int)card.value == 1);
    }
}
