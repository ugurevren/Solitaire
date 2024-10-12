using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings Instance;

    public int effectSoundActive;
    public int threeModeTurnActive;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        effectSoundActive = (int)PlayerPrefs.GetFloat("sound_configuration", 1);
        threeModeTurnActive = PlayerPrefs.GetInt("three_mode", -1);
    }

    public void ChangeEffectSound()
    {
        effectSoundActive = effectSoundActive * -1;
        PlayerPrefs.SetFloat("sound_configuration", effectSoundActive);
    }

    public bool intToBool(int Number)
    {
        return (Number == -1 ? false : true);
    }
}
