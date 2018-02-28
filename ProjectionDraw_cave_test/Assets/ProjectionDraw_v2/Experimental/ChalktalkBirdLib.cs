using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

namespace FRL_KTR {

public static class CTUtil {
	public static double SCurve(double t) { return t < 0 ? 0 : t > 1 ? 1 : t * t * (3 - t - t); }

	public static double Saw(double t) { t = 2 * t % 2; return t < 1 ? t : 2 - t; }

	public static double Mix(double a, double b, double t) {
		return a + (b - a) * t;
	}

	public static double Lerp(double t, double a, double b) {
		return a + t * (b - a);
	}

	public static Vector3 Ik(double a, double b, Vector3 C, ref Vector3 D) {
		double cc = (double)Vector3.Dot(C, C);
		double x = (1 + (a * a - b * b) / cc) / 2;
		double y = (double)Vector3.Dot(C, D)/cc;

		for (int i = 0; i < 3; i++) {
			D[i] -= (float)(y * C[i]);
		}
//		D.x = (float)(D.x - y * C.x); 
//		D.y = (float)(D.y - y * C.y); 
//		D.z = (float)(D.z - y * C.z);

		y = Mathf.Sqrt(Mathf.Max(0,(float)(a*a - cc*x*x)) / Vector3.Dot(D, D));

		for (int i = 0; i < 3; i++) {
			D[i] = (float)(x * C[i] + y * D[i]);
		}

		return D;
//		D.x = (float)(x * C.x + y * D.x); 
//		D.y = (float)(x * C.y + y * D.y); 
//		D.z = (float)(x*C.z + y*D.z);
	}

//	private static IDictionary<double, double> noise2P = new Dictionary<double, double>();
//	private static IDictionary<double, double> noise2U = new Dictionary<double, double>();
//	private static IDictionary<double, double> noise2V = new Dictionary<double, double>();

//	static FuncRetDouble _Random() {
//		double seed = 2;
//		double x = (seed % 30268) + 1;
//		seed = (seed - (seed % 30268)) / 30268;
//		double y = (seed % 30306) + 1;
//		seed = (seed - (seed % 30306)) / 30306;
//		double z = (seed % 30322) + 1;
//
//		return () => {
//			return (((x = (171 * x) % 30269) / 30268) +
//					((y = (172 * y) % 30307) / 30307) +
//					((z = (170 * z) % 30323) / 30323)) % 1;
//		};
//	}
//	delegate double FuncRetDouble();
//	static FuncRetDouble Random = _Random();
//
//	public static double Noise2(double x, double y) {
//		if (noise2P.Count == 0) {
//			IDictionary<double, double> _p = noise2P;
//			IDictionary<double, double> _u = noise2U;
//			IDictionary<double, double> _v = noise2V;		
//			int _i = 0;
//			int _j = 0;
//			for (; _i < 256; _i++) {
//				_p[_i] = _i; 
//				_u[_i] = 2 * Random() - 1;
//				_v[_i] = 2 * Random() - 1;
//				double _s = Mathf.Sqrt((float)(_u[_i] * _u[_i] + _v[_i] * _v[_i]));
//				_u[_i] /= _s;
//				_v[_i] /= _s;
//
//			}
//			while ((--_i) != 0) {
//				double k = _p[_i];
//				_p[_i] = _p[_j = (int)Mathf.Floor((float)(256 * Random()))];
//				_p[_j] = k;
//			}
//			for (_i = 0; _i < 256 + 2; _i++) {
//				_p[256 + _i] = _p[_i];
//				_u[256 + _i] = _u[_i];
//				_v[256 + _i] = _v[_i];
//			}
//				
//		}
//
//		IDictionary<double, double> P = noise2P;
//		IDictionary<double, double> U = noise2U;
//		IDictionary<double, double> V = noise2V;
//
//		x = (x + 4096) % 256;
//		y = (y + 4096) % 256;
//
//		int i = (int)Mathf.Floor((float)x);
//		double u = x - i;
//		double s = SCurve(u);
//
//		int j = (int)Mathf.Floor((float)x);
//		double v = y - j;
//		double t = SCurve(v);
//
//		double a = P[P[i] + j    ], b = P[P[i + 1] + j    ];
//		double c = P[P[i] + j + 1], d = P[P[i + 1] + j + 1];
//
//		return CTUtil.Mix(
//			CTUtil.Mix(u * U[a] +  v      * V[a], (u - 1) * U[b] + v       * V[b], s),
//			CTUtil.Mix(u * U[c] + (v - 1) * V[c], (u - 1) * U[d] + (v - 1) * V[d], s),
//			t
//		);
//	}

//	public class TransformStack {
//		private List<Transform> stack;
//		private int idx;
//		public Transform top { 
//			get { return stack[idx - 1]; }
//		}
//
//		public TransformStack() {
//			stack = new List<Transform>();
//			stack.Add(new GameObject("STACK IDENTITY").transform);
//			stack.Add(new GameObject("STACK").transform);
//			idx = 2;
//		}
//
//		public void Save() {
//			if (idx < stack.Count) {
//				stack[idx].position = top.position;
//				stack[idx].rotation = top.rotation;
//			} else {
//				stack.Add(UnityEngine.Object.Instantiate(top));
//			}
//			idx++;
//		}
//
//		public void Restore() {
//			if (idx > 2) {
//				idx--;
//			} else if (idx == 2) {
//				stack[idx - 1].position = Vector3.zero;
//				stack[idx - 1].rotation = Quaternion.identity;
//			}
//		}
//
//		public void Restart() {
//			idx = 2;
//		}
//
//		public void Clear() {
//			while (stack.Count > 2) {
//				UnityEngine.Object.Destroy(top);
//				stack.RemoveAt(stack.Count - 1);
//				idx--;
//			}
//			stack[1].position = Vector3.zero;
//			stack[1].rotation = Quaternion.identity;
//
//			idx = 2;
//		}
//
//		public override string ToString() {
//			string s = "{";
//			int len = idx;
//			for (int i = 0; i < len; i++) {
//				s = s + "[" + stack[i].position + ":" + stack[i].rotation + "]";
//			}
//			s += "}";
//
//			return s;
//		}
//	}

