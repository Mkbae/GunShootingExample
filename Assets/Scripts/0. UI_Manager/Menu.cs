using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	public GameObject mainMenuHolder;
	public GameObject optionsMenuHolder;
	public GameObject serverMenuHolder;
	public GameObject otherUser;

	public Slider[] volumeSliders;
	public Toggle[] resolutionToggles;
	public Toggle fullScreenToggle;
	public int[] screenWidths;

	int activeScreenResIndex;

	private void Start()
	{
		activeScreenResIndex = PlayerPrefs.GetInt("ScreenResIndex", 0);
		bool isFullScreen = PlayerPrefs.GetInt("FullScreen", 0) == 1 ? true : false;

		volumeSliders[0].value = AudioManager.Instance.masterVolumePercent;
		volumeSliders[1].value = AudioManager.Instance.musicVolumePercent;
		volumeSliders[2].value = AudioManager.Instance.sfxVolumePercent;

		for (int i = 0; i < resolutionToggles.Length; i++)
		{
			resolutionToggles[i].isOn = i == activeScreenResIndex;
		}

		for (int i = 0; i<resolutionToggles.Length; i++)
			resolutionToggles[i].interactable = !isFullScreen;
		fullScreenToggle.isOn = isFullScreen;
	}

	public void SinglePlay()
	{
		SceneManager.LoadScene(1);
	}

	public void MultiPlay()
	{
		serverMenuHolder.SetActive(true);

		if (NetworkManager.Instance != null)
		{
			NetworkManager.Instance.OnFindOtherUser += OnFindOtherUser;
			NetworkManager.Instance.OnNotFindOtherUser += OnNotFindOtherUser;
			NetworkManager.Instance.RefreshHostList();
		}
			
	}

	void OnNotFindOtherUser()
	{
		serverMenuHolder.SetActive(false);
	}

	void OnFindOtherUser()
	{
		StartCoroutine(OnFindOtherUserCoroutine());
	}

	private IEnumerator OnFindOtherUserCoroutine()
	{
		otherUser.SetActive(true);

		yield return new WaitForSeconds(2);

		SceneManager.LoadScene(2);
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void OptionMenu()
	{
		mainMenuHolder.SetActive(false);
		optionsMenuHolder.SetActive(true);
	}

	public void MainMenu()
	{	
		mainMenuHolder.SetActive(true);
		optionsMenuHolder.SetActive(false);
	}

	public void SetScreenResolution(int i)
	{
		if (i >= resolutionToggles.Length)
			return;

		if (resolutionToggles[i].isOn)
		{
			activeScreenResIndex = i;
			float aspectRatio = 16f / 9f;
			Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);
			PlayerPrefs.SetInt("ScreenResIndex", activeScreenResIndex);
			PlayerPrefs.Save();
		}
	}

	public void SetFullScreen(bool isFullScreen)
	{
		for (int i = 0; i < resolutionToggles.Length; i++)
			resolutionToggles[i].interactable = !isFullScreen;
		
		if (isFullScreen)
		{
			Resolution[] allResolutions = Screen.resolutions;
			Resolution maxResolution = allResolutions[allResolutions.Length - 1];
			Screen.SetResolution(maxResolution.width, maxResolution.height, true);
		}
		else
		{
			SetScreenResolution(activeScreenResIndex);
		}

		PlayerPrefs.SetInt("FullScreen", isFullScreen?1:0);
		PlayerPrefs.Save();
	}

	public void SetMasterVolume(float value)
	{
		AudioManager.Instance.SetVolume(value, AudioManager.AudioChannel.Master);
	}

	public void SetMusicVolume(float value)
	{
		AudioManager.Instance.SetVolume(value, AudioManager.AudioChannel.Music);
	}

	public void SetSfxVolume(float value)
	{
		AudioManager.Instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
	}
}
