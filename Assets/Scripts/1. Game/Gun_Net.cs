using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Gun_Net : NetworkBehaviour
{
	public enum FireMode
	{
		Auto,
		Burst,
		Single
	}
	public FireMode fireMode;

	public Transform[] projectileSpawn;
	public Projectile_Net projectile;
	public float msBetweenShot = 100; //격발 간격 (밀리초)
	public float muzzleVelocity = 35; //발사 순간의 총알 속력
	public int burstCount;
	public int projectilesPerMag;
	public float reloadTime = 1f;

	[Header("Recoil")]
	public Vector2 kickMinMax = new Vector2(0.05f, 0.2f);
	public Vector2 recoilAngleMinMax = new Vector2(3,5);
	public float recoilMoveSettleTime = 0.1f;
	public float recoilRotSettleTime = 0.1f;

	[Header("Effects")]
	public Transform shell;
	public Transform shellEjection;
	public AudioClip shootAudio;
	public AudioClip reloadAudio;

	private MuzzleFlash muzzleFlash;
	private float nextShotTime;

	private bool triggerReleasedSinceLastShot;
	private int shotsRemainingInBurst;
	private int projectilesRemainingInMag;
	private bool isReloading;

	Vector3 recoilSmoothDampVelocity;
	float recoilRotSmoothDampVelocity;
	float recoilAngle;

	private void Start()
	{
		muzzleFlash = GetComponent<MuzzleFlash>();
		shotsRemainingInBurst = burstCount;
		projectilesRemainingInMag = projectilesPerMag;
	}

	private void LateUpdate()
	{
		//animate recoil
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
		recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotSettleTime);
		transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

		if (!isReloading && projectilesRemainingInMag == 0) {
			Reload();
		}
	}

	private void Shoot()
	{
		CmdShootProcess();
	}

	[Command]
	void CmdShootProcess()
	{
		if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
		{
			if (fireMode == FireMode.Burst)
			{
				if (shotsRemainingInBurst == 0)
				{
					return;
				}
				shotsRemainingInBurst--;
			}
			else if (fireMode == FireMode.Single)
			{
				if (!triggerReleasedSinceLastShot)
					return;
			}

			for (int i = 0; i<projectileSpawn.Length; i++)
			{
				if (projectilesRemainingInMag == 0) {
					break;
				}

				projectilesRemainingInMag--;
				nextShotTime = Time.time + msBetweenShot / 1000;
				Projectile_Net newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile_Net;
				newProjectile.SetSpeed (muzzleVelocity);

				NetworkServer.Spawn(newProjectile.gameObject);
			}

			Transform shell_tf = Instantiate(shell, shellEjection.position, shellEjection.rotation) as Transform;
			NetworkServer.Spawn(shell_tf.gameObject);

			muzzleFlash.Activate();
			transform.localPosition -= Vector3.forward* Random.Range(kickMinMax.x, kickMinMax.y);
			recoilAngle += Random.Range(recoilAngleMinMax.x,recoilAngleMinMax.y);
			recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

			AudioManager.Instance.PlaySound(shootAudio, transform.position);
		}
	}

	public void Reload()
	{
		if (!isReloading && projectilesRemainingInMag != projectilesPerMag) {
            StartCoroutine(AnimateReload());
			AudioManager.Instance.PlaySound(reloadAudio, transform.position);
		}
	}

	IEnumerator AnimateReload()
	{
		isReloading = true;

		yield return new WaitForSeconds(0.2f);

		float reloadSpeed = 1f / reloadTime;
		float percent = 0;
		Vector3 initialRot = transform.localEulerAngles;
		float maxReloadAngle = 30;

		while (percent < 1) {
			percent += Time.deltaTime * reloadSpeed;
			float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
			float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);

			transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

			yield return null;
		}

		isReloading = false;
		projectilesRemainingInMag = projectilesPerMag;
	}

	public void Aim(Vector3 aimPoint)
	{
		if(!isReloading)
			transform.LookAt(aimPoint);
	}

	public void OnTriggerHold()
	{
		Shoot();
		triggerReleasedSinceLastShot = false;
	}

	public void OnTriggerRelease()
	{
		triggerReleasedSinceLastShot = true;
		shotsRemainingInBurst = burstCount;
	}
}
