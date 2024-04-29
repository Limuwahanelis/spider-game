using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(HelloWorldConstraint))]
//public class HelloWorldConstraintEditor: Editor
//{
//    SerializedProperty _constrainedObject;
//    SerializedProperty _sourceObject;
//    private void OnEnable()
//    {
//        var data = serializedObject.FindProperty("m_Data");
//        _sourceObject = data.FindPropertyRelative("sourceObject");
//        _constrainedObject = data.FindPropertyRelative("constrainedObject");
//    }
//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        base.OnInspectorGUI();

//        //EditorGUILayout.PropertyField( _constrainedObject );
//        //EditorGUILayout.PropertyField( _sourceObject );
//        serializedObject.ApplyModifiedProperties();
//    }
//}