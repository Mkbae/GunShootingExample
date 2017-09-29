using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
	public enum State
	{
		Idle,
		Chasing,
		Attacking
	};
	State currentState;

	NavMeshAgent pathfinder;
	Transform target;
	LivingEntity targetEntity;
	Material skinMaterial;

	Color originalColor;

	//유니티에서 1은 1meter이다.
	float attackDistanceThreshold = 0.5f;
	float timeBetweenAttacks = 1;
	float damage = 1;

	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;

	bool hasTarget;

	protected override void Start ()
	{
		base.Start ();
		pathfinder = GetComponent<NavMeshAgent> ();
		skinMaterial = GetComponent<Renderer> ().material;

		originalColor = skinMaterial.color;

		if (Player.Instance != null) 
		{
			currentState = State.Chasing;
			hasTarget = true;

			target = Player.Instance.transform;
			targetEntity = target.GetComponent<LivingEntity> ();
			targetEntity.OnDeath += OnTargetDeath;

			myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;

			StartCoroutine (UpdatePath ());
		}
	}

	private void OnTargetDeath()
	{
		hasTarget = false;
		currentState = State.Idle;
	}

	private void Update()
	{
		if (!hasTarget)
			return;
		
		if (Time.time > nextAttackTime)
		{
			//Vector3.Distance는 제곱근 연산이라 처리 비용이 비싸다.
			float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

			//공격범위에 들어옴.
			if (sqrDstToTarget < Mathf.Pow (attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
			{
				nextAttackTime = Time.time + timeBetweenAttacks;

				StartCoroutine (Attack ());
			}
		}

	}

	private IEnumerator Attack()
	{
		currentState = State.Attacking;
		//이동 정지
		pathfinder.enabled = false;

		Vector3 originPosition = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;

		skinMaterial.color = Color.red;
		bool hasAppliedDagame = false;

		while (percent <= 1)
		{
			if (percent >= 0.5f && !hasAppliedDagame) {
				hasAppliedDagame = true;
				targetEntity.TakeDamage (damage);
			}

			percent += Time.deltaTime * attackSpeed;

			float interpolation = (-Mathf.Pow(percent,2)+ percent) * 4;
			transform.position = Vector3.Lerp (originPosition, attackPosition, interpolation);

			yield return null; //Update처리가종료된 후 아랫부분이 처리된다.
		}

		currentState = State.Chasing;
		pathfinder.enabled = true;
		skinMaterial.color = originalColor;
	}

	private IEnumerator UpdatePath()
	{
		float refreshRate = .25f;

		while (hasTarget)
		{
			if (currentState == State.Chasing) {
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold);

				if (!isDead) {
					pathfinder.SetDestination (targetPosition);
				}
			}
			yield return new WaitForSeconds (refreshRate);
		}
	}
}
