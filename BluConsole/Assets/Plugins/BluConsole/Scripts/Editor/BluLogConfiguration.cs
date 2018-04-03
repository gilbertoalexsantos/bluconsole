using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace BluConsole.Editor
{

	[CreateAssetMenu(menuName="BluConsole/BluLogConfiguration")]
	public class BluLogConfiguration : ScriptableObject 
	{

		private static BluLogConfiguration _instance;

		public static BluLogConfiguration Instance
		{
			get
			{
				if (_instance != null)
					return _instance;

				string[] guids = AssetDatabase.FindAssets("t:" + typeof(BluLogConfiguration).ToString());
				if (guids.Length == 0) 
					return null;
				var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
				return _instance = (BluLogConfiguration) AssetDatabase.LoadAssetAtPath(assetPath, typeof(BluLogConfiguration));
			}
		}

		[Range(15, 50)]
		public float DefaultButtonHeightOffset = 15.0f;

		[Range(50, 1500)]
		public int MaxLengthListWindowText = 999;

		[Range(50, 1500)]
		public int MaxLengthDetailWindowText = 999;

		[Range(1, 999)]
		public int MaxAmountOfLogsCollapse = 999;

		[Range(1, 999)]
		public int MaxAmountOfLogs = 999;
		
		public Color ResizerColor = Color.black;

		[Range(50, 500)]
		public float SearchStringBoxWidth = 200f;

		[Range(60, 300)]
		public float MinHeightOfTopAndBotton = 60f;

		[Range(1, 30)]
		public float ResizerHeight = 1f;

		public Vector2 ListLogContentOffset = Vector2.zero;

		[Range(0, 30)]
		public int ListLogSize = 0;

	}

}
