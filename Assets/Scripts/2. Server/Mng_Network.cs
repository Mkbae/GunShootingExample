using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Mng_Network : MonoBehaviour
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

	public System.Action OnFindOtherUser;
	public System.Action OnNotFindOtherUser;

	private const float waitUserTime = 15;
	private const string typeName = "2345ajdfgnadfdafgih";
	private const string gameName = "top down gun shooting";

	private HostData[] hostList;

	private NetworkManager manager;

	private void Awake()
	{
		Mng_Network[] myself = FindObjectsOfType(typeof(Mng_Network)) as Mng_Network[];
		if (myself.Length >= 2)
		{
			for (int i = 0; i<myself.Length; i++)
				if (myself[i] == this)
					Destroy(myself [i].gameObject);
		}

		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		manager = GetComponent<NetworkManager>();
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

		StartCoroutine("ConnectionCloseCoroutine");
	}

	private IEnumerator ConnectionCloseCoroutine()
	{
		float time = Time.time;

		while (Time.time - time < waitUserTime)
			yield return null;

		Debug.Log("대기시간 초과");

		Network.Disconnect();
		OnNotFindOtherUser();
	}

	void OnConnectedToServer()
	{
		Debug.Log("join to host");
		if (OnFindOtherUser != null)
		{
            StopCoroutine("ConnectionCloseCoroutine");
            OnFindOtherUser();
		}
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log("Player" + player.ipAddress +": "+ player.port+" 에서 연결됨");

		if (OnFindOtherUser != null)
        {
            StopCoroutine("ConnectionCloseCoroutine");
			OnFindOtherUser();
		}
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		Debug.Log("서버에 연결할 수 없습니다 : "+error);
		if (OnNotFindOtherUser != null)
		{
            StopCoroutine("ConnectionCloseCoroutine");
			OnNotFindOtherUser();
		}
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
