using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
	private static Player _instance;
	public static Player Instance{
		get{
			if(_instance == null)
				_instance = FindObjectOfType(typeof(Player)) as Player;
			return _instance;
		}
	}

	public float moveSpeed = 5;

	public Crosshairs corsshairs;

	private Camera viewCamera;
	private PlayerController controller;
	private GunController gunController;

	Spawner spawner;
	NetworkView networkView;

	private void Awake()
	{
		controller = GetComponent<PlayerController>();
		gunController = GetComponent<GunController>();
		viewCamera = Camera.main;
		spawner = FindObjectOfType<Spawner>();
		spawner.OnNewWave += OnNewWave;
		networkView = GetComponent<NetworkView>();
	}

	protected override void Start ()
	{
		base.Start ();

		FindObjectOfType<GameUI>().SetPlayer();
		FindObjectOfType<ScoreKeeper>().SetPlayer();

		if (spawner.networkMode)
			spawner.SetPlayer();
	}

	void OnNewWave(int waveNumber)
	{
		health = startingHealth;
		gunController.EquipGun(waveNumber - 1);
	}

	private void Update ()
	{
		if (spawner.networkMode)
		{
			if (networkView == null)
				return;

			if (!networkView.isMine)
				return;
		}
		//Movement.
		//input값. 키보드 방향키. GetAxisRaw -> not smoothing (키입력 해제시 바로 동작 정지)
		Vector3 moveInput = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		//input 값 방향벡터로 정규화.
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move (moveVelocity);


		//Look input.
		Ray ray = viewCamera.ScreenPointToRay (Input.mousePosition);
		//new Plane(법선 벡터 : 수직 벡터값, 원점에서부터의 거리)
		Plane groundPlane = new Plane (Vector3.up, Vector3.up * gunController.GunHeight);
		float rayDistance;

		if(groundPlane.Raycast(ray,out rayDistance))
		{
			//바닥에 부딪힌 레이 지점.
			Vector3 point = ray.GetPoint (rayDistance);
//			Debug.DrawLine (ray.origin, point, Color.red);

			controller.LookAt (point);
			corsshairs.transform.position = point;
			corsshairs.DetectTagets(ray);

			if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).magnitude > 1) {
				gunController.Aim(point);
			}
		}

		//Weapon input.
		if(Input.GetMouseButton(0)) //좌클릭
		{
			gunController.OnTriggerHold ();
		}

		if(Input.GetMouseButtonUp(0)) //좌클릭
		{
			gunController.OnTriggerRelease ();
		}

		if(Input.GetKeyDown(KeyCode.R)) //좌클릭
		{
			gunController.Reload ();
		}

		if (transform.position.y < -10)
		{
			TakeDamage(health);
		}
	}

	public override void Die()
	{
		AudioManager.Instance.PlaySound("Player Death", transform.position);
		base.Die();
	}
}
