using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
	public SoundGroup[] soundGroups;

	private Dictionary<string, AudioClip[]> groupDic = new Dictionary<string, AudioClip[]>();

	private void Awake()
	{
		foreach (SoundGroup sgroup in soundGroups) {
			groupDic.Add(sgroup.groupID, sgroup.group);
		}
	}

	public AudioClip GetClipFromName(string name)
	{
		if (groupDic.ContainsKey(name))
		{
			AudioClip[] sounds = groupDic[name];
			return sounds[Random.Range(0, sounds.Length)];
		}
		else {
			return null;
		}
	}


	[System.Serializable]
	public class SoundGroup
	{
		public string groupID;
		public AudioClip[] group;
	}
}
