using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
	public Image fadePlane;
	public GameObject gameOverUI;

	public RectTransform newWaveBanner;
	public Text newWaveTitle;
	public Text newWaveEnemyCount;
	public Text scoreUI;
	public Text gameOverScoreUI;
	public RectTransform healthBar;

	Spawner spawner;
	Player player;

	private void Awake()
	{
		spawner = FindObjectOfType<Spawner>();
		spawner.OnNewWave += OnNewWave;	
	}

	public void SetPlayer () {
		player = Player.Instance;
		player.OnDeath += OnGameOver;
	}

	private void FixedUpdate()
	{
		scoreUI.text = ScoreKeeper.score.ToString("D7");

		float healthPercent = 0;
		if (player != null)
			healthPercent = player.health / player.startingHealth;
		healthBar.localScale = new Vector3(healthPercent, 1, 1);
	}

	private void OnNewWave(int waveNumber)
	{
		newWaveTitle.text = string.Format("-Wave {0}-", waveNumber);
		string enemyCountString = spawner.waves[waveNumber - 1].enemyCount < 0 ? "Infinity" : (spawner.waves[waveNumber - 1].enemyCount).ToString();
		newWaveEnemyCount.text = string.Format("Enemies: {0}", enemyCountString);

		StartCoroutine(AnimateNewWaveBanner());
	}

	private void OnGameOver()
	{
		Cursor.visible = true;
		StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, 0.75f)));
		gameOverScoreUI.text = scoreUI.text;
		scoreUI.gameObject.SetActive(false);
		healthBar.transform.parent.gameObject.SetActive(false);
		gameOverUI.SetActive(true);
	}

	private IEnumerator AnimateNewWaveBanner()
	{
		float delay = 1.5f;
		float speed = 2.5f;
		float animatePercent = 0;
		int dir = 1;

		float endDelayTime = Time.time + 1 / speed + delay;

		while (animatePercent >= 0)
		{
			animatePercent += Time.deltaTime * speed * dir;

			if (animatePercent >= 1)
			{
				animatePercent = 1;
				if (Time.time > endDelayTime)
				{
					dir = -1;
				}
			}

			newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-180, 55, animatePercent);
			yield return null;
		}
	}

	private IEnumerator Fade(Color from, Color to, float time = 1)
	{
		float speed = 1 / time;
		float percent = 0;

		while (percent < 1)
		{
			percent += Time.deltaTime * speed;
			fadePlane.color = Color.Lerp(from, to, percent);
			yield return null;
		}
	}

	//UI Input
	public void StartNewGame()
	{
		SceneManager.LoadScene(1);
	}

	public void ReturnToMainMenu()
	{
		SceneManager.LoadScene(0);
	}
}
