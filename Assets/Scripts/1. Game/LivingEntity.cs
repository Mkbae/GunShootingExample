using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LivingEntity : NetworkBehaviour, IDamageable
{
	public float startingHealth;

	public float health { get; protected set; }
	protected bool isDead;

	public event System.Action OnDeath;

	protected virtual void Start()
	{
		health = startingHealth;
	}

	public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		TakeDamage (damage);
	}

	public virtual void TakeDamage(float damage)
	{
		health -= damage;

		if (health <= 0 && !isDead)
		{
			Die ();
		}
	}

	[ContextMenu("Slef Destruct")]
	public virtual void Die()
	{
		isDead = true;

		if (OnDeath != null)
			OnDeath ();

		Destroy (gameObject);
	}
}
