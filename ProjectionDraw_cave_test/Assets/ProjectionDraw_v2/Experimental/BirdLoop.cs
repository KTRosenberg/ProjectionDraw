using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRL_KTR {

public class BirdLoop : MonoBehaviour {
	public ProjectionCurveContainer projectionMultiCurve;

	public bool project = false;
	void Update () {
		if (projectionMultiCurve == null) {
			return;
		}
		if (Input.GetKeyDown(KeyCode.P)) {
			project = !project;
		}

		if (!projectionMultiCurve.isInitialized) {
			projectionMultiCurve.continuousChange = true;
			projectionMultiCurve.Init(MultiCurveFactoryStatic.ChalktalkBird(
				DrawFunctions.DrawWithLineRenderer, (IDrawArgs args) => {
					if (!project) {
						return args.curves;
					}
					return ProjectionCurveContainer.SampleAndProject(curves : args.curves, origin : Camera.main, transform : projectionMultiCurve.transform, tVal : 0.007, allowBehindSurface : true, allowBehindOrigin : false);
				}),	DrawMode.UNITY_LINE_RENDERER);

			//this.gameObject.SetActive(false);
		}
	}

}

}