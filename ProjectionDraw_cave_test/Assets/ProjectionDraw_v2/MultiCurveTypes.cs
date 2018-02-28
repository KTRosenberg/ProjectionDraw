using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;

namespace FRL_KTR {

public enum MultiCurveType {
	CURVE = 0,
//	SPLINE = 1, temp
//	HYBRID = 2,
//	CHALKTALK_BIRD = 3
}

public interface IMultiCurve {

	List<List<Vector3>> curves { get; set; }
	void Draw(IDrawArgs args);
}

public class Curve : IMultiCurve {
	public List<List<Vector3>> curves {
		get; set;
	}

	public Curve(Action<IDrawArgs> d, Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		curves = new List<List<Vector3>>();
		_draw = (preprocessorPipeline == null) ? d : (IDrawArgs _args) => {
			_args.curves = preprocessorPipeline(_args);
			d(_args);
		};
	}
	public Curve(List<List<Vector3>> c, Action<IDrawArgs> d, Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		curves = new List<List<Vector3>>();
		_draw = (preprocessorPipeline == null) ? d : (IDrawArgs _args) => {
			_args.curves = preprocessorPipeline(_args);
			d(_args);
		};

		if (c == null) {
			return;
		}
		for (int i = 0; i < c.Count; i++) {
			curves.Add(new List<Vector3>(c[i]));
		}
	}

	private Action<IDrawArgs> _draw;
	public void Draw(IDrawArgs args) {
		args.curves = (args.curves == null) ? this.curves : args.curves;
		_draw(args);
	}
}

//public class Spline : IMultiCurve {
//	public List<List<Vector3>> curves {
//		get {
//			return curves;
//		}
//		set {
//			curves = value;
//		}
//	}
//	public List<List<Vector3>> keys;
//
//	private DrawFunc _draw = DrawFunctions.Default;
//	public void Draw(IDrawArgs args) {
//		args.curves = (args.curves == null) ? this.curves : args.curves;
//		_draw(args);
//	}
//
//	public Spline() {
//	}
//	public Spline(List<List<Vector3>> keys) {
//		// TODO
//	}
//}
//
//// TODO
//public class Hybrid : IMultiCurve {
//	public List<List<Vector3>> curves {
//		get {
//			return curves;
//		}
//		set {
//			curves = value;
//		}
//	}
//	public List<DrawFunc> drawHandlers;
//	List<List<Vector3>>   container;
//
//	public Hybrid() {
//		container = new List<List<Vector3>>(1);
//	}
//
//
//	// TODO adding of curves
//
//	public void Draw(IDrawArgs args) {
//
//		List<List<Vector3>> c = (args.curves == null) ? this.curves : args.curves;
//		args.curves = this.container;
//		for (int i = 0; i < c.Count; i++) {
//			// wrap list in outer list to create 2d list with one element (TEMPORARY SOLUTION)
//			this.container[0] = c[i];
//			// draw with the curve-specific draw handler
//			this.drawHandlers[i](args);
//		}
//	}
//}
//
// TODO
public class ChalktalkBird : IMultiCurve {
	private ChalktalkBirdLib _bird;

	public List<List<Vector3>> curves {
		get {
			return _bird.curves;
		}
		set {}
	}

	public ChalktalkBird(Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		_draw = (preprocessorPipeline == null) ? DrawFunctions.DrawWithLineRenderer : new Action<IDrawArgs>((IDrawArgs _args) => {
			_args.curves = preprocessorPipeline(_args);
			DrawFunctions.DrawWithLineRenderer(_args);
		});
		_bird = new ChalktalkBirdLib();
	}
	public ChalktalkBird(Action<IDrawArgs> d,  Func<IDrawArgs, List<List<Vector3>>> preprocessorPipeline = null) {
		_draw = (preprocessorPipeline == null) ? d : (IDrawArgs _args) => {
			_args.curves = preprocessorPipeline(_args);
			d(_args);
		};
		_bird = new ChalktalkBirdLib();
	}

	private Action<IDrawArgs> _draw;
		
	public void Draw(IDrawArgs args) {
		_bird.Update(Time.deltaTime, args.transform);

		args.curves = this.curves;
//		foreach (List<Vector3> curve in args.curves) { // not sure if necessary loop yet
//			for (int i = 0; i < curve.Count; i++) {
//				curve[i] = _parentTransform.InverseTransformPoint(curve[i]);
//			}
//		}
		_draw(args);
	}
}



//[CustomEditor(typeof(MultiCurveFactory))]
//public class MultiCurveFactoryEditor : Editor {
//	// TODO
//	public ProjectionCurveContainer c;
//	public override void OnInspectorGUI() { 
//		DrawDefaultInspector();
//		if (c == null || c.isInitialized) {
//			return;
//		}
//
//		MultiCurveFactory script = (MultiCurveFactory)target;
//		//Func<IMultiCurve> gen;
//
//		if (GUILayout.Button("curve")) {
//			// DROP-DOWN
//		} else if (GUILayout.Button("spline")) {
//			// DROP-DOWN
//		} else if (GUILayout.Button("Chalktalk Bird")) {
//			// DROP-DOWN
//		}
//
//		// TODO
//		return;
//		// c.Init(gen);
//	}
//}

}