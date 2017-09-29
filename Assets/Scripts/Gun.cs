using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
	public Transform muzzle;
	public Projectile projectile;
	public float msBetweenShot = 100; //격발 간격 (밀리초)
	public float muzzleVelocity = 35; //발사 순간의 총알 속력

	float nextShotTime;

	public void Shoot()
	{
		if (Time.time > nextShotTime)
		{
			nextShotTime = Time.time + msBetweenShot / 1000;
			Projectile newProjectile = Instantiate (projectile, muzzle.position, muzzle.rotation) as Projectile;
			newProjectile.SetSpeed (muzzleVelocity);
		}
	}
}