	public class MatrixStack {
		private List<Matrix4x4> stack;
		private int idxFirstAvail;

		public int Count { get { return idxFirstAvail; } }

		public Matrix4x4 top {
			get { return stack[idxFirstAvail - 1]; }
		}

		public MatrixStack() {
			stack = new List<Matrix4x4>();
			stack.Add(Matrix4x4.identity);
			idxFirstAvail = 1;
		}

		public void Identity() {
			stack[idxFirstAvail - 1] = Matrix4x4.identity;
		}
			
		public void Translate(Vector3 translation) {
			stack[idxFirstAvail - 1] *= Matrix4x4.Translate(translation);
		}
		public void Translate(double x, double y, double z) {
			Translate(new Vector3((float)x, (float)y, (float)z));
		}

		public void Rotate(Vector3 angle) {
			angle *= Mathf.Rad2Deg;
			stack[idxFirstAvail - 1] *= Matrix4x4.Rotate(Quaternion.Euler(angle));
		}
		public void Rotate(double x, double y, double z) {
			Rotate(new Vector3((float)x, (float)y, (float)z));
		}
		public void RotateX(double angle) {
			Rotate(new Vector3((float)angle, 0.0f, 0.0f));
		}
		public void RotateY(double angle) {
			Rotate(new Vector3(0.0f, (float)angle, 0.0f));
		}
		public void RotateZ(double angle) {
			Rotate(new Vector3(0.0f, 0.0f, (float)angle));
		}
		public void Rotate(Quaternion rot) {
			stack[idxFirstAvail - 1] *= Matrix4x4.Rotate(rot);
		}
		public void RotateDg(Vector3 angle) {
			stack[idxFirstAvail - 1] *= Matrix4x4.Rotate(Quaternion.Euler(angle));
		}
		public void RotateDg(double x, double y, double z) {
			Rotate(new Vector3((float)x, (float)y, (float)z));
		}
		public void RotateXDg(double angle) {
			Rotate(new Vector3((float)angle, 0.0f, 0.0f));
		}
		public void RotateYDg(double angle) {
			Rotate(new Vector3(0.0f, (float)angle, 0.0f));
		}
		public void RotateZDg(double angle) {
			Rotate(new Vector3(0.0f, 0.0f, (float)angle));
		}

