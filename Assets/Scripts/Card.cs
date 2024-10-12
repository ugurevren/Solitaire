using UnityEngine;
using DG.Tweening;

public class Card : MonoBehaviour
{
    public enum Suit 
    {
        Diamonds = 1,
        Spades,
        Heart,
        Clubs
    }
    public Suit suit;

    public enum Value
    {
        Ace=1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Nothing = 0
    }
    public Value value;

    private SpriteRenderer spriteRenderer;
    public Sprite frontCard;
    public Sprite backCard;

    public bool showCard;
    public bool hideCard;
    public bool isClickable;

    private void Start()
    {
        gameObject.name = $"{value}_{suit}";
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        CardShowState(showCard);
        if (!showCard) isClickable = false;
    }

    private void CardShowState(bool state)
    {
        if (hideCard)
        {
            spriteRenderer.sprite = backCard;
            return;
        }
        spriteRenderer.sprite = state ? frontCard : backCard;
    }
    
    public void ShakeCard()
    {
        GameController.instance.canClick = false;
        transform.DOShakePosition(0.4f, new Vector3(0.3f,0f,0f), randomness:1).OnComplete(()=> {
            GameController.instance.canClick = true;
        });
    }
    
    public void RotateAndShowCard()
    {
        transform.DORotate(new Vector3(0,90,0), 0.2f, RotateMode.FastBeyond360).SetLoops(2, LoopType.Yoyo);
        showCard = true;
    }


    public void OnMouseDown()
    {
        if (!isClickable || GameController.instance.winner || !GameController.instance.canClick || !showCard )
            return;
        
        GameController.instance.ValidMoveCard(this);
    }

}
