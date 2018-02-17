﻿using System.Collections;
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
		public int MaxLengthCollapse = 999;

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
