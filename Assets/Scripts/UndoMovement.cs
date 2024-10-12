using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UndoMovement : MonoBehaviour
{
    public static UndoMovement Instance;
    public List<Move> undoMovements;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void AddMove()
    {
        if (GameController.instance.winner)
            return;

        var move = new Move();
        
        var deckData = new DeckData();
        deckData.deckCards = new List<Card>(GameController.instance.deckController.cardsInDeck);
        deckData.deckCardsFaceState = new List<bool>(GameController.instance.deckController.CardsFace());
        deckData.deckCardsClickableState = new List<bool>(GameController.instance.deckController.CardsClickable());

        deckData.deckCardsClicked = new List<Card>(GameController.instance.deckController.showedCards);
        deckData.deckCardsClickedFaceState = new List<bool>(GameController.instance.deckController.ClickedCardsFace());
        deckData.deckCardsClickedClicklabeState = new List<bool>(GameController.instance.deckController.ClickedCardsClickable());
        move.deckData = deckData;

        for (int i = 0; i < GameController.instance.columns.Count; i++)
        {
            var pileData = new PileData();
            pileData.pileCards = new List<Card>(GameController.instance.columns[i].cards);
            pileData.pileCardsFaceState = new List<bool>(GameController.instance.columns[i].CardsFace());
            pileData.pileCardsClickableState = new List<bool>(GameController.instance.columns[i].CardsClickable());
            move.pilesData.Add(pileData);
        }

        //Add top piles card movement
        for (int i = 0; i < GameController.instance.topPiles.Count; i++)
        {
            var pileData = new PileData();
            pileData.pileCards = new List<Card>(GameController.instance.topPiles[i].cards);
            pileData.pileCardsFaceState = new List<bool>(GameController.instance.topPiles[i].CardsFace());
            pileData.pileCardsClickableState = new List<bool>(GameController.instance.topPiles[i].CardsClickable());
            move.topPilesData.Add(pileData);
        }
        undoMovements.Add(move);

        GameController.instance.SetMove(undoMovements.Count);
    }


    public void BackMove()
    {
        if (undoMovements.Count <= 0) return;
        var move = undoMovements[undoMovements.Count - 1];

        //Remake deck cards
        GameController.instance.deckController.cardsInDeck = move.deckData.deckCards;
        for (int i = 0; i < GameController.instance.deckController.cardsInDeck.Count; i++)
        {
            GameController.instance.deckController.cardsInDeck[i].showCard = move.deckData.deckCardsFaceState[i];
            GameController.instance.deckController.cardsInDeck[i].isClickable = move.deckData.deckCardsClickableState[i];
        }

        //Remake showed cards from deck
        GameController.instance.deckController.showedCards = move.deckData.deckCardsClicked;
        for (int i = 0; i < GameController.instance.deckController.showedCards.Count; i++)
        {
            GameController.instance.deckController.showedCards[i].showCard = move.deckData.deckCardsClickedFaceState[i];
            GameController.instance.deckController.showedCards[i].isClickable = move.deckData.deckCardsClickedClicklabeState[i];
        }

        //Remake columns/piles
        for (int i = 0; i < GameController.instance.columns.Count; i++)
        {
            GameController.instance.columns[i].cards = move.pilesData[i].pileCards;
            for (int j = 0; j < GameController.instance.columns[i].cards.Count; j++)
            {
                GameController.instance.columns[i].cards[j].showCard = move.pilesData[i].pileCardsFaceState[j];
                GameController.instance.columns[i].cards[j].isClickable = move.pilesData[i].pileCardsClickableState[j];
            }
        }

        //Remake top piles
        for (int i = 0; i < GameController.instance.topPiles.Count; i++)
        {
            GameController.instance.topPiles[i].cards = move.topPilesData[i].pileCards;
            for (int j = 0; j < GameController.instance.topPiles[i].cards.Count; j++)
            {
                GameController.instance.topPiles[i].cards[j].showCard = move.topPilesData[i].pileCardsFaceState[j];
                GameController.instance.topPiles[i].cards[j].isClickable = move.topPilesData[i].pileCardsClickableState[j];
            }
        }

        //Movement cards to original position
        GameController.instance.deckController.OrganizeShowedCard();
        for (int i = 0; i < GameController.instance.columns.Count; i++)
            GameController.instance.columns[i].OrganizePile();

        for (int i = 0; i < GameController.instance.topPiles.Count; i++)
            GameController.instance.topPiles[i].OrganizePile();

        undoMovements.Remove(move);
        GameController.instance.CanDoAutoCompleteGame();
        GameController.instance.SetMove(undoMovements.Count);
    }

    //Back to the first movement, reset the game
    public void BackToFirstMove()
    {
        if (undoMovements.Count > 1)
            undoMovements.RemoveRange(1, undoMovements.Count - 1);

        BackMove();
    }
}



[System.Serializable]
public class Move
{
    public DeckData deckData;
    public List<PileData> pilesData = new List<PileData>();
    public List<PileData> topPilesData = new List<PileData>();
}


[System.Serializable]
public class DeckData
{
    public List<Card> deckCards = new List<Card>();
    public List<bool> deckCardsFaceState = new List<bool>();
    public List<bool> deckCardsClickableState = new List<bool>();

    public List<Card> deckCardsClicked = new List<Card>();
    public List<bool> deckCardsClickedFaceState = new List<bool>();
    public List<bool> deckCardsClickedClicklabeState = new List<bool>();
}



[System.Serializable]
public class PileData
{
    public List<Card> pileCards = new List<Card>();
    public List<bool> pileCardsFaceState = new List<bool>();
    public List<bool> pileCardsClickableState = new List<bool>();
}