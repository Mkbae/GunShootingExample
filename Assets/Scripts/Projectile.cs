using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
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
			OnHitObject (initialCollisions [0]);
		}
	}

	public void SetSpeed(float newSpeed)
	{
		speed = newSpeed;
	}

	private void Update()
	{
		float moveDistance = speed * Time.deltaTime;

		CheckCollisions (moveDistance);

		transform.Translate (Vector3.forward * moveDistance);
	}

	private void CheckCollisions(float moveDistance)
	{
		Ray ray = new Ray (transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))//QueryTrigerInteraction.Collide = IsTrigger의 상태인 콜라이더도 체크함.
		{
			OnHitObject (hit);	
		}
	}

	private void OnHitObject(RaycastHit hit)
	{
		IDamageable damageableObject = hit.collider.GetComponent<IDamageable> ();
		if (damageableObject != null)
			damageableObject.TakeHit (damage, hit);
		
		Destroy (gameObject);
	}

	void OnHitObject(Collider col)
	{
		IDamageable damageableObject = col.GetComponent<IDamageable> ();
		if (damageableObject != null)
			damageableObject.TakeDamage (damage);

		Destroy (gameObject);
	}
}