		public void Scale(Vector3 scale) {
			stack[idxFirstAvail - 1] *= Matrix4x4.Scale(scale);
		}
		public void Scale(double scaleX, double scaleY, double scaleZ) {
			Scale(new Vector3((float)scaleX, (float)scaleY, (float)scaleZ));
		}

		public Vector3 Transform(Vector3 p) {
			return stack[idxFirstAvail - 1].MultiplyPoint(p);
		}
		public Vector3 Transform(double x, double y, double z = 0.0) {
			return Transform(new Vector3((float)x, (float)y, (float)z));
		}
		public Vector3[] Transform(Vector3[] points) {
			Vector3[] transformedPoints = new Vector3[points.Length];
			for (int i = 0; i < points.Length; i++) {
				transformedPoints[i] = Transform(points[i]);
			}
			return transformedPoints;
		}
		public List<Vector3> Transform(List<Vector3> points) {
			List<Vector3> transformedPoints = new List<Vector3>(points);
			for (int i = 0; i < points.Count; i++) {
				transformedPoints[i] = Transform(transformedPoints[i]);
			}		
			return transformedPoints;
		}

		public Vector3[] TransformInPlace(Vector3[] points) {
			Vector3[] transformedPoints = points;
			for (int i = 0; i < points.Length; i++) {
				transformedPoints[i] = Transform(points[i]);
			}
			return transformedPoints;
		}
		public List<Vector3> TransformInPlace(List<Vector3> points) {
			List<Vector3> transformedPoints = points;
			for (int i = 0; i < points.Count; i++) {
				transformedPoints[i] = Transform(transformedPoints[i]);
			}		
			return transformedPoints;
		}

		public void Save() {
			if (idxFirstAvail < stack.Count) {
				stack[idxFirstAvail] = top;
			} else {
				stack.Add(top);
			}
			idxFirstAvail++;
		}

		public void Restore() {
			if (idxFirstAvail > 1) {
				idxFirstAvail--;
			}
		}

		public void Reset() {
			idxFirstAvail = 1;
			Identity();
		}

		public void Clear() {
			while (stack.Count > 1) {
				stack.RemoveAt(stack.Count - 1);
				idxFirstAvail--;
			}

			idxFirstAvail = 1;
			Identity();
		}

		public override string ToString() {
			string s = "{";
			int len = idxFirstAvail;
			for (int i = 0; i < len; i++) {
				s += "[" + stack[i] + "]";
			}
			s += "}";

			return s;
		}
	}

	public static Action<List<Vector3>> mCurve(MatrixStack m, List<List<Vector3>> curves) {
		return (List<Vector3> strokes) => {
			if (strokes.Count < 2) {
				return;
			}

			curves.Add(m.Transform(strokes));
		};
	}

	public static Action<List<Vector3>> mCurveInPlace(MatrixStack m, List<List<Vector3>> curves) {
		return (List<Vector3> strokes) => {
			if (strokes.Count < 2) {
				return;
			}

			curves.Add(m.TransformInPlace(strokes));
		};
	}
}

public class Choice {
	List<double> weights;
	int stateValue;

