using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Projectile_Net : NetworkBehaviour
{
	public LayerMask collisionMask;

	float speed = 10;
	float damage = 1;

	float lifeTime = 3;
	float skinWidth = .1f;//이동에 의한 raycast미 실행 보정값.

	void Start()
	{
		Destroy (gameObject, lifeTime);

		Collider[] initialCollisions = Physics.OverlapSphere (transform.position, 0.1f,collisionMask);
		if (initialCollisions.Length > 0)
		{
			OnHitObject (initialCollisions[0], transform.position);
		}
	}

	public void SetSpeed(float newSpeed)
	{
		speed = newSpeed;
	}

	[ServerCallback]
	private void Update()
	{
		float moveDistance = speed * Time.deltaTime;

		CheckCollisions (moveDistance);

		transform.Translate (Vector3.forward * moveDistance);
	}

	private void CheckCollisions(float moveDistance)
	{
		if (!isServer)
			return;

		Ray ray = new Ray (transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))//QueryTrigerInteraction.Collide = IsTrigger의 상태인 콜라이더도 체크함.
		{
			OnHitObject (hit.collider, hit.point);	
		}
	}

	void OnHitObject(Collider col, Vector3 hitPoint)
	{
		IDamageable damageableObject = col.GetComponent<IDamageable> ();
		if (damageableObject != null)
			damageableObject.TakeHit (damage, hitPoint, transform.forward);

		NetworkServer.Destroy (gameObject);
	}
}
