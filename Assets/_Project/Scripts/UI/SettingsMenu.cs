using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenu : Menu
{
    private Slider m_GlobalAudioSlider;
    private Slider m_MusicAudioSlider;
    private Slider m_SFXAudioSlider;
    private Button m_BackButton;

    private void GetComponents()
    {
        m_GlobalAudioSlider = m_RootVisualElement.Q<Slider>("GlobalAudio");
        m_MusicAudioSlider = m_RootVisualElement.Q<Slider>("MusicAudio");
        m_SFXAudioSlider = m_RootVisualElement.Q<Slider>("SFXAudio");
        m_BackButton = GetButtonByName("Back");
    }

    private bool ValidateComponents()
    {
        bool allComponentsValid = true;

        if (m_GlobalAudioSlider == null)
        {
            Debug.LogError("Global audio slider is missing in the SettingsMenu.", this);
            allComponentsValid = false;
        }

        if (m_MusicAudioSlider == null)
        {
            Debug.LogError("Music audio slider is missing in the SettingsMenu.", this);
            allComponentsValid = false;
        }

        if (m_SFXAudioSlider == null)
        {
            Debug.LogError("SFX audio slider is missing in the SettingsMenu.", this);
            allComponentsValid = false;
        }

        if (m_BackButton == null)
        {
            Debug.LogError("Back button is missing in the SettingsMenu.", this);
            allComponentsValid = false;
        }

        return allComponentsValid;
    }

    private void LinkEvents()
    {
        m_GlobalAudioSlider.RegisterValueChangedCallback(OnGlobalAudioChanged);
        m_MusicAudioSlider.RegisterValueChangedCallback(OnMusicAudioChanged);
        m_SFXAudioSlider.RegisterValueChangedCallback(OnSFXAudioChanged);
        m_BackButton.clicked += OnBackButtonClicked;
    }

    private void UnlinkEvents()
    {
        m_GlobalAudioSlider.UnregisterValueChangedCallback(OnGlobalAudioChanged);
        m_MusicAudioSlider.UnregisterValueChangedCallback(OnMusicAudioChanged);
        m_SFXAudioSlider.UnregisterValueChangedCallback(OnSFXAudioChanged);
        m_BackButton.clicked -= OnBackButtonClicked;
    }

    private void OnGlobalAudioChanged(ChangeEvent<float> evt)
    {
        SoundManager.SetMasterVolume(evt.newValue);
    }

    private void OnMusicAudioChanged(ChangeEvent<float> evt)
    {
        SoundManager.SetMusicVolume(evt.newValue);
    }

    private void OnSFXAudioChanged(ChangeEvent<float> evt)
    {
        SoundManager.SetSFXVolume(evt.newValue);
    }

    private void OnBackButtonClicked()
    {
        MenuManager.Instance.ShowPreviousMenu();
    }

    protected override void Awake()
    {
        base.Awake();
        GetComponents();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GetComponents();
        if (!ValidateComponents()) { return; }

        LinkEvents();
        m_GlobalAudioSlider.SetValueWithoutNotify(SoundManager.MasterVolume);
        m_MusicAudioSlider.SetValueWithoutNotify(SoundManager.MusicVolume);
        m_SFXAudioSlider.SetValueWithoutNotify(SoundManager.SFXVolume);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (ValidateComponents())
        {
            UnlinkEvents();
        }
    }
}
