using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    
    [Header("Settings")]
    public DeckController deckController;
    public List<ColumnPile> columns;
    public List<TopPile> topPiles;

    [SerializeField] private Text moveText;
    [SerializeField] private Text timeText;

    public int moves;
    public float time;

    [Header("Game UI Buttons")]
    [SerializeField] private Button autoCompleteButton;
    [SerializeField] private Button undoMovementButton;
    public int undoMovementCount;

    [Header("States")]
    public bool canClick;
    public bool winner;
    private bool _canCountTimer;

    [Header("Audios")]
    [SerializeField] private AudioClip shuffleAudio;
    [SerializeField] private AudioClip swapAudio;
    
    
    public static GameController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }


    private void Start()
    {
        autoCompleteButton.onClick.AddListener(AutoCompleteGame);
        undoMovementButton.onClick.AddListener(Undo);
        StartCoroutine(WaitToOrganizeBoard());
    }
    private void Update()
    {
        if (_canCountTimer)
        {
            time += Time.deltaTime;
            var ss = ((int)(time % 60)).ToString("00");
            var mm = (Mathf.Floor(time / 60) % 60).ToString("00");
            var hh = Mathf.Floor(time / 60 / 60).ToString("00");
            if(hh == "00")
                timeText.text = $"Time {mm}:{ss}";
            else
                timeText.text = $"Time {hh}:{mm}:{ss}";
        }
    }


    //Creates the deck and distributes the cards to piles
    private IEnumerator WaitToOrganizeBoard()
    {
        canClick = false;
        deckController.CreateDeck();
        deckController.Shuffle();
        yield return new WaitForSeconds(0.3f);
        EffectPlayer.instance.PlayOneShot(shuffleAudio);

        for (int i = 0; i < columns.Count; i++)
        {
            for (int j = 0; j < (i + 1); j++)
            {
                var card = deckController.GetLastCard();
                card.showCard = false;
                card.isClickable = false;
                columns[i].AddCardToPile(card);
                if (i != j) continue;
                card.showCard = true;
                card.isClickable = true;
            }
            columns[i].OrganizePile();
        }
        canClick = true;
        AnalyticsManager.instance.GamesPlayedOfUser();
    }

    //Move to top pile
    private bool MoveCardToTopPile(Card card)
    {
        for (int i = 0; i < topPiles.Count; i++)
        {
            if (!topPiles[i].ValidToAddCardToPile(card, topPiles[i].cards.Count)) continue;
            UndoMovement.Instance.AddMove();
            var pile = topPiles[i];
            topPiles[i].AddCardToPile(card);
            pile.OrganizePile();
            IsWonTheGame();
            return true;
        }
        return false;
    }

    //Move to column pile
    private bool MoveOneCardToColumnPile(Card card)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            if (!columns[i].ValidToAddCardToPile(card, columns[i].cards.Count)) continue;
            UndoMovement.Instance.AddMove();
            var pile = columns[i];
            columns[i].AddCardToPile(card);
            pile.OrganizePile();
            return true;
        }
        return false;
    }


    private (List<Card>, Pile) CardsToMove(Card card)
    {
        Pile pile = null;
        var cardsToMove = new List<Card>();
        for (int i = 0; i < columns.Count; i++)
        {
            if (!columns[i].ContainsCard(card)) continue;
            pile = columns[i];
            cardsToMove = columns[i].GetCardAndChildren(card);
            break;
        }

        return (cardsToMove, pile);
    }

    private void MovePileCardToAnotherColumnPile(Card card, Pile actualPile, List<Card> cardsToMove)
    {
        Pile newPile = null;
        var canMoveCard = false;
        for (int i = 0; i < columns.Count; i++)
        {
            if (columns[i] == actualPile) continue;
            
            if (!columns[i].ValidToAddCardToPile(card, columns[i].cards.Count)) continue;
            UndoMovement.Instance.AddMove();
            newPile = columns[i];
            canMoveCard = true;
            //If valid, pass all the cards to this pile
            for (int j = 0; j < cardsToMove.Count; j++)
            {
                columns[i].AddCardToPile(cardsToMove[j]);
            }
            break;
        }

        //If the move was validated, remove the cards from the current pile
        if (canMoveCard)
        {
            EffectPlayer.instance.PlayOneShot(swapAudio);
            for (int i = 0; i < cardsToMove.Count; i++)
            {
                actualPile.RemoveCardFromPile(cardsToMove[i]);
            }
            
            if (actualPile != null)
                actualPile.OrganizePile();
            if (newPile != null)
                newPile.OrganizePile();
            
            actualPile.TurnOverLastCard();
        }
        else
            card.ShakeCard();
    }


    //Checks if the player's move is possible
    public void ValidMoveCard(Card card)
    {
        var operation = CardClickedOperation(card);

        if (operation != 0)
        {
            StartStopwatch();
        }
        
        switch (operation)
        {
            //Checks if it is possible to place the card on top of the pile
            case 1 when MoveCardToTopPile(card):
                deckController.RemoveFromCardShowed(card);
                deckController.ValidCardsClickable();
                EffectPlayer.instance.PlayOneShot(swapAudio);
                break;
            //Checks if it is possible to place the card in one of the columns/piles
            case 1 when MoveOneCardToColumnPile(card):
                deckController.RemoveFromCardShowed(card);
                deckController.ValidCardsClickable();
                EffectPlayer.instance.PlayOneShot(swapAudio);
                break;
            case 1:
                card.ShakeCard();
                break;
            case 2 when MoveOneCardToColumnPile(card):
            {
                EffectPlayer.instance.PlayOneShot(swapAudio);
                //If it was valid, remove the card that was on the top pile
                for (int i = 0; i < topPiles.Count; i++)
                {
                    if (topPiles[i].ContainsCard(card))
                    {
                        topPiles[i].RemoveCardFromPile(card);
                        break;
                    }
                }

                break;
            }
            case 2:
                card.ShakeCard();
                break;
            case 3:
            {
                var tupleCardMove = CardsToMove(card);
                var cardsToMove = tupleCardMove.Item1;
                var pile = tupleCardMove.Item2;
                var moveToTopPile = false;
                
                //Checks if the clicked card has no children
                if (pile.GetCardAndChildren(card).Count == 1)
                {
                    moveToTopPile = MoveCardToTopPile(card);
                    if (moveToTopPile)
                    {
                        EffectPlayer.instance.PlayOneShot(swapAudio);
                        pile.RemoveCardFromPile(card);
                        pile.TurnOverLastCard();
                    }
                }
                
                if (cardsToMove.Count > 0 && !moveToTopPile)
                    MovePileCardToAnotherColumnPile(card, pile, cardsToMove);
                break;
            }
        }
        autoCompleteButton.gameObject.SetActive(CanDoAutoCompleteGame());
    }

    //Returns the type of move the player is making
    private int CardClickedOperation(Card card)
    {
        if (deckController.IsContainsShowedCard(card))
            return 1;

        for (int i = 0; i < topPiles.Count; i++)
            if (topPiles[i].ContainsCard(card))
                return 2;
        
        for (int i = 0; i < columns.Count; i++)
            if (columns[i].ContainsCard(card))
                return 3;

        return 0;
    }


    //Check if the game is finished
    private void IsWonTheGame()
    {
        var value = true;
        for (int i = 0; i < topPiles.Count; i++)
        {
            if (topPiles[i].cards.Count == 13) continue;
            value = false;
            break;
        }

        if (!value) return;
        _canCountTimer = false;
        winner = true;
        canClick = false;
        
        var minutes = (int)(Mathf.Floor(time / 60) % 60);
        var hours = (int)Mathf.Floor(time / 60 / 60);
        AnalyticsManager.instance.PlayTime(hours,minutes);
        AnalyticsManager.instance.Moves(moves);
        
    }

    //Check if it is possible to autocomplete the game
    public bool CanDoAutoCompleteGame()
    {
        autoCompleteButton.gameObject.SetActive(false);
        for (int i = 0; i < columns.Count; i++)
        {
            if (!columns[i].IsAllCardsShowed())
                return false;
        }
        return true;
    }

    private void AutoCompleteGame()
    {
        StartCoroutine(MakeAutoCompleteMovements());
    }

    //Animation to auto-complete the game
    private IEnumerator MakeAutoCompleteMovements()
    {
        canClick = false;
        autoCompleteButton.gameObject.SetActive(false);
        winner = true;
        _canCountTimer = false;
        //Show all cards face
        for (int i = 0; i < deckController.allCards.Count; i++)
        {
            deckController.allCards[i].showCard = true;
            deckController.allCards[i].isClickable = false;
        }

        //Remove cards that are already on top of the pile
        deckController.allCards = deckController.allCards.Except(topPiles[0].cards).ToList(); 
        deckController.allCards = deckController.allCards.Except(topPiles[1].cards).ToList();
        deckController.allCards = deckController.allCards.Except(topPiles[2].cards).ToList();
        deckController.allCards = deckController.allCards.Except(topPiles[3].cards).ToList();

        //Make cards move to top piles
        while(deckController.allCards.Count > 0)
        {
            for (int j = 0; j < deckController.allCards.Count; j++)
            {
                var card = deckController.allCards[j];
                if (!MoveCardToTopPile(card)) continue;
                
                deckController.allCards.Remove(card);
                deckController.RemoveCard(card);
                for (int z = 0; z < columns.Count; z++)
                {
                    if (!columns[z].ContainsCard(card)) continue;
                    columns[z].RemoveCardFromPile(card);
                    break;
                }
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
    
    private void Undo()
    {
        undoMovementCount++;
        UndoMovement.Instance.BackMove();
    }

    public void SetMove(int move)
    {
        moves = move;
        moveText.text = $"Moves: {moves}";
    }

    public void StartStopwatch()
    {
        _canCountTimer = true;
    }
    
    public void StopStopwatch()
    {
        _canCountTimer = false;
    }
    
    public void ResetGame()
    {
        winner = false;
        _canCountTimer = false;
        time = 0;
        timeText.text = "Time 00:00";
        SetMove(0);
    }
}
