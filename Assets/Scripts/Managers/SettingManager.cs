// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Michsky.UI.Shift;
using UnityEngine.Audio;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;

public class SettingManager : MonoBehaviour
{
    // NOTE - Make these Serialized private. They are not used anywhere else
    public SwitchManager vsyncButton, fullscreenButton, fpsButton;

    // NOTE - Make this Serialized private. They are not used anywhere else
    public GameObject fpsBox;

    private Resolution[] res;
    private List<Resolution> filteredResolutions;

    // NOTE - Make this Serialized private. They are not used anywhere else
    public TMP_Dropdown resolutionDropdown;
    private float currentRefreshRate;
    private int currentResolutionIndex = 0;

    // NOTE - In my opinion, put these above at the top
    [SerializeField] private AudioMixer globalMixer;
    [SerializeField] private Slider bgmSlider, sfxSlider, vlSlider;

    private float bgmBefore, sfxBefore, vlBefore;
    private bool fullscreenBefore, fpsBefore, vsyncBefore;

    // NOTE - Make this Serialized private. They are not used anywhere else,
    // last bool is not used
    public bool appliedSettings = false, localFullscreenBool, localFPSBool;

    // NOTE - Missing access specifier
    void Awake()
    {
        // NOTE - Use a ternary operator. Actually, just use GetFloat second
        // parameter, which is the default value it returns if not found
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume");
        }
        else
        {
            bgmSlider.value = 1;
        }

