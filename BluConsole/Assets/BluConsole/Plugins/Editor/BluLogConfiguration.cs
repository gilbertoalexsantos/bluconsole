using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace BluConsole.Editor
{

	[CreateAssetMenu()]
	public class BluLogConfiguration : ScriptableObject 
	{

		public float DefaultButtonHeightOffset = 15.0f;
		public int MaxLengthMessage = 999;
		public int MaxAmountOfLogsCollapse = 999;
		public int MaxAmountOfLogs = 999;
		public Color ResizerColor = Color.black;
		public float SearchStringBoxWidth = 200f;
		public float MinHeightOfTopAndBotton = 60f;
		public float ResizerHeight = 1f;

		public static BluLogConfiguration Instance
		{
			get
			{
				string[] guids = AssetDatabase.FindAssets("t:" + typeof(BluLogConfiguration).ToString());
				if (guids.Length == 0) 
					return null;
				var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
				return (BluLogConfiguration) AssetDatabase.LoadAssetAtPath(assetPath, typeof(BluLogConfiguration));
			}
		}

	}

}
