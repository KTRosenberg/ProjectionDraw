//#define DO_COMPILE
#if DO_COMPILE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRL {

public class CurveProjector : MonoBehaviour {
	public ProjectionCurve curve;
	public Camera origin;
	public double sampleStep;
	private static double DEFAULT_SAMPLE_STEP = 0.001;

	public bool allowBehindSurface = false;
	public bool allowBehindOrigin = false;

	void OnValidate() {
		if (sampleStep <= 0.0) {
			sampleStep = DEFAULT_SAMPLE_STEP;
		}
	}

	// TODO MultiCurve versions

	public void Project() {
		List<Vector3> transformedCurve = new List<Vector3>();
		// transform points into world space
		for (int i = 0; i < curve.points.Count; i++) {
			transformedCurve.Add(curve.transform.TransformPoint(curve.points[i]));
		}

		// project TODO do transformations in curve's method
		curve.Project(
			origin, transformedCurve, allowBehindSurface, allowBehindOrigin
		);

		projectedCurve.pointsOriginal = new List<Vector3>(curve.points);
		// inverse transform back into local space
		for (int i = 0; i < projectedCurve.points.Count; i++) {
			projectedCurve.points[i] = curve.transform.InverseTransformPoint(projectedCurve.points[i]);
		}
	}


	public static Vector3 SampleCurve(List<Vector3> curve, float t) {
		return CT.SplineUtils.Sample(curve, t);
	}

	public void SampleAndProject() {
		// if sample step invalid, project non-sampled curve
		if (sampleStep <= 0.0) {
			return CurveProjector.Project(origin, curve, allowBehindOrigin, allowBehindOrigin);
		}

		List<Vector3> sampledCurve = new List<Vector3>();
		// transform points into world space, sample each point
		for (double t = 0.0; t <= 1.0; t += sampleStep) {
			sampledCurve.Add(curve.transform.TransformPoint(
				CurveProjector.SampleCurve(curve.points, (float)t)
			));
		}

		// project points
		ProjectedCurveData projectedCurve = ProjectedCurve.ProjectCurve(
			origin, sampledCurve, allowBehindSurface, allowBehindOrigin
		);
		/* TODO make transformation automatic in ProjectCurve () instead? */ 
		projectedCurve.pointsOriginal = new List<Vector3>(curve.points);
		// inverse transform points back into local space
		for (int i = 0; i < projectedCurve.points.Count; i++) {
			projectedCurve.points[i] = curve.transform.InverseTransformPoint(projectedCurve.points[i]);
		}

		ProjectedCurve script = curve.gameObject.AddComponent(typeof(ProjectedCurve)) as ProjectedCurve;
		script.projectedPoints = projectedCurve;
		//Destroy(curve);
	}


	public void ConvertToBasicCurve() {
		Curve script = curve.gameObject.AddComponent(typeof(Curve)) as Curve;
		script.points = curve.points;
		Destroy(curve);
	}

	public void SampleandConvertToBasicCurve() {
		// if sample step invalid, convert to curve without sampling
		if (sampleStep <= 0.0) {
			CurveProjector.ConvertToBasicCurve(curve);
		}

		List<Vector3> sampledCurve = new List<Vector3>();
		// sample each point
		for (double t = 0.0; t <= 1.0; t += sampleStep) {
			sampledCurve.Add(CurveProjector.SampleCurve(curve.points, (float)t));
		}
		curve.points = sampledCurve;

	}

	public void RevertProjectedCurveToFullCurve() {
		if (curve as ProjectedCurve == null) {
			return curve;
		}

		Curve script = curve.gameObject.AddComponent(typeof(Curve)) as Curve;
		ProjectedCurve pc = (curve as ProjectedCurve);
		List<Vector3> points = new List<Vector3>(pc.projectedPoints.pointsOriginal);
		script.points = points;

		Destroy(curve);

		return script;
	}

	public void ProjectUsingOriginalPoints() {
		if (curve as ProjectedCurve != null) {
			curve = CurveProjector.RevertProjectedCurveToFullCurve(curve);
		}
		return CurveProjector.Project(origin, curve, allowBehindSurface, allowBehindOrigin);
	}

	public void SampleAndProjectUsingOriginalPoints() {
		if (curve as ProjectedCurve != null) {
			curve = CurveProjector.RevertProjectedCurveToFullCurve(curve);
		}
		return CurveProjector.SampleAndProject(origin, curve, allowBehindSurface, allowBehindOrigin, sampleStep);
	}
}

}
#endif
