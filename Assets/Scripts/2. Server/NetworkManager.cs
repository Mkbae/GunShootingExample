using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
	private static NetworkManager _instance;
	public static NetworkManager Instance
	{
		get
		{
			if (_instance == null)
				_instance = FindObjectOfType(typeof(NetworkManager)) as NetworkManager;
			return _instance;
		}
	}

	public System.Action OnFindOtherUser;
	//public System.Action OnNotFindOtherUser;

	//private float waitUserTime = 30;


	private const string typeName = "2345ajdfgnadfdafgih";
	private const string gameName = "top down gun shooting";

	private HostData[] hostList;

	private void Awake()
	{
		NetworkManager[] manager = FindObjectsOfType(typeof(NetworkManager)) as NetworkManager[];
		if (manager.Length >= 2)
		{
			for (int i = 0; i<manager.Length; i++)
				if (manager[i] == this)
					Destroy(manager [i].gameObject);
		}

		DontDestroyOnLoad(gameObject);
	}


	//function.
	public void RefreshHostList()
	{
		Debug.Log("Refresh");
		MasterServer.RequestHostList(typeName);
	}

	private void StartServer () //not opened host server.
	{
		Debug.Log("Start");
		Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	private void JoinServer(HostData hostData) //opened host server.
	{
		Debug.Log("Join");
		Network.Connect(hostData);
	}

	//callback.
	void OnServerInitialized()
	{
		Debug.Log("create host server.");
	}

	void OnConnectedToServer()
	{
		Debug.Log("join to host");

		if (OnFindOtherUser != null)
			OnFindOtherUser();
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log("Player" + player.ipAddress +": "+ player.port+" 에서 연결됨");

		if (OnFindOtherUser != null)
            OnFindOtherUser();
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		Debug.Log("서버에 연결할 수 없습니다 : "+error);
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
		{
			hostList = MasterServer.PollHostList();

			if (hostList.Length > 0)
				JoinServer(hostList[0]);
			else
				StartServer();
		}
	}

	//void OnGUI()
	//{
	//	if (!Network.isClient && !Network.isServer)
	//	{
	//		if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
	//			StartServer();

	//		if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
	//			RefreshHostList();

	//		if (hostList != null)
	//		{
	//			for (int i = 0; i < hostList.Length; i++)
	//			{
	//				if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
	//					JoinServer(hostList[i]);
	//			}
	//		}
	//	}
	//}
}