        // NOTE - Use a ternary operator. Actually, just use GetFloat second
        // parameter, which is the default value it returns if not found
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        }
        else
        {
            sfxSlider.value = 1;
        }

        // NOTE - Use a ternary operator. Actually, just use GetFloat second
        // parameter, which is the default value it returns if not found
        if (PlayerPrefs.HasKey("VLVolume"))
        {
            vlSlider.value = PlayerPrefs.GetFloat("VLVolume");
        }
        else
        {
            vlSlider.value = 1;
        }

        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            string localFullscreen = PlayerPrefs.GetString("Fullscreen");

            // NOTE - Use bool.Parse 
            if (localFullscreen.ToLower() == "true")
            {
                localFullscreenBool = true;
            }
            else if (localFullscreen.ToLower() == "false")
            {
                localFullscreenBool = false;
            }

            // NOTE - Use a ternary operator
            if (localFullscreenBool)
            {
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }

            fullscreenButton.isOn = localFullscreenBool;

            Debug.Log("Loaded PlayerPrefs (FullscreenBool) : " + localFullscreenBool);
        }
        else
        {
            Screen.fullScreen = true;
            fullscreenButton.isOn = true;
        }

        // Retrieve the saved state of the fpsButton from PlayerPrefs
        bool savedFPSState = PlayerPrefs.GetInt("FPSButtonState", 0) == 1;
        fpsButton.isOn = savedFPSState;

        // Show or hide FPS based on the retrieved state
        if (!savedFPSState)
        {
            HideFPS();
        }
        else
        {
            ShowFPS();
        }
    }

    // NOTE - Missing access specifier
    void Start()
    {
        // NOTE - Maybe make a function for this mathametical function in this scope
        globalMixer.SetFloat("BGMVolume", Mathf.Log10(bgmSlider.value) * 20);
        globalMixer.SetFloat("SFXVolume", Mathf.Log10(sfxSlider.value) * 20);
        globalMixer.SetFloat("VLVolume", Mathf.Log10(vlSlider.value) * 20);

        GrabScreenResolution();

        // NOTE - Just pass in the condition 
        if (QualitySettings.vSyncCount == 0)
        {
            vsyncButton.isOn = false;
        }
        else
        {
            vsyncButton.isOn = true;
        }
    }

    public void ResetAppliedSettingsBool()
    {
        appliedSettings = false;
    }

    public void SetInitialVolumes()
    {
        bgmBefore = bgmSlider.value;
        sfxBefore = sfxSlider.value;
        vlBefore = vlSlider.value;
    }

    public void SetInitialBooleans()
    {
        fullscreenBefore = fullscreenButton.isOn;
        fpsBefore = fpsButton.isOn;
        vsyncBefore = vsyncButton.isOn;
    }

    public void ResetVolumes()
    {
        if (!appliedSettings)
        {
            bgmSlider.value = bgmBefore;
            sfxSlider.value = sfxBefore;
            vlSlider.value = vlBefore;

            // NOTE - Maybe make a function for this mathametical function in
            // this class 
            globalMixer.SetFloat("BGMVolume", Mathf.Log10(bgmSlider.value) * 20);
            globalMixer.SetFloat("SFXVolume", Mathf.Log10(sfxSlider.value) * 20);
            globalMixer.SetFloat("VLVolume", Mathf.Log10(vlSlider.value) * 20);
        }
    }

    public void ResetBooleans()
    {
        // NOTE - Unnest this and do 
        // if (appliedSettings) 
        // {
        //     return;
        // }
        if (!appliedSettings)
        {
            fullscreenButton.isOn = fullscreenBefore;
            fpsButton.isOn = fpsBefore;
            vsyncButton.isOn = vsyncBefore;

            // NOTE - Use a ternary operator
            if (fullscreenButton.isOn)
            {
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }

            // NOTE - Have one function that takes in a boolean, and take in
            // this condition, wait, you do have that, just use SetFPS
            if (fpsButton.isOn)
            {
                ShowFPS();
            }
            else
            {
                HideFPS();
            }

            // NOTE - Use a ternary operator
            if (vsyncButton.isOn)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }
        }
    }

    // NOTE - Again, just have one function that does this mathematical function
    // for these three functions
    public void SetBGMVolume(float volume)
    {
        globalMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        globalMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    public void SetVLVolume(float volume)
    {
        globalMixer.SetFloat("VLVolume", Mathf.Log10(volume) * 20);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        // NOTE - Use a ternary operator
        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    public void SetVsync(bool isVsync)
    {
        // NOTE - Use a ternary operator
        if (isVsync)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }
    }

    // NOTE - Use this in the above function!
    public void SetFPS(bool isFPS)
    {
        if (isFPS)
        {
            ShowFPS();
        }
        else
        {
            HideFPS();
        }
    }

    private void GrabScreenResolution()
    {
        res = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();
        currentRefreshRate = Screen.currentResolution.refreshRate;

        // NOTE - Use a foreach loop.
        for (int i = 0; i < res.Length; i++)
        {
            if (res[i].refreshRate == currentRefreshRate)
            {
                filteredResolutions.Add(res[i]);
            }
        }

        List<string> options = new List<string>();
        // NOTE - Use a foreach loop.
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            // NOTE - Use string.Format
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height + " " + filteredResolutions[i].refreshRate + " Hz";
            options.Add(resolutionOption);

            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resIndex)
    {
        Resolution resolution = filteredResolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, fullscreenButton.isOn);
    }

    public void ApplySettings()
    {
        appliedSettings = true;

        SetInitialVolumes();
        SetInitialBooleans();

        PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetFloat("VLVolume", vlSlider.value);

        // NOTE - You use ternary here, but not up there? Be consistent 
        PlayerPrefs.SetString("Fullscreen", fullscreenButton.isOn ? "true" : "false");
        PlayerPrefs.SetInt("VSync", vsyncButton.isOn ? 1 : 0);
        PlayerPrefs.SetInt("FPSButtonState", fpsButton.isOn ? 1 : 0);
        
        // Set the resolution based on the selected index of the dropdown
        SetResolution(resolutionDropdown.value);

        PlayerPrefs.Save();

        appliedSettings = false;
    }

    private void HideFPS()
    {
        fpsBox.SetActive(false);
    }

    private void ShowFPS()
    {
        fpsBox.SetActive(true);
    }
}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}
