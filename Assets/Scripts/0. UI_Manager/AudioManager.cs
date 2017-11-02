using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public enum AudioChannel : int
	{
		Master,
		Sfx,
		Music
	};

	public float masterVolumePercent { get; private set; }
	public float sfxVolumePercent { get; private set; }
	public float musicVolumePercent { get; private set; }

	AudioSource sfx2DSource;
	AudioSource[] musicSources;
	int activeMusicSourceIndex;

	private static AudioManager _instance;
	public static AudioManager Instance
	{
		get
		{
			if (_instance == null)
				_instance = FindObjectOfType(typeof(AudioManager)) as AudioManager;
			return _instance;
		}	
	}

	Transform audioListener;

	SoundLibrary library;

	private void Awake()
	{
		AudioManager[] manager = FindObjectsOfType(typeof(AudioManager)) as AudioManager[];
		if (manager.Length >= 2)
		{
			for (int i = 0; i<manager.Length; i++)
			if (manager[i] == this)
				Destroy(manager [i].gameObject);
		}
        DontDestroyOnLoad(gameObject);



		library = GetComponent<SoundLibrary>();

		musicSources = new AudioSource[2];
		for (int i = 0; i<musicSources.Length; i++)
		{
			GameObject newMusicSource = new GameObject("Music source" + (i + 1));
			musicSources[i] = newMusicSource.AddComponent<AudioSource>();
			musicSources[i].loop = true;
			newMusicSource.transform.parent = transform;
		}

		GameObject newSfx2DSource = new GameObject("2D sfx source");
		sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
		newSfx2DSource.transform.parent = transform;

		audioListener = FindObjectOfType<AudioListener>().transform;

		masterVolumePercent = PlayerPrefs.GetFloat("MasterVolume", 1);
		sfxVolumePercent = PlayerPrefs.GetFloat("SfxVolume", 1);
		musicVolumePercent = PlayerPrefs.GetFloat("MusicVolume", 1);
	}

	private void FixedUpdate()
	{
		if (Player.Instance != null) {
			audioListener.position = Player.Instance.transform.position;
		}
	}

	public void SetVolume(float volumePercent, AudioChannel channel)
	{
		switch (channel)
		{
			case AudioChannel.Master:
				masterVolumePercent = volumePercent;
				break;
			case AudioChannel.Sfx:
				sfxVolumePercent = volumePercent;
				break;
			case AudioChannel.Music:
				musicVolumePercent = volumePercent;
				break;
		}

		for (int i = 0; i < musicSources.Length; i++) {
			if(i == activeMusicSourceIndex)
				musicSources[i].volume = musicVolumePercent* masterVolumePercent;
		}

		PlayerPrefs.SetFloat("MasterVolume", masterVolumePercent);
		PlayerPrefs.SetFloat("SfxVolume", sfxVolumePercent);
		PlayerPrefs.SetFloat("MusicVolume", musicVolumePercent);
		PlayerPrefs.Save();
	}

	public void PlayMusic(AudioClip clip, float fadeDuration = 1)
	{
		if (musicSources.Length <= 0)
			return;
		
		activeMusicSourceIndex = (musicSources.Length-1) - activeMusicSourceIndex;
		musicSources[activeMusicSourceIndex].clip = clip;
		musicSources[activeMusicSourceIndex].Play();

		StartCoroutine(AnimateMusicCrossfade(fadeDuration));
	}

	public void PlaySound(AudioClip clip, Vector3 pos)
	{
		AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
	}

	public void PlaySound2D(string soundName)
	{
		sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
	}

	public void PlaySound(string soundName, Vector3 pos)
	{
		if (library == null)
			return;

		PlaySound(library.GetClipFromName(soundName), pos);
	}

	private IEnumerator AnimateMusicCrossfade(float duration)
	{
		float percent = 0;

		while (percent < 1)
		{
			percent += Time.deltaTime * 1 / duration;
			musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent* masterVolumePercent, percent);
			musicSources[1-activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent* masterVolumePercent, 0, percent);

			yield return null;
		}
	}
}
