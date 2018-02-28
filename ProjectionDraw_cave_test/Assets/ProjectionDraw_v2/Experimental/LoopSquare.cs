using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRL_KTR {
//[ExecuteInEditMode]
public class LoopSquare : MonoBehaviour {
	public ProjectionCurveContainer projectionMultiCurve;
	public Camera origin;
	public bool project = false;
	public void Update() {
		if (projectionMultiCurve == null) {
			return;
		}
		if (Input.GetKeyDown(KeyCode.P)) {
			project = !project;
		}
		if (!projectionMultiCurve.isInitialized) {
//			projectionMultiCurve.Init(MultiCurveFactoryStatic.Curve(
//				DrawFunctions.DrawWithLineRenderer, PreprocessPipelineFunctions.TestStreamingCurvePreprocessFunc
//			),	DrawMode.UNITY_LINE_RENDERER);
//			projectionMultiCurve.Init(MultiCurveFactoryStatic.Curve(
//				DrawFunctions.DrawWithLineRenderer, (IDrawArgs args) => { return PreprocessPipelineFunctions.TestStreamingCurvePreprocessFunc(args.curves); }
//			),	DrawMode.UNITY_LINE_RENDERER);

			projectionMultiCurve.Init(MultiCurveFactoryStatic.Curve(
				DrawFunctions.DrawWithLineRenderer, (IDrawArgs args) => {
					if (!project) {
						return args.curves;
					}
					return ProjectionCurveContainer.SampleAndProject(args.curves, Camera.main, projectionMultiCurve.transform, 0.006, true, false);
				}),	DrawMode.UNITY_LINE_RENDERER);

			List<List<Vector3>> square = new List<List<Vector3>>();
			List<Vector3> squareCurve = new List<Vector3>();

			Transform t = projectionMultiCurve.transform;
			squareCurve.Add(/*t.InverseTransformPoint*/(new Vector3(-1.0f, 1.0f, 0.0f)));
			squareCurve.Add(/*t.InverseTransformPoint*/(new Vector3(-1.0f, -1.0f, 0.0f)));
			squareCurve.Add(/*t.InverseTransformPoint*/(new Vector3(1.0f, -1.0f, 0.0f)));
			squareCurve.Add(/*t.InverseTransformPoint*/(new Vector3(1.0f, 1.0f, 0.0f)));
			squareCurve.Add(/*t.InverseTransformPoint*/(new Vector3(-1.0f, 1.0f, 0.0f)));
			square.Add(squareCurve);
			projectionMultiCurve.data.curves = square;
			//this.gameObject.SetActive(false);
		}
		//
	}
}

}