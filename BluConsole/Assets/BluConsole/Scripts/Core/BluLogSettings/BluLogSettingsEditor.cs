using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using BluConsole.Core;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BluLogSettings))]
public class BluLogSettingsEditor : Editor
{

    private SerializedProperty _filterProperty;

    private void OnEnable()
    {
        _filterProperty = serializedObject.FindProperty("_filter");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_filterProperty, true);

        serializedObject.ApplyModifiedProperties();
    }

}
