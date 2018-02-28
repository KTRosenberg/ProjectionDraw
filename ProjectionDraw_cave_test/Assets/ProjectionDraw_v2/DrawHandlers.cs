//#define DO_DEBUG_PRINT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FRL_KTR {

public interface IDrawArgs {
	List<List<Vector3>> curves { get; set; }
	Transform transform { get; set; }
}
public class DebugDrawArgs : IDrawArgs {
	public List<List<Vector3>> curves { get; set; }
	public Transform transform { get; set; }

	public DebugDrawArgs(
		List<List<Vector3>> curves,
		Transform transform
	) {
		this.curves = curves;
		this.transform = transform;
	}
}
public class LineRendererDrawArgs : IDrawArgs {
	public List<List<Vector3>> curves { get; set; }
	public Transform transform { get; set; }
	public List<LineRenderer> lineRenderers { get; set; }
	public LineRenderer lineRendererPrefab { get; set; }

	public LineRendererDrawArgs(
		List<List<Vector3>> curves,
		Transform transform,
		List<LineRenderer> lineRenderers,
		LineRenderer lineRendererPrefab
	) {
		this.curves = curves;
		this.transform = transform;
		this.lineRenderers = lineRenderers;
		this.lineRendererPrefab = lineRendererPrefab;
	}
}

public static class DrawFunctions {

	public static void DrawUnityDebugLine(List<List<Vector3>> c, Transform transform) {
		DrawUnityDebugLine(c, transform, Color.blue);
	}
	public static void DrawUnityDebugLine(List<List<Vector3>> c, Transform transform, Color color) {
		for (int k = 0; k < c.Count; k++) {
			List<Vector3> curve = c[k];
			for (int p = 0; p < curve.Count - 1; p++) {
				Vector3 point0 = curve[p];
				Vector3 point1 = curve[p + 1];

				Debug.DrawLine(
					transform.TransformPoint(point0),
					transform.TransformPoint(point1),
					color
				);
			}
		}
	}

	public static void DrawUnityLineRenderer(List<List<Vector3>> c, Transform transform, List<LineRenderer> lineRenderers, LineRenderer lineRendererPrefab) {
		while (lineRenderers.Count < c.Count) {
			lineRenderers.Add(UnityEngine.Object.Instantiate(lineRendererPrefab));
		}
		while (lineRenderers.Count > c.Count) {
			LineRenderer rend = lineRenderers[lineRenderers.Count - 1];
			lineRenderers.RemoveAt(lineRenderers.Count - 1);
			UnityEngine.Object.Destroy(rend.gameObject);
		}
		for (int k = 0; k < c.Count; k++) {
			List<Vector3> curve = c[k];
			lineRenderers[k].positionCount = curve.Count;

			Vector3[] arr = curve.ToArray();
			for (int i = 0; i < curve.Count; i++) {
				arr[i] = transform.TransformPoint(arr[i]);
			}
			lineRenderers[k].SetPositions(arr);
		}
	}


	public static void Default(IDrawArgs args) {
		DebugDrawArgs _args = args as DebugDrawArgs;
		if (_args == null) {
			Debug.LogError("INVALID ARGUMENTS TO DRAW HANDLER");
			return;
		}

		List<List<Vector3>> c       = _args.curves;
		Transform transform         = _args.transform;

		for (int k = 0; k < c.Count; k++) {
			List<Vector3> curve = c[k];
			for (int p = 0; p < curve.Count - 1; p++) {
				Vector3 point0 = curve[p];
				Vector3 point1 = curve[p + 1];

				Debug.DrawLine(
					transform.TransformPoint(point0),
					transform.TransformPoint(point1),
					Color.blue
				);
			}
		}
	}

	public static void DrawWithLineRenderer(IDrawArgs args) {
		LineRendererDrawArgs _args = args as LineRendererDrawArgs;
		if (_args == null) {
			Debug.LogError("INVALID ARGUMENTS TO DRAW HANDLER");
			return;
		}

		// unpack args
		List<List<Vector3>> c            = _args.curves;
		Transform transform              = _args.transform;
		List<LineRenderer> lineRenderers = _args.lineRenderers;
		LineRenderer lineRendererPrefab  = _args.lineRendererPrefab;

		while (lineRenderers.Count < c.Count) {
			lineRenderers.Add(UnityEngine.Object.Instantiate(lineRendererPrefab));
		}
		while (lineRenderers.Count > c.Count) {
			LineRenderer rend = lineRenderers[lineRenderers.Count - 1];
			lineRenderers.RemoveAt(lineRenderers.Count - 1);
			UnityEngine.Object.Destroy(rend.gameObject);
		}
		for (int k = 0; k < c.Count; k++) {
			List<Vector3> curve = c[k];
			lineRenderers[k].positionCount = curve.Count;

			Vector3[] arr = curve.ToArray();
			for (int i = 0; i < curve.Count; i++) {
				arr[i] = transform.TransformPoint(arr[i]);
			}
			lineRenderers[k].SetPositions(arr);
		}
	}
}

