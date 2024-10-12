using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class Pile : MonoBehaviour
{
    public List<Card> cards;

    public Vector3 offset;

    public virtual bool ValidToAddCardToPile(Card card, int pileLength = -1)
    {
        return false;
    }

    public void AddCardToPile(Card card, bool organizePile = false)
    {
        card.transform.SetParent(transform);
        cards.Add(card);
        if (organizePile)
            OrganizePile();
    }

    public void RemoveCardFromPile(Card card, bool organizePile = false)
    {
        cards.Remove(card);
        if (organizePile)
            OrganizePile();
    }

    public bool ContainsCard(Card card)
    {
        return cards.Contains(card);
    }
    
    public bool IsAllCardsShowed()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (!cards[i].showCard)
                return false;
        }
        return true;
    }
    
    public List<Card> GetCardAndChildren(Card card)
    {
        var cardAndChildren = new List<Card>();
        int parentIndex = cards.IndexOf(card);
        for (int i = parentIndex; i < cards.Count; i++)
        {
            cardAndChildren.Add(cards[i]);
        }
        return cardAndChildren;
    }
    
    public void TurnOverLastCard()
    {
        if (cards.Count <= 0 || cards[^1].showCard) return;
        
        cards[^1].isClickable = true;
        cards[^1].RotateAndShowCard();
    }
    
    public List<bool> CardsFace()
    {
        List<bool> cardsFace = new List<bool>();
        for (int i = 0; i < cards.Count; i++)
        {
            cardsFace.Add(cards[i].showCard);
        }
        return cardsFace;
    }
    
    public List<bool> CardsClickable()
    {
        var cardsClickable = new List<bool>();
        for (int i = 0; i < cards.Count; i++)
        {
            cardsClickable.Add(cards[i].isClickable);
        }
        return cardsClickable;
    }

    public void OrganizePile()
    {
        GameController.instance.canClick = false;
        var sequence = DOTween.Sequence();
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetParent(transform);
            if (i == 0)
                sequence.Append(cards[i].transform.DOMove(transform.position, 0.2f));

            else
            {
                var newPos = new Vector3(
                    transform.position.x + (i * offset.x),
                    transform.position.y + (i * offset.y),
                    transform.position.z + (i * offset.z));

                sequence.Join(cards[i].transform.DOMove(newPos, 0.2f));
            }
            sequence.OnComplete(() =>
            {
                GameController.instance.canClick = true;
            });
        }
    }
}

