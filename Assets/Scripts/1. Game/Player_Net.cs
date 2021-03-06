﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController_Net))]
public class Player_Net : LivingEntity
{
	private static Player_Net _instance;
	public static Player_Net Instance{
		get{
			if(_instance == null)
				_instance = FindObjectOfType(typeof(Player_Net)) as Player_Net;
			return _instance;
		}
	}

	public float moveSpeed = 5;

	public Crosshairs corsshairs;

	private Camera viewCamera;
	private PlayerController controller;
	private GunController_Net gunController;

	Spawner spawner;

	private void Awake()
	{
		controller = GetComponent<PlayerController>();
		gunController = GetComponent<GunController_Net>();
		viewCamera = Camera.main;
		spawner = FindObjectOfType<Spawner>();
		if(spawner != null)
			spawner.OnNewWave += OnNewWave;
	}

	protected override void Start ()
	{
		base.Start ();

		if (isLocalPlayer)
		{
			FindObjectOfType<GameUI>().SetPlayer_Net(this);
			FindObjectOfType<ScoreKeeper>().SetPlayer_Net(this);
			if(spawner != null)
				spawner.SetPlayer();
		}
		else
		{
			corsshairs.gameObject.SetActive(false);
		}
	}

	void OnNewWave(int waveNumber)
	{
		health = startingHealth;
		gunController.EquipGun(waveNumber - 1);
	}

	private void Update ()
	{
		if (!isLocalPlayer)
			return;
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

	public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		AudioManager.Instance.PlaySound("Impact", transform.position);

		base.TakeHit(damage, hitPoint, hitDirection);
	}

	public override void Die()
	{
		AudioManager.Instance.PlaySound("Player Death", transform.position);
		base.Die();
	}
}
