using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ParticleSystem))]
public class EnemyDieEffect_Net : NetworkBehaviour
{
	private ParticleSystem particle;

	private float destroyTime;
	private float curTime;

	private void Start()
	{
		particle = GetComponent<ParticleSystem>();
		destroyTime = particle.main.startLifetime.constant;
		curTime = Time.time;
	}

	[ServerCallback]
	void Update()
	{
		if (Time.time - curTime < destroyTime)
			return;

		NetworkServer.Destroy(gameObject);
	}
}
