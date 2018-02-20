using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FRL2 {

public class ProjectionMultiCurve {
	public IMultiCurve         multiCurveType;
	public List<List<Vector3>> curves { 
		get {
			switch (state) {
			case State.DEFAULT:
				return multiCurveType.curves;
			case State.PROJECTED:
				return curvesProjected;
			default: 
				goto case State.DEFAULT;
			}
		}
		set {
			switch (state) {
			case State.DEFAULT:
				multiCurveType.curves = value;
				break;
			case State.PROJECTED:
				curvesProjected = value;
				break;
			default: 
				goto case State.DEFAULT;
			}
			isModified = true;
		}
	}
	public List<List<Vector3>> curvesDefault {
		get {
			return multiCurveType.curves;
		}
		set {
			multiCurveType.curves = value;
			isModified = true;
		}
	}
	public List<List<Vector3>> _curvesScreen;
	public List<List<Vector3>> curvesScreen {
		get {
			//StackTrace st = new StackTrace();
			//UnityEngine.Debug.Log(st.GetFrame(1).GetMethod().Name);
			return _curvesScreen;
		}
		set {
			_curvesScreen = value;
		}
	}
	public List<List<Vector3>> curvesProjected;
	public List<LineRenderer> lineRenderers;
	public bool isModified;

	public State state;

	public DrawMode drawMode;

	public ProjectionMultiCurve(Func<IMultiCurve> mcGen, DrawMode drawMode = DrawMode.UNITY_LINE_RENDERER) {
		multiCurveType = mcGen();
		curvesScreen     = new List<List<Vector3>>();
		curvesProjected  = new List<List<Vector3>>();

		this.drawMode = drawMode;
		if (drawMode == DrawMode.UNITY_LINE_RENDERER) {
			lineRenderers = new List<LineRenderer>();
		}
		state = State.DEFAULT;
		isModified = true;
	}

	public void Clear() {
		curvesDefault.Clear();
		curvesScreen.Clear();
		curvesProjected.Clear();
		isModified = true;
	}
}

}