	public Choice() {
		weights = new List<double>();
		setState(0);
	}

	public double getValue(int i) {
		return (i >= weights.Count) ? 0 : CTUtil.SCurve(weights[i]);
	}

	public int getState(double n = 0.0) {
		return stateValue;
	}

	public void setState(int n) {
		stateValue = n;
		update();
	}

	public void update(double delta = -1.0) {
		if (delta == -1.0) {
			delta = 0.0;
		}

		while (weights.Count <= stateValue) {
			weights.Add(0);
		}

		for (int i = 0; i < weights.Count; i++) {
			weights[i] =
				(i == stateValue) ? Mathf.Min((float)1, (float)(weights[i] + 2 * delta)) :
								    Mathf.Max((float)0, (float)(weights[i] - delta));
		}
	}
}
	

public class ChalktalkBirdLib {
	private List<List<Vector3>> _curves = new List<List<Vector3>>();
	public List<List<Vector3>> curves {
		get { return _curves; } set {}
	}

	private static int numInstances = 0;

	public readonly int instanceID;

	const double BEFORE_TIME = -1.0;
	double time = BEFORE_TIME;

	double legLength  = 1.0;
	double bodyLength = 1.0;
	double headWidth  = 1.0;
	double headHeight = 1.0;

	double lookUp  = 0.0;
	double pace    = 1.2;
	double turnDir = 0.0;

	Choice moving;
	Choice gazing;

	double walkP     = 0.0;
	double walkT     = 0.0;
	double walkX 	 = 0.0;
	double walkXPrev = 0.0;
	double walkZ     = 0.0;
	double walkZPrev = 0.0;
	double T         = 0.0;
	double t         = 0.0;

	CTUtil.MatrixStack m;

	public readonly Action<List<Vector3>> mCurve;

	public ChalktalkBirdLib() {
		instanceID = ChalktalkBirdLib.numInstances++;
		m = new CTUtil.MatrixStack();
		mCurve = CTUtil.mCurveInPlace(m, _curves);

		moving = new Choice();
		gazing = new Choice();

		doAction(0);
	}

	public void doAction(int idx) {
		switch (idx) {
		case 0:
			this.turnOnWalk();
			break;
		case 1:
			this.toggleGaze();
			break;
		case 2:
			this.toggleGaze();
			break;
		case 4:
			this.turnOnIdle();
			break;
		}
	}

	public void turnOnWalk() {
		moving.setState(2);
		gazing.setState(0);
	}
	public void toggleGaze() {
		gazing.setState(1 - gazing.getState());
	}
	public void turnOnIdle() {
		moving.setState(1);
	}

    public double stretch(string label, double val) {
		return val;
    }


