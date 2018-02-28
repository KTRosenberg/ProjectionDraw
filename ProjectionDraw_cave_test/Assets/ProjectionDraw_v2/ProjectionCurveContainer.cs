#define DO_COMPILE
#define ADD_NOISE_TO_PROJECTION

//#define DO_DEBUG_PRINT
//#define DO_PRINT_IS_MODIFIED

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace FRL_KTR {

#region Projection

public enum State {
	DEFAULT,
	PROJECTED,
	COUNT_STATES
}

public enum DrawMode {
	DEBUG_LINE,
	UNITY_LINE_RENDERER,
	COUNT_DRAW_MODES
}
	
[ExecuteInEditMode]
public class ProjectionCurveContainer : MonoBehaviour {
	public ProjectionMultiCurve data;

	public LineRenderer lineRendererPrefab;

	void OnApplicationQuit() {
		if (data.lineRenderers != null) {
			foreach (LineRenderer rend in data.lineRenderers) {
				Destroy(rend);
			}
		}
	}

	public State state { 
		get { return this.data.state; }
		private set { 
			if (strokeIsInProgress) {
				return;
			} 
			this.data.state = value;
		}
	}

	public bool isModified {
		get { return data.isModified; } set { data.isModified = value; }
	}

	void Awake() {
		this._draw = DefaultDraw;
	}

	public void Init(Func<IMultiCurve> mcGen, DrawMode drawMode = DrawMode.DEBUG_LINE) {
		if (isInitialized) {
			return;
		}
		this.data = new ProjectionMultiCurve(mcGen, drawMode);
		SetToDefaultCurveMode();
		isModified = true;
	}

	public bool isInitialized { get { return (data != null); } }
		
	public void SetToDefaultCurveMode() {
		this.state = State.DEFAULT;
	}
	public void SetToProjectedCurveMode() {
		this.state = State.PROJECTED;
	}

	// DRAW-TO-SCREEN PROCEDURES // ------------------------------------------------------------------
	#region UPDATE_AND_DRAW_TO_SCREEN

	private delegate void DrawFunction(ProjectionMultiCurve c);
	private DrawFunction _draw;

	public void DefaultDraw(ProjectionMultiCurve mc) {
#if DO_DEBUG_PRINT
		Debug.Log(this.state);
#endif
		List<List<Vector3>> curves = mc.curves;
#if DO_DEBUG_PRINT
		Debug.Log("num curves to draw: " + curves.Count);
#endif

		// if using Unity LineRenderer, only re-draw the curves when point data has been modified or 
		// the perspective has been switched
		// TODO
		switch (data.drawMode) {
		case DrawMode.UNITY_LINE_RENDERER:
			if (!data.isModified) {
				return;
			}
			mc.multiCurveType.Draw(new LineRendererDrawArgs(mc.curves, this.transform, mc.lineRenderers, lineRendererPrefab));
			break;
		default:
			mc.multiCurveType.Draw(new DebugDrawArgs(mc.curves, this.transform));
			break;
		}
	}

	public bool continuousChange = false;
	void Update() {
		if (!isInitialized) {
			return;
		}
		if (transform.hasChanged || continuousChange) {
			isModified = true;
			transform.hasChanged = false;
		}
#if DO_PRINT_IS_MODIFIED
		if (isModified) {
			Debug.Log("isModified");
		}
#endif
#if UNITY_EDITOR
		_draw = DefaultDraw;
#endif
		_draw(data);
		isModified = false;
	}
	#endregion
	// DRAW-INTERACTION PROCEDURES // ----------------------------------------------------------------
	#region DRAW-INTERACTION
	public bool strokeIsInProgress { get ; private set;}

	public int Count {
		get { return data.curves.Count; }
	}

	// TODO different lists won't necessarily have the same numbers of curves, so this may need to be changed
	public void AddCurve() {
		data.curvesDefault.Add(new List<Vector3>());
		data.curvesScreen.Add(new List<Vector3>());
		data.curvesProjected.Add(new List<Vector3>());
		this.isModified = true;
	}

	public void AddCurve(ProjectionMultiCurve data) {
		data.curvesDefault.Add(new List<Vector3>());
		data.curvesScreen.Add(new List<Vector3>());
		data.curvesProjected.Add(new List<Vector3>());
		this.isModified = true;
	}

	private void RemoveLastCurve() {
		// TODO, probably only need to remove one or two depending on the state, but will leave as-is
		// REMEMBER TO CLEAR LineRenderers if draw loop ends before list of renderers ends
		if (data.curvesDefault.Count > 0) {
			data.curvesDefault.RemoveAt(data.curvesDefault.Count - 1);
			this.isModified = true;
		}
		if (data.curvesScreen.Count > 0) {
			data.curvesScreen.RemoveAt(data.curvesScreen.Count - 1);
			this.isModified = true;
		}
		if (data.curvesProjected.Count > 0) {
			data.curvesProjected.RemoveAt(data.curvesProjected.Count - 1);
			this.isModified = true;
		}
	}

	public void ChangeInverseTransform(List<List<Vector3>> curves, Transform transformOriginal, Transform transformNew) {
		foreach (List<Vector3> curve in curves) {
			for (int i = 0; i < curve.Count; i++) {
				curve[i] = transformNew.InverseTransformPoint(transformOriginal.TransformPoint(curve[i]));
			}
		}
	}
	public void MergeFromList<T>(List<T> thisData, List<T> otherData) {
		thisData.AddRange(otherData);
		otherData.Clear();
	}
	public void MergeFromList2D<T>(List<List<T>> thisData, List<List<T>> otherData) {
		for (int i = 0; i < otherData.Count; i++) {
			thisData.Add(new List<T>(otherData[i]));
		}
		otherData.Clear();
	}
	public void MergeFrom(ProjectionCurveContainer other) {
		if (data.multiCurveType.GetType() != other.data.multiCurveType.GetType()) {
			return;
		}
			
		ChangeInverseTransform(other.data.curvesDefault, other.transform, transform);
		MergeFromList2D(data.curvesDefault, other.data.curvesDefault);

		ChangeInverseTransform(other.data.curvesScreen, other.transform, transform);
		MergeFromList2D(data.curvesScreen, other.data.curvesScreen);

		ChangeInverseTransform(other.data.curvesProjected, other.transform, transform);
		MergeFromList2D(data.curvesProjected, other.data.curvesProjected);

		MergeFromList(data.lineRenderers, other.data.lineRenderers);

		this.isModified = true;
		other.isModified = true;
	}

	public void Clear() {
		strokeIsInProgress = false;
		data.Clear();
		SetToDefaultCurveMode();
	}

	public void BeginStroke() {
		strokeIsInProgress = true;
		AddCurve();
	}

	public void EndStroke() {
		if (data.curves.Count > 0 && data.curves[data.curves.Count - 1].Count < 2) {
			RemoveLastCurve();
		}
#if DO_DEBUG_PRINT
		Debug.Log("END STROKE: NUMCURVES " + data.curves.Count);
#endif
		strokeIsInProgress = false;
	}

	// re-project screen points (overwrites default curve)
	public void ReProjectUsingScreenPoints(Camera origin) {
		SetToDefaultCurveMode();
		data.curvesDefault.Clear();

		// for each existing screen point point
		for (int i = 0; i < data.curvesScreen.Count; i++) {
			List<Vector3> curveScreen = data.curvesScreen[i];

			List<Vector3> currCurve = new List<Vector3>();
			List<bool> currVisibility = new List<bool>();
			data.curvesDefault.Add(currCurve);

			for (int p = 0; p < curveScreen.Count; p++) {
				// raycast from origin to screen point
				Ray r = origin.ScreenPointToRay(curveScreen[p]);
				RaycastHit hit;
				if (Physics.Raycast(r, out hit)) {
					// update projected point
					currCurve.Add(transform.InverseTransformPoint(hit.point));
					currVisibility.Add(true);
				} else if (currCurve.Count == 1) {
					currCurve.Clear();
					currVisibility.Clear();
				} else if (p < curveScreen.Count - 2) {
					currCurve = new List<Vector3>();
					currVisibility = new List<bool>();
					data.curvesDefault.Add(currCurve);
				} else {
					break;
				}
			}

			if (currCurve.Count == 1) {
				data.curvesDefault[data.curvesDefault.Count - 1].Clear();
			}
		}
		isModified = true;
	}
		
	public delegate Ray CreateRayFunc(Camera origin, Vector3 pos);
	public static Ray ScreenRay(Camera origin, Vector3 pos) {
		return origin.ScreenPointToRay(pos);
	}
	public static Ray ViewportRay(Camera origin, Vector3 pos) {
		return origin.ViewportPointToRay(pos);
	}

	public void ProjectionDraw(Camera origin, ProjectionDrawInputHandler.InputStatus status, CreateRayFunc func) {
		ProjectionDraw(origin, status.pos, status.strokeStart, status.strokeEnd, func);
	}

	public void ProjectionDraw(Camera origin, Vector3 pos, bool strokeStart, bool strokeEnd, CreateRayFunc func) {
		SetToDefaultCurveMode();

		if (strokeStart) {
			BeginStroke();
		} else if (strokeEnd) {
			EndStroke();
			return;
		} else if (!strokeIsInProgress) {
			//EndStroke();
			return;
		}

		Ray r = func(origin, pos);
		UnityEngine.Debug.DrawRay(r.origin, r.direction + r.direction);
		RaycastHit hit;
		if (Physics.Raycast(r, out hit)) {
			//Debug.DrawLine(r.origin, hit.point, Color.green);
#if DO_DEBUG_PRINT
			Debug.Log("target hit at world position: " + hit.point);
#endif 

			// ignore duplicate points
			List<Vector3> curve = data.curves[data.curves.Count - 1];
			Vector3 inverseTransformHitPoint = transform.InverseTransformPoint(hit.point);
			if (curve.Count > 0 && curve[curve.Count - 1] == inverseTransformHitPoint) {
				return;
			}

			data.curves[data.curves.Count - 1].Add(inverseTransformHitPoint);
			data.curvesScreen[data.curvesScreen.Count - 1].Add(pos);
			isModified = true;
		} else {
			EndStroke();
			BeginStroke();
		}
	}

	private static Vector3 LocalLerp(List<Vector3> line, double t) {
		return CT.SplineUtils.Sample(line, (float)t);
	}

	public void ProjectionDrawAutoSample(Camera origin, ProjectionDrawInputHandler.InputStatus status, double tVal, CreateRayFunc func) {
		ProjectionDrawAutoSample(origin, status.pos, status.strokeStart, status.strokeEnd, tVal, func);
	}

	public void ProjectionDrawAutoSample(Camera origin, Vector3 pos, bool strokeStart, bool strokeEnd, double tVal, CreateRayFunc func) {
		SetToDefaultCurveMode();

		if (strokeStart) {
			BeginStroke();
		} else if (strokeEnd) {
			EndStroke();
			return;
		} else if (!strokeIsInProgress) {
			//EndStroke();
			return;
		}

		Ray r = func(origin, pos);
		//UnityEngine.Debug.DrawRay(r.origin, r.direction);
		RaycastHit hit;
		if (Physics.Raycast(r, out hit)) {
			//Debug.DrawLine(r.origin, hit.point, Color.green);
#if DO_DEBUG_PRINT
Debug.Log("target hit at world position: " + hit.point);
#endif 

			// ignore duplicate points
			List<Vector3> curve = data.curves[data.curves.Count - 1];
			Vector3 firstInverseTransformHit = transform.InverseTransformPoint(hit.point);
			Vector3 inverseTransformHit;
			if (curve.Count > 0 && curve[curve.Count - 1] == firstInverseTransformHit) {
				return;
			}

			List<Vector3> currCurve = data.curves[data.curves.Count - 1];
			List<Vector3> currCurveScreen = data.curvesScreen[data.curvesScreen.Count - 1];
			// project points in-between
			if (currCurve.Count > 0) {

				//List<Vector3> sampledSubCurve = new List<Vector3>();
				//List<Vector3> sampledSubScreen = new List<Vector3>();
				//List<bool> sampledSubVisibility = new List<bool>();

				Vector3 startPointSubCurve = currCurve[currCurve.Count - 1];
				Vector3 startPointSubScreen = currCurveScreen[currCurveScreen.Count - 1];

				List<Vector3> line = new List<Vector3>(2);
				line.Add(transform.InverseTransformPoint(startPointSubCurve));
				line.Add(hit.point);
				List<Vector3> lineScreen = new List<Vector3>(2);
				lineScreen.Add(startPointSubScreen);
				lineScreen.Add(pos);

				for (double t = tVal; t < 1.0; t += tVal) {
					Vector3 screenPointLerp = LocalLerp(lineScreen, tVal);

					r = func(origin, screenPointLerp);
					if (Physics.Raycast(r, out hit)) {
						inverseTransformHit = transform.InverseTransformPoint(hit.point);
						if (currCurve[currCurve.Count - 1] == inverseTransformHit) {
							continue;
						}

						currCurve.Add(inverseTransformHit);
						currCurveScreen.Add(screenPointLerp);
					} else if (currCurve.Count > 0 && currCurve.Count < 2) {
						currCurve.Clear();
					} else {
						currCurve = new List<Vector3>();
						data.curvesDefault.Add(currCurve);
					}
				}
				if (currCurve.Count > 0 && currCurve.Count < 2) {
					data.curvesDefault[data.curvesDefault.Count - 1].Clear();
				}
				// TODO, simplify, probably just lerp from start to hit point since a curve might exist between points even if the endpoint doesn't hit
			}

			currCurve.Add(firstInverseTransformHit);
			currCurveScreen.Add(pos);
			isModified = true;
		} else {
			EndStroke();
			BeginStroke();
		}
	}
		
	#endregion
	// MAIN PROJECTION PROCEDURES //-----------------------------------------------------------------------------
	#region PROJECTION
	public void Project(Camera origin, bool allowBehindSurface = false, bool allowBehindOrigin = false) {
		ProjectInner(data.curves, origin, allowBehindSurface, allowBehindOrigin);
		SetToProjectedCurveMode();
	}
	private void ProjectInner(
		List<List<Vector3>> curves,
		Camera origin,
		bool allowBehindSurface = false, 
		bool allowBehindOrigin = false
	) {
		isModified = true;
		// TEMP RE-INITIALIZATION clearing then adding, better to overwrite existing slots when possible
		data.curvesProjected.Clear();
		data.curvesScreen.Clear();
		for (int i = 0; i < curves.Count; i++) {
			data.curvesProjected.Add(new List<Vector3>());
			data.curvesScreen.Add(new List<Vector3>());
		}
		////////

		for (int c = 0; c < curves.Count; c++) {
			List<Vector3> curve = curves[c];
			for (int p = 0; p < curve.Count; p++) {
				// transform point into world space
				Vector3 pointWorld = this.transform.TransformPoint(curve[p]);

				// position in the viewport
				Vector3 vPos = origin.WorldToViewportPoint(pointWorld);
				// position on the screen
				Vector3 sPos = origin.WorldToScreenPoint(pointWorld);
				// skip points outside the viewport
				if (!allowBehindOrigin && 
					vPos.x < 0 || vPos.x > 1 ||
				    vPos.y < 0 || vPos.y > 1 ||
				    vPos.z <= 0) {

					// Add placeholder values ////////
					data.curvesProjected[c].Add(curve[p]);
					data.curvesScreen[c].Add(sPos);
					////////
					continue;
				}

				Ray r = origin.ScreenPointToRay(sPos);

				RaycastHit hit;
				// detect collision
				if (Physics.Raycast(r, out hit)) { // collision hit
					Vector3 toHit = hit.point - origin.transform.position;
					Vector3 toPoint = pointWorld - origin.transform.position;
					// when allowBehindSurface disabled,
					// hit-point must be farther away than the curve point
					if (!allowBehindSurface && toHit.sqrMagnitude < toPoint.sqrMagnitude) {
						// Temp
//						data.curvesProjected[c].Add(curve[p]);
//						data.curvesScreen[c].Add(sPos);
//						data.curvesVisibility[c].Add(false);
						continue;
					}
					// transform point back into local space
					data.curvesProjected[c].Add(this.transform.InverseTransformPoint(hit.point));
					data.curvesScreen[c].Add(sPos);
				} else { // collision miss
					// TEMP ////////
//					data.curvesProjected[c].Add(curve[p]);
//					data.curvesScreen[c].Add(sPos);
//					data.curvesVisibility[c].Add(false);
					////////
				}
			}
		}
	}
		
	public static List<List<Vector3>> SampleAndProject(
		List<List<Vector3>> curves,
		Camera origin,
		Transform transform,
		double tVal = 0.1,
		bool allowBehindSurface = false, 
		bool allowBehindOrigin = false
	) {
		List<List<Vector3>> projected = new List<List<Vector3>>();
		for (int c = 0; c < curves.Count; c++) {
			List<Vector3> curve = curves[c];
			List<Vector3> curveP = new List<Vector3>();
			projected.Add(curveP);

			// transform into world space
//			List<Vector3> curveTrans = new List<Vector3>(curve.Count);
//			for (int i = 0; i < curve.Count; i++) {
//				curveTrans.Add(transform.TransformPoint(curve[i]));
//			}
			// create lerped points
			List<Vector3> curveLerp = new List<Vector3>();
			List<Vector3> line = new List<Vector3>(new Vector3[2]);
			for (int i = 1; i < curve.Count; i++) {
				line[0] = curve[i - 1];
				line[1] = curve[i];
				for (double t = 0.0; t <= 1.0; t += tVal) {
					curveLerp.Add(LocalLerp(line, t));
				}
			}
			curve = curveLerp;

			// try projecting lerped points
			for (int p = 0; p < curve.Count; p++) {
				// transform point into world space
				Vector3 pointWorld = transform.TransformPoint(curve[p]);

				// position in the viewport
				Vector3 vPos = origin.WorldToViewportPoint(pointWorld);
				// position on the screen
				Vector3 sPos = origin.WorldToScreenPoint(pointWorld);
				// skip points outside the viewport
				if (!allowBehindOrigin && 
					vPos.x < 0 || vPos.x > 1 ||
					vPos.y < 0 || vPos.y > 1 ||
					vPos.z <= 0) {
		
					if (curveP.Count == 1) {
						curveP.Clear();
					} else {
						curveP = new List<Vector3>();
						projected.Add(curveP);
					}
					continue;
				}
				#if ADD_NOISE_TO_PROJECTION
				float noise = 10.0f * Mathf.PerlinNoise(origin.transform.position.x + c * p, origin.transform.position.y + c + p);
				sPos.x += noise;
				#endif
				Ray r = origin.ScreenPointToRay(sPos);

				RaycastHit hit;
				// detect collision
				if (Physics.Raycast(r, out hit)) { // collision hit
					Vector3 toHit = hit.point - origin.transform.position;
					Vector3 toPoint = pointWorld - origin.transform.position;
					// when allowBehindSurface disabled,
					// hit-point must be farther away than the curve point
					if (!allowBehindSurface && toHit.sqrMagnitude < toPoint.sqrMagnitude) {
						if (curveP.Count == 1) {
							curveP.Clear();
						} else {
							curveP = new List<Vector3>();
							projected.Add(curveP);
						}
						continue;
					}

					curveP.Add(transform.InverseTransformPoint(hit.point));
				} else { // collision miss
					if (curveP.Count == 1) {
						curveP.Clear();
					} else {
						curveP = new List<Vector3>();
						projected.Add(curveP);
					}
					continue;
				}
			}
			if (curveP.Count < 2 && projected.Count > 0) {
				projected.RemoveAt(projected.Count - 1);
			}
		}

		return projected;
	}


	#endregion
}

}

#endregion
