using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

public class DeckController : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;
    [SerializeField] private List<Sprite> cardsFront;
    
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform cardShowedPosition; 
    [SerializeField] private AudioClip swapAudio;
    
    public List<Card> cardsInDeck;
    public List<Card> showedCards;
    public List<Card> allCards; 
    
    public void CreateDeck()
    {
        var pos = transform.position;
        var cardOffset = new Vector3(pos.x, pos.y, pos.z + 0.1f);
        var i = 0;
        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            foreach (Card.Value value in System.Enum.GetValues(typeof(Card.Value)))
            {
                if (value == Card.Value.Nothing)
                    continue;
                var card = Instantiate(cardPrefab.gameObject, cardOffset, Quaternion.identity).GetComponent<Card>();
                card.suit = suit;
                card.value = value;
                card.frontCard = cardsFront[i];
                i++;
                card.showCard = false;
                card.isClickable = false;
                AddCard(card);
                allCards.Add(card);
                cardOffset.y += offset.y;
                cardOffset.x += offset.x;
                cardOffset.z += offset.z;
            }
        }

        allCards = allCards.OrderBy(x => x.value).ToList();
    }

    public void Shuffle()
    {
        // Fisher-Yates Method
        for (var i = cardsInDeck.Count - 1; i > 0; i--)
        {
            var randomCardIndex = Random.Range(0, i + 1);
            
            var tempCard = cardsInDeck[i];
            cardsInDeck[i] = cardsInDeck[randomCardIndex];
            cardsInDeck[randomCardIndex] = tempCard;
        }
        OrganizeDeck();
    }
    
    public void RemoveCard(Card card)
    {
        if (!cardsInDeck.Contains(card)) return;
        cardsInDeck.Remove(card);
    }

    private void AddCard(Card card)
    {
        cardsInDeck.Add(card);
    }

    public Card GetLastCard()
    {
        var card = cardsInDeck.Last();
        cardsInDeck.Remove(card);
        return card;
    }

    public bool IsContainsShowedCard(Card card)
    {
        return showedCards.Contains(card);
    }

    public void RemoveFromCardShowed(Card card)
    {
        showedCards.Remove(card);
        if (showedCards.Count > 0)
            showedCards[showedCards.Count - 1].isClickable = true;
    }
    
    private void OrganizeDeck()
    {
        var cardOffset = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.1f);

        foreach (var card in cardsInDeck)
        {
            card.gameObject.SetActive(true);
            card.showCard = false;
            card.isClickable = false;
            card.transform.DOMove(cardOffset, 0.1f);
            cardOffset.x += offset.x;
            cardOffset.y += offset.y;
            cardOffset.z += offset.z;
        }
    }
    
    public void OrganizeShowedCard()
    {
        if (cardsInDeck.Count > 0)
        {
            var lastCard = cardsInDeck[^1];
            var transform1 = lastCard.transform;
            var position = transform1.position;
            position = new Vector3(position.x, position.y, transform.position.z - 0.01f);
            transform1.position = position;
        }
        OrganizeDeck();
        for (int i = showedCards.Count - 1; i >= 0; i--)
        {
            showedCards[i].transform.DOMove(new Vector3(cardShowedPosition.position.x, cardShowedPosition.position.y, cardShowedPosition.position.z + (i * -0.01f)), 0.2f);
        }
    }
    
    public void ValidCardsClickable()
    {
        for (int i = 0; i < showedCards.Count; i++)
        {
            showedCards[i].isClickable = false;
            if (i == showedCards.Count - 1) showedCards[i].isClickable = true;
        }
    }
    
    public void OnMouseDown()
    {
        if (!GameController.instance.canClick || GameController.instance.winner)
            return;

        EffectPlayer.instance.PlayOneShot(swapAudio);
        GameController.instance.StartStopwatch();
        GameController.instance.canClick = false;
        UndoMovement.Instance.AddMove();
        
        if (cardsInDeck.Count > 0)
        {
            var card = GetLastCard();

            Vector3 posClicked;
            if (showedCards.Count == 0)
                posClicked = cardShowedPosition.position;
            else
                posClicked = showedCards[showedCards.Count - 1].transform.position;
            posClicked.z -= 0.1f;

            var sequence = DOTween.Sequence();
            sequence.Append(card.transform.DOMove(posClicked, 0.1f));
            sequence.AppendInterval(0.05f);
            sequence.OnComplete(() =>
            {
                for (var i = showedCards.Count - 1; i >= 0; i--)
                {
                    showedCards[i].transform.position = new Vector3(cardShowedPosition.position.x, cardShowedPosition.position.y, cardShowedPosition.position.z + (i * -0.01f));
                    showedCards[i].isClickable = false;
                }
                card.isClickable = true;
                card.showCard = true;
                GameController.instance.canClick = true;
            });
            showedCards.Add(card);
        }
        else
        {
            for (int i = showedCards.Count - 1; i >= 0; i--)
            {
                showedCards[i].showCard = false;
                showedCards[i].isClickable = false;
                AddCard(showedCards[i]);
            }
            showedCards.Clear();
            OrganizeDeck();
            GameController.instance.canClick = true;
        }
        
    }
    
    public List<bool> CardsFace()
    {
        return cardsInDeck.Select(t => t.showCard).ToList();
    }
    
    public List<bool> CardsClickable()
    {
        return cardsInDeck.Select(t => t.isClickable).ToList();
    }
    
    public List<bool> ClickedCardsFace()
    {
        return showedCards.Select(t => t.showCard).ToList();
    }
    
    public List<bool> ClickedCardsClickable()
    {
        return showedCards.Select(t => t.isClickable).ToList();
    }
}

