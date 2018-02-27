using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FRL.IO;

namespace FRL2 {

public class ReferenceMultipleGeometry : MonoBehaviour,
IReferenceGeometry,
IGlobalTouchpadPressSetHandler,
IGlobalTouchpadTouchSetHandler,
IGlobalTriggerPressSetHandler,
IGlobalGripPressUpHandler,
IGlobalGripPressHandler,
IGlobalGripPressDownHandler
{	
	public ViveControllerModule leftModule;
	public ViveControllerModule rightModule;

	public bool isInitialized { get; private set; }
	public bool isActive { get { return this.currActiveState; } }

	[HideInInspector]
	public bool stateIsModified;
	[HideInInspector]
	public bool currActiveState;

	public List<ProjectionCurveContainer> auxCurves;

	public ProjectionCurveContainer _auxCurve;
	public ProjectionCurveContainer auxCurve { get { return _auxCurve; } }

	public int Count { 
		get { 
			if (_auxCurve == null) {
				return 0;
			} 
			return _auxCurve.Count; 
		} 
	}
	// probably will want to use a singleton class for a static list
	// to which we can add draw reference objects
	public  List<GameObject> refGeometryPrefabList;
	private List<GameObject> _refGeometryList;
	private List<Quaternion> _refGeometryRotationIdents;
	private int _activeRefGeometryIdx;
	List<List<GameObject>> _refGeometryInstances;

	private bool _isRotating;
	private float _rotationSpeed;
	private Vector3 _rotationAxis;

	public void Awake() {
		_refGeometryList = new List<GameObject>();
		_refGeometryRotationIdents = new List<Quaternion>();
		_refGeometryInstances = new List<List<GameObject>>();

		foreach (GameObject prefab in refGeometryPrefabList) {
			GameObject inst = Instantiate(prefab);
			inst.SetActive(false);
			_refGeometryList.Add(inst);
			_refGeometryRotationIdents.Add(_refGeometryList[_refGeometryList.Count - 1].transform.rotation);
			_refGeometryInstances.Add(new List<GameObject>());
		}
	}

	Func<ProjectionCurveContainer> MakeDrawCurve;
	public void Init(ProjectionCurveContainer mc, Func<IMultiCurve> mcGen, DrawMode drawMode = DrawMode.UNITY_LINE_RENDERER) {
		if (isInitialized) {
			return;
		}

		auxCurves = new List<ProjectionCurveContainer>();

		MakeDrawCurve = () => {
			ProjectionCurveContainer c = UnityEngine.Object.Instantiate(mc);
			c.name = "AUX_DRAW_CURVE_" + auxCurves.Count; 
			c.Init(MultiCurveFactoryStatic.Curve(DrawFunctions.DrawWithLineRenderer), drawMode);
			return c;
		};
		_auxCurve = MakeDrawCurve();
		auxCurves.Add(_auxCurve);

		_activeRefGeometryIdx = 0;

		_isRotating = false;
		_rotationSpeed = 60.0f;
		_rotationAxis = Vector3.up;

		this.stateIsModified = false;
		this.currActiveState = false;

	}


	public void Update() {
		if (!isActive) {
			return;
		}

		if (!_isRotating) {
			return;
		}

		List<GameObject> geometryList = _refGeometryInstances[_activeRefGeometryIdx];
		if (geometryList.Count < 1) {
			return;
		}
		GameObject refGeometry = geometryList[geometryList.Count - 1];

		refGeometry.transform.RotateAround(refGeometry.transform.position, _rotationAxis, Time.deltaTime * _rotationSpeed);
		_auxCurve.transform.RotateAround(refGeometry.transform.position, _rotationAxis, Time.deltaTime * _rotationSpeed);
	}

	private float _cycleStartTime;
	private float _selectionCycleDuration = 1.0f;
	private bool _touchpadPressedOn = false;
	private bool _rotationControlOn = false;
	private Vector3 _onPosition;

	// NOTE: CURRENTLY IN HOTEL CALIFORNIA MODE, YOU CAN ENTER THIS MODE, BUT YOU MAY NEVER LEAVE ( TODO improve controls)
	bool CANT_LEAVE = true;
	void IGlobalTouchpadPressDownHandler.OnGlobalTouchpadPressDown(VREventData eventData) {
		if (eventData.module != rightModule) {
			return;
		}
		if (currActiveState && CANT_LEAVE) {
			return;
		}
		// toggle active state
		this.currActiveState = !this.currActiveState;
		this.stateIsModified = true;
	}
	void IGlobalTouchpadPressHandler.OnGlobalTouchpadPress(VREventData eventData) {
		if (CANT_LEAVE) {
			return;
		}

		if (!_touchpadPressedOn) {
			GameObject refGeometry = _refGeometryList[_activeRefGeometryIdx];
			refGeometry.SetActive(false);

			_activeRefGeometryIdx = 0;

			_rotationControlOn = false;
			_isRotating = false;
			return;
		}
	}
	void IGlobalTouchpadPressUpHandler.OnGlobalTouchpadPressUp(VREventData eventData) {
		_rotationControlOn = isActive && _touchpadPressedOn;
	}

	// touch controls for touchpad

	public float Atan2_2PI(float angle_1PI) {
		if (angle_1PI < 0.0) {
			angle_1PI = (float)((double)Mathf.PI + ((double)Mathf.PI - (double)-angle_1PI)); 
		}
		return angle_1PI;
	}
	public float Atan2_2PI(float y, float x) {
		float angle = Mathf.Atan2(y, x);
		if (angle < 0.0) {
			angle = (float)((double)Mathf.PI + ((double)Mathf.PI - (double)-angle)); 
		}
		return angle;
	}

	private const int NUM_SECTORS = 4;
	private const float RADIANS_PER_SECTOR = (2 * Mathf.PI) / (float)NUM_SECTORS;
	private Vector3[] axes = { 
		Vector3.down,  // left
		Vector3.right, // up
		Vector3.up,    // right
		Vector3.left   // down
	};

	private Vector3 GetGeometryRotationAxis(Vector2 axis) {
		double angle = (double)Atan2_2PI(Mathf.Atan2(axis.y, axis.x) + (RADIANS_PER_SECTOR / 2));
		return axes[(int)((angle * NUM_SECTORS) / (2 * Mathf.PI))];
	}

	void IGlobalTouchpadTouchDownHandler.OnGlobalTouchpadTouchDown(VREventData eventData) {
		if (eventData.module != leftModule) {
			return;
		}
			
		_isRotating = !_isRotating;

	}
	void IGlobalTouchpadTouchHandler.OnGlobalTouchpadTouch(VREventData eventData) {
		if (eventData.module != leftModule) {
			return;
		}

		if (_isRotating) {
			_rotationAxis = GetGeometryRotationAxis(eventData.touchpadAxis);
		}
	}
	void IGlobalTouchpadTouchUpHandler.OnGlobalTouchpadTouchUp(VREventData eventData) {
	}







	//////////////////////////////////////////////////////////////////////////////////
	void IGlobalTriggerPressDownHandler.OnGlobalTriggerPressDown(VREventData eventData) {
		if (!isActive) {
			return;
		}
		if (eventData.module != leftModule) {
			return;
		}

		// toggle geometry reference object on state
		_refGeometryList[_activeRefGeometryIdx].SetActive(isActive);

		_rotationControlOn = true;

//		if (!isActive) {
//			// reset
//			for (int i = 0; i < _refGeometryList.Count; i++) {
//				_refGeometryList[i].transform.rotation = _refGeometryRotationIdents[i];
//			}
//			_activeRefGeometryIdx = 0;
//			_touchpadPressedOn = false;
//			return;
//		}
		_touchpadPressedOn = true;

		// show geometry reference object
		GameObject refGeometry = _refGeometryList[_activeRefGeometryIdx];
		refGeometry.transform.position = eventData.module.transform.position + (Camera.main.transform.rotation * Vector3.forward);

		_onPosition = refGeometry.transform.position;

		ProjectionCurveContainer c = MakeDrawCurve();
		_auxCurve = c;
		auxCurves.Add(c);

		stateIsModified = true;

		_auxCurve.transform.position = refGeometry.transform.position;

		_cycleStartTime = Time.time;


	}

	void IGlobalTriggerPressHandler.OnGlobalTriggerPress(VREventData eventData) {
		if (!isActive) {
			return;
		}
		if (eventData.module != leftModule) {
			return;
		}

		double elapsed = Time.time - _cycleStartTime;
		// move to next geometry in list
		if (elapsed >= _selectionCycleDuration) {
			_cycleStartTime = (float)(Time.time + (elapsed - _selectionCycleDuration));
			GameObject refGeometry = _refGeometryList[_activeRefGeometryIdx];
			refGeometry.SetActive(false);

			_activeRefGeometryIdx = (_activeRefGeometryIdx + 1) % _refGeometryList.Count;

			refGeometry = _refGeometryList[_activeRefGeometryIdx];
			refGeometry.SetActive(true);
			refGeometry.transform.position = _onPosition;
		}
	}

	GameObject latestRef = null;
	void IGlobalTriggerPressUpHandler.OnGlobalTriggerPressUp(VREventData eventData) {
		if (!isActive) {
			return;
		}
		if (eventData.module != leftModule) {
			return;
		}
		GameObject refGeometry = _refGeometryList[_activeRefGeometryIdx];

		latestRef = Instantiate(_refGeometryList[_activeRefGeometryIdx]);
		_refGeometryInstances[_activeRefGeometryIdx].Add(latestRef);

		_onPosition = latestRef.transform.position;
		_auxCurve.transform.position = latestRef.transform.position;
		_cycleStartTime = Time.time;

		refGeometry.SetActive(false);
	}

	void IGlobalGripPressDownHandler.OnGlobalGripPressDown(VREventData eventData) {
		if (eventData.module != leftModule) {
			return;
		}
		foreach (List<GameObject> L in _refGeometryInstances) {
			foreach (GameObject gRef in L) {
				UnityEngine.Object.Destroy(gRef);
			}
			L.Clear();
		}
		// TODO merge all curves into one 
	}
	void IGlobalGripPressHandler.OnGlobalGripPress(VREventData eventData) {
	}
	void IGlobalGripPressUpHandler.OnGlobalGripPressUp(VREventData eventData) {
	}
}

}