using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopPile : Pile
{
    public override bool ValidToAddCardToPile(Card card, int pileLength = -1)
    {
       
        if (pileLength == 0 && card.value == Card.Value.Ace) return true;
        if (cards.Count == 0) return false;
        
        var lastCard = cards[^1];
        
        if (card.suit != lastCard.suit) return false;
        
        return ((int)card.value > (int)lastCard.value) && ((int)card.value - (int)lastCard.value == 1);
    }
}
