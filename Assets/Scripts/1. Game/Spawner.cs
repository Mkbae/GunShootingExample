using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour
{
	public bool devMode;
	public bool networkMode;

	public Wave[] waves;
	public Enemy enemy;
	public Enemy_Net enemy_net;

	private LivingEntity playerEntitiy;
	private Transform playerT;

	private Wave currentWave;
	private int currentWaveNumber;

	private int enemiesRemainingToSpawn;
	private int enemiesRemainingAlive;
	private float nextSpawnTime;

	private MapGenerator map;

	private float timeBetweenCampingChecks = 2;
	private float campThresholdDistance = 1.5f;
	private float nextCampCheckTime;
	private Vector3 campPositionOld;
	private bool isCamping;

	private bool isDisabled;
	private bool isSetPlayer;

	public event System.Action<int> OnNewWave;

	public void SetPlayer()
	{
		if (networkMode)
			playerEntitiy = FindObjectOfType<Player_Net>();
		else
			playerEntitiy = FindObjectOfType<Player>();

		playerT = playerEntitiy.transform;
		playerEntitiy.OnDeath += OnPlayerDeath;

		nextCampCheckTime = timeBetweenCampingChecks + Time.time;
		campPositionOld = playerT.position;

		map = FindObjectOfType<MapGenerator>();
        NextWave ();
		isSetPlayer = true;
	}

	private void Update()
	{
		if (isDisabled)
			return;

		if (!isSetPlayer)
			return;

		if (playerT == null)
			return;

		if (Time.time > nextCampCheckTime)
		{
			nextCampCheckTime = Time.time + timeBetweenCampingChecks;

			isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
			campPositionOld = playerT.position;
		}

		if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
		{
			enemiesRemainingToSpawn--;
			nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

			StartCoroutine("SpawnEnemy");
		}

		if (devMode) {
			if (Input.GetKeyDown(KeyCode.Return))
			{
				StopCoroutine("SpawnEnemy");

				foreach (Enemy e in FindObjectsOfType<Enemy>())
					Destroy(e.gameObject);

				NextWave();
			}
		}
	}

	IEnumerator SpawnEnemy()
	{
		float spawnDelay = 1;
		float tileFlashSpeed = 4;


		Transform randomTile = map.GetRandomOpenTile();

		if (isCamping) {
			randomTile = map.GetTileFromPosition(playerT.position);
		}

		Material tileMat = randomTile.GetComponent<Renderer>().material;
		Color initialColor = Color.white;
		Color flashColor = Color.red;
		float spawnTimer = 0;

		while(spawnTimer < spawnDelay)
		{
			tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
		}

		if (networkMode)
		{
			CmdSpawn_Net(randomTile.position);
		}
		else
		{
			Enemy spawnedEnemy = Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
			spawnedEnemy.OnDeath += OnEnemyDeath;
			spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
		}
	}

	[Command]
	void CmdSpawn_Net(Vector3 randomtilePos)
	{
		Enemy_Net spawnedEnemy = Instantiate(enemy_net, randomtilePos + Vector3.up, Quaternion.identity) as Enemy_Net;
		spawnedEnemy.OnDeath += OnEnemyDeath;
		spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);

		NetworkServer.Spawn(spawnedEnemy.gameObject);
	}

	private void OnPlayerDeath()
	{
		isDisabled = true;
	}

	private void OnEnemyDeath()
	{
		enemiesRemainingAlive--;

		if (enemiesRemainingAlive <= 0)
		{
			NextWave ();
		}
	}

	private void ResetPlayerPosition()
	{
		playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
	}

	private void NextWave()
	{
		if (currentWaveNumber > 0 && currentWave != null && !currentWave.infinite) {
			AudioManager.Instance.PlaySound2D("Level Complete");
		}

		currentWaveNumber++;
		if (currentWaveNumber - 1 < waves.Length)
		{
			currentWave = waves [currentWaveNumber - 1];

			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;

			if (OnNewWave != null)
				OnNewWave(currentWaveNumber);

			ResetPlayerPosition();
		}
	}

	[System.Serializable]
	public class Wave 
	{
		public bool infinite;
		public int enemyCount;
		public float timeBetweenSpawns;

		public float moveSpeed;
		public int hitsToKillPlayer;
		public float enemyHealth;
		public Color skinColor;
	}
}
