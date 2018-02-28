using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FRL_KTR {

public class MultiCurveFactory : MonoBehaviour {
	public MultiCurveType curveType;
	public List<List<Vector3>> curves;
	public List<List<Vector3>> keys; // TODO conditionally show properties

	void Awake() {
		curves = new List<List<Vector3>>();
		keys = new List<List<Vector3>>();
	}

	public static Func<IMultiCurve> Curve(List<List<Vector3>> curves, Action<IDrawArgs> d, Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return MultiCurveFactoryStatic.Curve(curves, d, preprocessorPipeline);
	}
	public static Func<IMultiCurve> Curve(Action<IDrawArgs> d, Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return MultiCurveFactoryStatic.Curve(d, preprocessorPipeline);
	}
	public static Func<IMultiCurve> Curve(Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return MultiCurveFactoryStatic.Curve(DrawFunctions.DrawWithLineRenderer, preprocessorPipeline);
	}

	//	public static Func<IMultiCurve> Spline(List<List<Vector3>> keys) {
	//		return MultiCurveFactoryStatic.Spline(keys);
	//	}
	//	public static Func<IMultiCurve> Spline() {
	//		return MultiCurveFactoryStatic.Spline();
	//	}
	//
	public static Func<IMultiCurve> ChalktalkBird(Action<IDrawArgs> d, Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return MultiCurveFactoryStatic.ChalktalkBird(d, preprocessorPipeline);
	}
	public static Func<IMultiCurve> ChalktalkBird(Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return MultiCurveFactoryStatic.ChalktalkBird(preprocessorPipeline);
	}
}

public static class MultiCurveFactoryStatic {
	public static Func<IMultiCurve> Curve(List<List<Vector3>> curves, Action<IDrawArgs> d, Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return () => { return new Curve(curves, d, preprocessorPipeline); };
	}
	public static Func<IMultiCurve> Curve(Action<IDrawArgs> d, Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return () => { return new Curve(d, preprocessorPipeline); };
	}
	public static Func<IMultiCurve> Curve(Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return () => { return new Curve(DrawFunctions.DrawWithLineRenderer, preprocessorPipeline); };
	}

	//	public static Func<IMultiCurve> Spline(List<List<Vector3>> curveKeys) {
	//		return () => { return new Spline(curveKeys); };
	//	}
	//	public static Func<IMultiCurve> Spline() {
	//		return () => { return new Spline(); };
	//	}
	//
	public static Func<IMultiCurve> ChalktalkBird(Action<IDrawArgs> d, Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return () => { return new ChalktalkBird(d, preprocessorPipeline); };
	}
	public static Func<IMultiCurve> ChalktalkBird(Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		return () => { return new ChalktalkBird(preprocessorPipeline); };
	}
}

}
