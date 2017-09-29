using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

	public Transform tilePrefab;
	public Vector2 mapSize;

	[Range(0,1)]
	public float outlinePercent;

	private void Start()
	{
		GeneratorMap ();
	}

	public void GeneratorMap()
	{
		string holderName = "Generated Map";
		if (transform.FindChild (holderName)) {
			DestroyImmediate (transform.FindChild (holderName).gameObject);
		}

		Transform mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;

		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				Vector3 tilePos = new Vector3 (-mapSize.x * 0.5f + 0.5f + x, 0, -mapSize.y * 0.5f +0.5f +y);
				Transform tile = Instantiate (tilePrefab, tilePos, Quaternion.Euler (Vector3.right * 90)) as Transform;
				tile.localScale = Vector3.one * (1 - outlinePercent);
				tile.parent = mapHolder;
			}
		}
	}
}
