using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
	public AudioClip mainTheme;
	public AudioClip menuTheme;

	private int sceneIndex;

	void OnEnable()
	{
		//Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable()
	{
		//Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}
	
	private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if (sceneIndex != scene.buildIndex)
		{
			sceneIndex = scene.buildIndex;
			AudioManager.Instance.PlayMusic(sceneIndex==0? menuTheme:mainTheme, 2);
		}
	}
}
