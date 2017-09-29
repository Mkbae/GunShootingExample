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

	private PlayerController controller;
	private GunController gunController;

	protected override void Start ()
	{
		base.Start ();
		controller = GetComponent<PlayerController> ();
		gunController = GetComponent<GunController> ();
	}

	private void Update ()
	{
		//Movement.
		//input값. 키보드 방향키. GetAxisRaw -> not smoothing (키입력 해제시 바로 동작 정지)
		Vector3 moveInput = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		//input 값 방향벡터로 정규화.
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move (moveVelocity);


		//Look input.
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		//new Plane(법선 벡터 : 수직 벡터값, 원점에서부터의 거리)
		Plane groundPlane = new Plane (Vector3.up,Vector3.zero);
		float rayDistance;

		if(groundPlane.Raycast(ray,out rayDistance))
		{
			//바닥에 부딪힌 레이 지점.
			Vector3 point = ray.GetPoint (rayDistance);
//			Debug.DrawLine (ray.origin, point, Color.red);

			controller.LookAt (point);
		}


		//Weapon input.
		if(Input.GetMouseButton(0)) //좌클릭
		{
			gunController.Shoot ();
		}
	}
}
