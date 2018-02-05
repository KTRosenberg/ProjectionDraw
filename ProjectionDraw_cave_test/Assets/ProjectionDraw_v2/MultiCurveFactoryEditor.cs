using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace FRL2 {

[CustomEditor(typeof(MultiCurveFactory))]
[CanEditMultipleObjects]
public class MultiCurveFactoryEditor : Editor {
	//private readonly DrawFunc func = DrawFunctions.DrawWithLineRenderer;
	private readonly DrawMode drawMode = DrawMode.UNITY_LINE_RENDERER;

//	SerializedProperty _curves;
//	SerializedProperty _keys;
//
//	void OnEnable() {
//		_curves = serializedObject.FindProperty("curves");
//		_keys = serializedObject.FindProperty("keys");
//	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		//serializedObject.Update();

		MultiCurveFactory script = (MultiCurveFactory)target;

//		serializedObject.Update();
//		EditorGUILayout.PropertyField(_curves, new GUIContent("curves"), true);
//		EditorGUILayout.PropertyField(_keys, new GUIContent("curves"), true);
//		serializedObject.ApplyModifiedProperties();

		switch (script.curveType) {
		case MultiCurveType.CURVE:
			if (GUILayout.Button("NO-OP")) {
			}
			break;
		default:
			break;
		}
	}
}

}
