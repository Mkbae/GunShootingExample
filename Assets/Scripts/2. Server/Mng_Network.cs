using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Mng_Network : NetworkManager
{
	private static Mng_Network _instance;
	public static Mng_Network Instance
	{
		get
		{
			if (_instance == null)
				_instance = FindObjectOfType(typeof(Mng_Network)) as Mng_Network;
			return _instance;
		}
	}

	public override void OnStartClient(NetworkClient client)
	{
		Debug.Log("OnStartClient : " + client.serverIp + " : " + client.serverPort);
	}

	public override void OnStartHost()
	{
		Debug.Log("OnStartHost");
	}

	public override void OnStopClient()
	{
		Debug.Log("OnStopClient");
	}

	public override void OnStopHost()
	{
		Debug.Log("OnStopHost");
	}

	//public override void OnClientConnect(NetworkConnection conn)
	//{
	//	Debug.Log(conn.address);
	//}
}