	public List<List<Vector3>> Update(float elapsed, Transform transform) {
		_curves.Clear();
		m.Reset();

		legLength  = stretch("leg length", 2.0 / 0.2);
		bodyLength = stretch("body length", 1.0 / 0.15);
		headWidth  = stretch("head width", 1.0 / 0.15);
		headHeight = stretch("head height", 1.0 / 0.1);

		moving.update((double)elapsed);
		gazing.update((double)elapsed);
		double gaze = gazing.getValue(1);
		double idle = moving.getValue(1);
		double walk = moving.getValue(2);
	
		int state = moving.getState();

		switch (state) {
		case 1:
		case 2:
			if (time == BEFORE_TIME) {
				time = (double)Time.time;
				this.T = instanceID * 3617 * Mathf.PerlinNoise((float)(ChalktalkBirdLib.numInstances * 2069.0), (float)(Time.time * ChalktalkBirdLib.numInstances * 1489.0));
				walkT = 0.0;
			}
			break;
		}

		
		// LEFT AND RIGHT FOOT ARE DISPLACED IN PHASE.

		double[] TFoot = { CTUtil.Saw(walkP) / 2 - 0.5, CTUtil.Saw(walkP + 0.5) / 2 - 0.5};
	
        // WALKING MOVEMENT IS DRIVEN BY SINUSOIDAL WAVES

		double s2 = 0;
		double c2 = 1;
		double s4 = 0;
		double c4 = 1;
		
		if (walk > 0) {
			double phase = Mathf.Abs((float)pace) * 2 * (double)Mathf.PI * walkT;
			s2 = Mathf.Sin((float)phase);
			c2 = Mathf.Cos((float)phase);
			s4 = Mathf.Sin((float)(2 * phase));
			c4 = Mathf.Cos((float)(2 * phase));
		}

		// BODY PROPORTIONS

		double footY     = -1.2;
        double spineBase = footY + legLength;
        double spineTop  = spineBase + 0.8 * bodyLength;
		double uLeg      = (spineBase - footY) / 1.2;
		double lLeg      = 0.9 * uLeg;

		// PARAMETERS THAT CONTROL BODY LANGUAGE.

		double liftFoot = 0;
		double lookUp   = 0.2      + 1.5 * /*CTUtil.Noise2(this.T / 3.0, 10)*/ Mathf.PerlinNoise((float)(this.T / 3.0), (float)10);
		double lookSide = 0.0      + 3.0 * /*CTUtil.Noise2(this.T / 3.0, 20)*/ Mathf.PerlinNoise((float)(this.T / 3.0), (float)20);
		double bounce   = 0.1 * s4 + 0.2 * /*CTUtil.Noise2(this.T / 1.5, 30)*/ Mathf.PerlinNoise((float)(this.T / 1.5), (float)30);
		double bob      = 0.1 * c4 + 0.3 * /*CTUtil.Noise2(this.T / 1.5, 40)*/ Mathf.PerlinNoise((float)(this.T / 1.5), (float)40);

		Vector3 screen = Camera.main.WorldToScreenPoint(Camera.main.transform.position);
		// TODO this.lookUp = -Mathf.Atan2(Screen.height - screen.y, Mathf.Max(0.0f, Screen.width - screen.x));


		// COMPUTED VALUES.

		double hipX = 0;
		double hipY = spineBase + bounce;
		double neckX = bob - .3 * lookUp - .1;
		double neckY = spineTop + bounce;

		// SET CURRENT POSITION AND ORIENTATION OF BIRD WITHIN THE WORLD.
		if (moving.getState() == 2) {
			m.Translate(-100 + (this.walkX) % 225, 0.0f, this.walkZ);
		} else {
			m.Translate(this.walkX, 0.0f, this.walkZ);
		}
		//transform.Translate((float)this.walkX, 0.0f, (float)this.walkZ);
		//transform.Translate(transform.TransformPoint(new Vector3((float)-this.walkX, 0.0f, 0.0f)).x * transform.lossyScale.x, 0, transform.TransformPoint(new Vector3(0.0f, 0.0f, (float)-this.walkZ)).z * transform.lossyScale.z);
		m.RotateY(this.turnDir);

      	// DRAW HEAD


		const float OFFSET = 0.0f;
		m.Save();
			m.RotateDg(transform.rotation.eulerAngles);
			m.Translate(neckX, neckY, 0);
			bool isAbove = Camera.main.transform.position.y - transform.position.y > OFFSET;
			Vector3 transVec = transform.InverseTransformDirection(transform.position);
			m.RotateX(-.5 * CTUtil.Lerp(gaze, lookUp, this.lookUp - ((isAbove) ? 100.0 : -100.0) * transform.InverseTransformPoint(Camera.main.transform.position).y));


		// TEMP		m.RotateX(-.5 * CTUtil.Lerp(gaze, lookUp, this.lookUp));


		Quaternion rot = Quaternion.LookRotation(
			transform.InverseTransformPoint(Camera.main.transform.position) + 
			transform.InverseTransformPoint(transform.right)
		);
		Vector3 euler = rot.eulerAngles;
		//euler.x = Mathf.Clamp(euler.x, -90.0f, 90.0f);
		euler.y = Mathf.Clamp(euler.y, -90.0f, 90.0f);
		//euler.z = Mathf.Clamp(euler.z, -90.0f, 90.0f);
		euler.x *= -0.20f;
		euler.y *= 0.25f;
		euler.z = 0.12f;


		m.Rotate(euler);
		//			Vector3 v = Camera.main.transform.position + transform.right;
//			Quaternion rot = Quaternion.LookRotation(v);
//			Vector3 euler = rot.eulerAngles;
//			//UnityEngine.Debug.Log(euler);
//			euler.x = 0.0f;
//			//euler.y = Mathf.Clamp(euler.y, 0.0f, 135.0f);
//			m.RotateDg(-euler);
//			m.RotateYDg((-180.0 / 6));
		//	UnityEngine.Debug.Log(Vector3.Angle(-transform.right, Camera.main.transform.forward));
		//

			m.RotateY(lookSide);
			m.Scale(this.headWidth, this.headHeight, 1.0f);

			List<Vector3> head = new List<Vector3>(new Vector3[]{
				new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.8f, 0.3f, 0.0f),
				new Vector3(0.0f, 0.6f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)
			});
			