public static class PreprocessPipelineFunctions {
	public static List<List<Vector3>> TestStreamingCurvePreprocessFunc(List<List<Vector3>> curves) {
		int count = curves.Count;
		int countSub = 0;
		List<List<Vector3>> adjustedCurves = new List<List<Vector3>>();
		if (count < 1) {
			return adjustedCurves;
		}

		for (int i = 0; i < count; i++) {
			List<Vector3> adjustedCurve = new List<Vector3>();
			List<Vector3> originalCurve = curves[i];
			countSub = originalCurve.Count;
			for (int j = 0; j < countSub; j++) {
				Vector3 v = new Vector3(
					originalCurve[j].x + Mathf.PerlinNoise(originalCurve[j].x + 0.1f * Mathf.Sin(Time.time), originalCurve[(int)Mathf.Clamp((float)(countSub - j - 1), 0.0f, (float)(countSub - 1))].x),
					originalCurve[j].y + Mathf.PerlinNoise(originalCurve[j].y + 0.15f * Mathf.Sin(Time.time), originalCurve[(int)Mathf.Clamp((float)(countSub - j - 2), 0.0f, (float)(countSub - 1))].y),
					originalCurve[j].z + Mathf.PerlinNoise(originalCurve[j].z + 0.12f * Mathf.Sin(Time.time), originalCurve[(int)Mathf.Clamp((float)(countSub - j - 3), 0.0f, (float)(countSub - 1))].z)
				);
				adjustedCurve.Add(v);
			}
			adjustedCurves.Add(adjustedCurve);
		}

		return adjustedCurves;
	}

	public static List<List<Vector3>> TestStreamingCurvePreprocessFuncNMinusOne(List<List<Vector3>> curves) {
		int count = curves.Count;
		int countSub = 0;
		List<List<Vector3>> adjustedCurves = new List<List<Vector3>>();
		if (count < 1) {
			return adjustedCurves;
		}

		for (int i = 0; i < count - 1; i++) {
			List<Vector3> adjustedCurve = new List<Vector3>();
			List<Vector3> originalCurve = curves[i];
			countSub = originalCurve.Count;
			for (int j = 0; j < countSub; j++) {
				Vector3 v = new Vector3(
					originalCurve[j].x + Mathf.PerlinNoise(originalCurve[j].x + 0.1f * Mathf.Sin(Time.time), originalCurve[(int)Mathf.Clamp((float)(countSub - j - 1), 0.0f, (float)(countSub - 1))].x),
					originalCurve[j].y + Mathf.PerlinNoise(originalCurve[j].y + 0.15f * Mathf.Sin(Time.time), originalCurve[(int)Mathf.Clamp((float)(countSub - j - 2), 0.0f, (float)(countSub - 1))].y),
					originalCurve[j].z + Mathf.PerlinNoise(originalCurve[j].z + 0.12f * Mathf.Sin(Time.time), originalCurve[(int)Mathf.Clamp((float)(countSub - j - 3), 0.0f, (float)(countSub - 1))].z)
				);
				adjustedCurve.Add(v);
			}
			adjustedCurves.Add(adjustedCurve);
		}

		List<Vector3> _adjustedCurve = new List<Vector3>();
		List<Vector3> _originalCurve = curves[count - 1];
		countSub = _originalCurve.Count;
		for (int j = 0; j < countSub; j++) {
			Vector3 v = new Vector3(
				_originalCurve[j].x + Mathf.PerlinNoise(_originalCurve[j].x, _originalCurve[(int)Mathf.Clamp((float)(countSub - j - 1), 0.0f, (float)(countSub - 1))].x),
				_originalCurve[j].y + Mathf.PerlinNoise(_originalCurve[j].y, _originalCurve[(int)Mathf.Clamp((float)(countSub - j - 2), 0.0f, (float)(countSub - 1))].y),
				_originalCurve[j].z + Mathf.PerlinNoise(_originalCurve[j].z, _originalCurve[(int)Mathf.Clamp((float)(countSub - j - 3), 0.0f, (float)(countSub - 1))].z)
			);
			_adjustedCurve.Add(v);
		}
		adjustedCurves.Add(_adjustedCurve);

		if (count - 1 >= 0 && countSub - 1 >= 0) {
			adjustedCurves[count - 1][countSub - 1] = curves[count - 1][countSub - 1];
		}

		return adjustedCurves;
	}

}

}
