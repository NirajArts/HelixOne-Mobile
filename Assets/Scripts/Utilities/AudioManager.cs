using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private int soundIndex;
    private int musicIndex;

    [Header("Music & Audio")]
    public GameObject voiceOverGameObject;
    public GameObject musicGameObject;

    [Header("Menu UI Elements")]
    public GameObject soundOnIcon;
    public GameObject soundOffIcon;
    public GameObject musicOnIcon;
    public GameObject musicOffIcon;

    void Start()
    {
        soundIndex = PlayerPrefs.GetInt("Sound Index", 1);
        musicIndex = PlayerPrefs.GetInt("Music Index", 1);

        UpdateSoundState();
        UpdateMusicState();
    }

    public void ToggleMusic()
    {
        musicIndex = 1 - musicIndex; // Toggles between 0 and 1
        PlayerPrefs.SetInt("Music Index", musicIndex);
        UpdateMusicState();
    }

    public void ToggleSound()
    {
        soundIndex = 1 - soundIndex; // Toggles between 0 and 1
        PlayerPrefs.SetInt("Sound Index", soundIndex);
        UpdateSoundState();
    }

    private void UpdateMusicState()
    {
        if (musicGameObject != null)
            musicGameObject.SetActive(musicIndex == 1);

        ToggleIcons(musicOnIcon, musicOffIcon, musicIndex == 1);
    }

    private void UpdateSoundState()
    {
        if (voiceOverGameObject != null)
            voiceOverGameObject.SetActive(soundIndex == 1);

        ToggleIcons(soundOnIcon, soundOffIcon, soundIndex == 1);
    }

    private void ToggleIcons(GameObject onIcon, GameObject offIcon, bool isOn)
    {
        if (onIcon != null)
            onIcon.SetActive(isOn);

        if (offIcon != null)
            offIcon.SetActive(!isOn);
    }
}
