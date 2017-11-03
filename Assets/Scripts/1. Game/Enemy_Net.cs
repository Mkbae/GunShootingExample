using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Net : LivingEntity
{
	public enum State
	{
		Idle,
		Chasing,
		Attacking
	};
	State currentState;

	public ParticleSystem deathEffect;
	public static event System.Action OnDeathStatic;

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

	void Awake()
	{
		pathfinder = GetComponent<NavMeshAgent> ();
		myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
	}

	protected override void Start ()
	{
		base.Start ();

		Player_Net[] players = FindObjectsOfType(typeof(Player_Net)) as Player_Net[];
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].isLocalPlayer)
			{
				hasTarget = true;

				target = players[i].transform;
				targetEntity = target.GetComponent<LivingEntity> ();
				targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;

				break;
			}
		}

		if (hasTarget) 
		{
			currentState = State.Chasing;

			targetEntity.OnDeath += OnTargetDeath;

			StartCoroutine (UpdatePath ());
		}
	}

	public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
	{
		pathfinder.speed = moveSpeed;

		if (hasTarget) {
			damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
		}
		startingHealth = enemyHealth;

		ParticleSystem.MainModule main = deathEffect.main;
		main.startColor = new ParticleSystem.MinMaxGradient(new Color(skinColor.r, skinColor.g, skinColor.b, 1));

		skinMaterial = GetComponent<Renderer> ().material;
		skinMaterial.color = skinColor;
		originalColor = skinMaterial.color;
	}

	public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		AudioManager.Instance.PlaySound("Impact", transform.position);
		if (damage >= health) {

			if (OnDeathStatic != null)
				OnDeathStatic();

			AudioManager.Instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.main.startLifetime.constant);
		}

		base.TakeHit(damage, hitPoint, hitDirection);
	}

	private void OnTargetDeath()
	{
		hasTarget = false;
		currentState = State.Idle;
	}

	private void Update()
	{
		if (!hasTarget || target == null)
			return;
		
		if (Time.time > nextAttackTime)
		{
			//Vector3.Distance는 제곱근 연산이라 처리 비용이 비싸다.
			float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

			//공격범위에 들어옴.
			if (sqrDstToTarget < Mathf.Pow (attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
			{
				nextAttackTime = Time.time + timeBetweenAttacks;
				AudioManager.Instance.PlaySound("Enemy attack", transform.position);
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

				if (!isDead && hasTarget) {
					pathfinder.SetDestination (targetPosition);
				}
			}
			yield return new WaitForSeconds (refreshRate);
		}
	}
}
