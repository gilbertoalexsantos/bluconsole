using BluConsole.Core;
using UnityEditor;


namespace BluConsole.Editor
{

[CustomEditor(typeof(BluLogSettings))]
public class BluLogSettingsEditor : UnityEditor.Editor
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

}
