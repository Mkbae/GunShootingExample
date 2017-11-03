using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Shell_Net : NetworkBehaviour
{
	public Rigidbody myRigidbody;
	public float forceMin;
	public float forceMax;

	float lifetime = 4;
	float fadetime = 2;

	private bool IsDestroy;

	private void Start()
	{
		float force = Random.Range(forceMin, forceMax);
		myRigidbody.AddForce(transform.right * force);
		myRigidbody.AddTorque(Random.insideUnitSphere * force); //회전

		StartCoroutine(Fade());
	}


	private IEnumerator Fade()
	{
		IsDestroy = false;
		yield return new WaitForSeconds(lifetime);

		float percent = 0;
		float fadeSpeed = 1 / fadetime;
		Material mat = GetComponent<Renderer>().material;
		Color initialColor = mat.color;

		while (percent < 1)
		{
			percent += Time.deltaTime * fadeSpeed;
			mat.color = Color.Lerp(initialColor, Color.clear, percent);
			yield return null;
		}

		IsDestroy = true;
	}

	[ServerCallback]
	void Update()
	{
		if (!IsDestroy)
			return;

		NetworkServer.Destroy(gameObject);
	}
}
