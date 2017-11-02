using System.Collections;
using UnityEngine;
using UnityEditor;


[CustomEditor (typeof(MapGenerator))]
public class MapEditor : Editor {

	public override void OnInspectorGUI()
	{
		MapGenerator map = target as MapGenerator;
		if (DrawDefaultInspector())
		{
			map.GeneratorMap();
		}

		if (GUILayout.Button("Generate Map"))
		{
			map.GeneratorMap();
		}
	}
}
