using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunController_Net : NetworkBehaviour
{
	public Transform weaponHold;

	public Gun_Net[] allGuns;
	private Gun_Net equippedGun;

	public void EquipGun(Gun_Net gunToEquip)
	{
		if (equippedGun != null)
			Destroy (equippedGun.gameObject);

		equippedGun = Instantiate (gunToEquip,weaponHold.position,weaponHold.rotation) as Gun_Net;
		equippedGun.transform.parent = weaponHold;

		CmdCreateGun(equippedGun.gameObject);
	}

	[Command]
	void CmdCreateGun(GameObject go)
	{
		NetworkServer.Spawn(go);
	}

	public void EquipGun(int weaponIndex)
	{
		EquipGun(allGuns[weaponIndex]);	
	}

	public void OnTriggerHold()
	{
		if (equippedGun == null)
			return;
		
		equippedGun.OnTriggerHold ();
	}

	public void OnTriggerRelease()
	{
		if (equippedGun == null)
			return;

		equippedGun.OnTriggerRelease();
	}

	public float GunHeight { 
		get {
			return weaponHold.position.y;
		}
	}

	public void Aim(Vector3 aimPoint)
	{
		if (equippedGun == null)
			return;

		equippedGun.Aim(aimPoint);
	}

	public void Reload()
	{
		if (equippedGun == null)
			return;

		equippedGun.Reload();
	}
}
