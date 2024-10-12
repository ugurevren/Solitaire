
using System;
using System.Collections;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsManager : MonoBehaviour
{
    private bool _isInitialized=false;
    [Tooltip("Time to wait before flushing analytics data when the application quits.")]
    public float quitDelay = 3f;
    public static AnalyticsManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
        _isInitialized = true;
    }
    
    public void GamesPlayedOfUser()
    {
        if (!_isInitialized) return;
        
        var gamesPlayed = PlayerPrefs.GetInt("games_played_of_user", 0);
        gamesPlayed++;
        PlayerPrefs.SetInt("games_played", gamesPlayed);

        var userID = AnalyticsSessionInfo.userId;
        
        var myEvent = new CustomEvent("games_played_of_user")
        {
            { "games_played_of_user", gamesPlayed },
            { "user_id", userID }
        };
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }
    
    public void Moves(int moves)
    {
        if (!_isInitialized) return;
        var gamesPlayed = PlayerPrefs.GetInt("games_played", 0);
        var movesPerGame = (float)moves / gamesPlayed;
        
        
        var myEvent1 = new CustomEvent("moves_per_game")
        {
            { "moves_per_game", movesPerGame },
        };
        
        AnalyticsService.Instance.RecordEvent(myEvent1);
        AnalyticsService.Instance.Flush();
        
        var myEvent2 = new CustomEvent("moves")
        {
            { "moves", moves },
        };
        
        AnalyticsService.Instance.RecordEvent(myEvent2);
    }

    public void UndoMoves(int undoMoves)
    {
        if (!_isInitialized) return;
        var myEvent = new CustomEvent("undo_moves")
        {
            { "undo_moves", undoMoves },
        };
        
        AnalyticsService.Instance.RecordEvent(myEvent);
    }
    
    public void PlayTime(int hours,int minutes)
    {
        if (!_isInitialized) return;
        var myEvent = new CustomEvent("complete_time")
        {
            { "hours", hours },
            { "minutes", minutes }
        };
        
        AnalyticsService.Instance.RecordEvent(myEvent);
    }
    
    public void RepeatGame()
    {
        if (!_isInitialized) return;
        var myEvent = new CustomEvent("repeat_game");
        
        AnalyticsService.Instance.RecordEvent(myEvent);
    }
    public void Flush()
    {
        if (!_isInitialized) return;
        AnalyticsService.Instance.Flush();
    }

    private void OnApplicationQuit()
    {
        StartCoroutine(WaitAndFlush());
    }
    private IEnumerator WaitAndFlush()
    {
        yield return new WaitForSeconds(quitDelay);
        Flush();
    }
}
