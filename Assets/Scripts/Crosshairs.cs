using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
	public LayerMask targetMask;
	public SpriteRenderer[] aims;
	public Color dotHighlightColor;
	private Color originalDotColor;

	private void Start()
	{
		Cursor.visible = false;
		originalDotColor = aims[0].color;
	}

	private void FixedUpdate() 
	{
		transform.Rotate(Vector3.forward * 40 * Time.deltaTime);
	}

	public void DetectTagets(Ray ray)
	{
		SetAimColor(Physics.Raycast(ray, 100, targetMask));
	}

	private void SetAimColor(bool isTarget)
	{
		for (int i = 0; i < aims.Length; i++)
			aims[i].color = isTarget ? dotHighlightColor : originalDotColor;
	}
}
