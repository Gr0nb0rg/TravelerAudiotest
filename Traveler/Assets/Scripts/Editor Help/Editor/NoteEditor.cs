using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
    Authors: Ludvig Grönborg
    Last Edited: 2017/04/02
*/

/*
    SUMMARY
    Editor for the note script.
    Only function is displaying a larger text box than the standard
*/

[CustomEditor(typeof(Note))]
public class NoteEditor : Editor
{

    public SerializedProperty m_noteMessageProp;
    void OnEnable()
    {
        m_noteMessageProp = serializedObject.FindProperty("m_noteMessage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        m_noteMessageProp.stringValue = EditorGUILayout.TextArea(m_noteMessageProp.stringValue, GUILayout.MaxHeight(75));
        serializedObject.ApplyModifiedProperties();
    }
}
