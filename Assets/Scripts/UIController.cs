using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public enum UIPanel { 
        Game,
        Menu,
        Settings
    }

    public UIPanel uiPanel;

    public GameObject gamePanel;
    public GameObject playPanel;
    public GameObject settingPanel;

    public Text effectSoundText;

    private void UIPanelState(UIPanel UIPanel)
    {
        uiPanel = UIPanel;
        playPanel.SetActive(false);
        settingPanel.SetActive(false);
        GameController.instance.canClick = false;
        
        if(UIPanel == UIPanel.Game)
        {
            gamePanel.SetActive(true);
            GameController.instance.canClick = true;
            GameController.instance.StartStopwatch();
        }
        else if(UIPanel == UIPanel.Menu)
        {
            playPanel.SetActive(true);
            GameController.instance.StopStopwatch();
        }
        else if(UIPanel == UIPanel.Settings)
        {
            UpdateSettingText();
            settingPanel.SetActive(true);
            GameController.instance.StopStopwatch();
        }
    }

    private void UpdateSettingText()
    {
        var effectSound = Settings.Instance.intToBool(Settings.Instance.effectSoundActive);

        if (effectSound) effectSoundText.text = "Effect Sound: On";
        else effectSoundText.text = "Effect Sound: Off";

    }
    
    private void QuitGame()
    {
        Application.Quit();
    }

    public void OnClickPlayPanelButton()
    {
        UIPanelState(UIPanel.Menu);
    }

    public void OnClickSettingPanelButton()
    {
        UIPanelState(UIPanel.Settings);
    }

    public void OnClickContinueButton()
    {
        UIPanelState(UIPanel.Game);
    }

    public void OnClickRepeatButton()
    {
        AnalyticsManager.instance.Moves(GameController.instance.moves);
        var time = GameController.instance.time;
        var minutes = (int)(Mathf.Floor(time / 60) % 60);
        var hours = (int)Mathf.Floor(time / 60 / 60);
        AnalyticsManager.instance.PlayTime(hours, minutes);
        AnalyticsManager.instance.RepeatGame();
        AnalyticsManager.instance.UndoMoves(GameController.instance.undoMovementCount);
        UndoMovement.Instance.BackToFirstMove();
        GameController.instance.ResetGame();
        UIPanelState(UIPanel.Game);
    }

    public void OnClickNewGameButton()
    {
        AnalyticsManager.instance.Moves(GameController.instance.moves);
        var time = GameController.instance.time;
        var minutes = (int)(Mathf.Floor(time / 60) % 60);
        var hours = (int)Mathf.Floor(time / 60 / 60);
        AnalyticsManager.instance.PlayTime(hours, minutes);
        AnalyticsManager.instance.UndoMoves(GameController.instance.undoMovementCount);
        SceneManager.LoadScene(0);
        
    }

    public void OnClickExitButton()
    {
        AnalyticsManager.instance.Moves(GameController.instance.moves);
        var time = GameController.instance.time;
        var minutes = (int)(Mathf.Floor(time / 60) % 60);
        var hours = (int)Mathf.Floor(time / 60 / 60);
        AnalyticsManager.instance.PlayTime(hours, minutes);
        AnalyticsManager.instance.UndoMoves(GameController.instance.undoMovementCount);
        AnalyticsManager.instance.Flush();
        Invoke(nameof(QuitGame), AnalyticsManager.instance.quitDelay);
    }

    public void OnClickEffectSoundButton()
    {
        Settings.Instance.ChangeEffectSound();
        UpdateSettingText();
    }
    public void OnClickBackButton()
    {
        UIPanelState(UIPanel.Game);
    }
}