			mCurve(head);

		m.Restore();

		// DRAW SPINE

		List<Vector3> spine = new List<Vector3>();
		double arching = .1;
		for (double t = 0.0; t <= 1.0; t += (double)1/(double)10) {
			double arch = arching * Mathf.Sin((float)(Mathf.PI * t));
			spine.Add(new Vector3(
				(float)(CTUtil.Lerp(t, neckX, hipX) + arch * (1 - 2.7 * bounce)),
				(float)(CTUtil.Lerp(t, neckY, hipY)),
				(float)0
			));
		}
		mCurve(spine);

		// DRAW LEGS
		if (true) {
			for (int n = 0; n < 2; n++) {
				double footLift = (double)Mathf.Max(0, (float)((n == 0 ? .3 : -.3) * s2));
				Vector3 Foot = new Vector3(
					              (float)(-hipX + TFoot[n] * 2.4 * uLeg + .15), 
					              (float)(footY - hipY + footLift), 
					              (float)0
				              );

				Vector3 Knee = new Vector3(-1, 0, 0);
				Knee = CTUtil.Ik(uLeg, lLeg, Foot, ref Knee);
				Foot[2] = (float)((n == 0 ? .3 : -.3) * lLeg);
				Knee[2] = Foot[2];

				List<Vector3> c = new List<Vector3>();
				m.Save();
					m.Identity();
					m.Translate(hipX, hipY, 0);

					c.Add(m.Transform(Vector3.zero));
					c.Add(m.Transform(new Vector3(0, 0, Knee[2])));
					c.Add(m.Transform(Knee));
					c.Add(m.Transform(Foot));
					m.Save();
						m.Translate(Foot[0], Foot[1], Foot[2]);
						m.RotateZ(.6 * footLift);
						c.Add(m.Transform(Vector3.zero));
						c.Add(m.Transform(new Vector3((float).3, 0, 0)));
					m.Restore();
				m.Restore();
				mCurve(c);
			}
		}

		// ADVANCE TIME.

		switch (state) {
		case 1:
		case 2:
			double dt = this.pace < 0 ? this.time - Time.time : Time.time - this.time;
			T += dt;
			if (state == 2) {
				this.walkP += dt * this.pace;
				this.walkT += dt;
				this.walkX +=  Mathf.Cos((float)this.turnDir) * dt * 2.4 * uLeg * Mathf.Abs((float)this.pace);
				this.walkZ += -Mathf.Sin((float)this.turnDir) * dt * 2.4 * uLeg * Mathf.Abs((float)this.pace);
			}
			this.time = (double)Time.time;
			break;
		}

		return _curves;
	}
}


}
