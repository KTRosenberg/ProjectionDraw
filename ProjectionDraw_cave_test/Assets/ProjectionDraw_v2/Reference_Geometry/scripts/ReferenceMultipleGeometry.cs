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
	List<GameObject> refGeometryInstances;
	int entityIdx = 0;

	private bool _isRotating;
	private float _rotationSpeed;
	private Vector3 _rotationAxis;

	public void Awake() {
		_refGeometryList = new List<GameObject>();
		_refGeometryRotationIdents = new List<Quaternion>();
		refGeometryInstances = new List<GameObject>();

		foreach (GameObject prefab in refGeometryPrefabList) {
			GameObject inst = Instantiate(prefab);
			inst.SetActive(false);
			_refGeometryList.Add(inst);
			_refGeometryRotationIdents.Add(_refGeometryList[_refGeometryList.Count - 1].transform.rotation);
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

		///////// TEMP, ALWAYS ACTIVE ON START
		this.currActiveState = !this.currActiveState;
		this.stateIsModified = true;
		/////////

	}


	public void Update() {
		if (!isActive) {
			return;
		}

		if (!_isRotating) {
			return;
		}

		if (refGeometryInstances.Count < 1) {
			return;
		}
			
		GameObject refGeometry = activeRef;

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


	// REFERENCE GEOMETRY SELECTION
	static int referenceGeometryLayer = 8;
	static int layerMask = 1 << referenceGeometryLayer;

	static GameObject SelectReferenceGeometryWithRaycast(
		Camera origin, 
		Vector3 pointWorld,
		bool allowBehindSurface = false,
		bool allowBehindOrigin = false
	) {
		Vector3 sPos = origin.WorldToScreenPoint(pointWorld);
		Ray r = origin.ScreenPointToRay(sPos);
		RaycastHit hit;
		GameObject referenceGeometry = null;

		Debug.DrawRay(r.origin, r.direction * 20.0f, Color.blue);

		if (Physics.Raycast(r, out hit, Mathf.Infinity, layerMask)) {
			referenceGeometry = hit.collider.gameObject;
		}

		return referenceGeometry;
	}
		
	void IGlobalTouchpadPressHandler.OnGlobalTouchpadPress(VREventData eventData) {
		if (eventData.module != leftModule) {
			return;
		}
		if (isCreatingReferenceGeometry) {
			return;
		}

		Vector3 p = eventData.module.transform.position;
		GameObject refG = SelectReferenceGeometryWithRaycast(Camera.main, p);

		if (refG == null) {
			return;
		}

		// TODO assign rotating state to each reference geometry
		_isRotating = false;
		_auxCurve = auxCurves[(int)Char.GetNumericValue(refG.name[0])];
		activeRef = refG;

		this.stateIsModified = true;
	}
	void IGlobalTouchpadPressUpHandler.OnGlobalTouchpadPressUp(VREventData eventData) {
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
	bool isCreatingReferenceGeometry = false;

	void IGlobalTriggerPressDownHandler.OnGlobalTriggerPressDown(VREventData eventData) {
		if (!isActive) {
			return;
		}
		if (eventData.module != leftModule) {
			return;
		}

		isCreatingReferenceGeometry = true;

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

		_auxCurve.transform.position = refGeometry.transform.position;

		_cycleStartTime = Time.time;

		stateIsModified = true;
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

	GameObject activeRef = null;
	void IGlobalTriggerPressUpHandler.OnGlobalTriggerPressUp(VREventData eventData) {
		if (!isActive) {
			return;
		}
		if (eventData.module != leftModule) {
			return;
		}
		GameObject refGeometry = _refGeometryList[_activeRefGeometryIdx];

		activeRef = Instantiate(_refGeometryList[_activeRefGeometryIdx]);
		int nextIdx = ++entityIdx;
		activeRef.name = nextIdx.ToString() + "_" + refGeometry.name;

		refGeometryInstances.Add(activeRef);

		_onPosition = activeRef.transform.position;
		_auxCurve.transform.position = activeRef.transform.position;
		_cycleStartTime = Time.time;

		refGeometry.SetActive(false);

		isCreatingReferenceGeometry = false;
	}

	void IGlobalGripPressDownHandler.OnGlobalGripPressDown(VREventData eventData) {
		if (eventData.module == leftModule) {
			foreach (GameObject refG in refGeometryInstances) {
				UnityEngine.Object.Destroy(refG);
			}
			refGeometryInstances.Clear();
			_isRotating = false;
		} else if (eventData.module == rightModule) {
			foreach (ProjectionCurveContainer c in auxCurves) {
				c.Clear();
			}
		}
		// TODO merge all curves into one 
	}
	void IGlobalGripPressHandler.OnGlobalGripPress(VREventData eventData) {
	}
	void IGlobalGripPressUpHandler.OnGlobalGripPressUp(VREventData eventData) {
	}
}

}