using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FRL.IO;

namespace FRL2 {

public class ReferenceSingleGeometry : MonoBehaviour,
	IReferenceGeometry,
	IGlobalTouchpadPressSetHandler,
	IGlobalTouchpadTouchSetHandler
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

	private ProjectionCurveContainer _auxCurve;
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

	private bool _isRotating;
	private float _rotationSpeed;
	private Vector3 _rotationAxis;

	public void Awake() {
		_refGeometryList = new List<GameObject>();
		_refGeometryRotationIdents = new List<Quaternion>();
		foreach (GameObject prefab in refGeometryPrefabList) {
			GameObject inst = Instantiate(prefab);
			inst.SetActive(false);
			_refGeometryList.Add(inst);
			_refGeometryRotationIdents.Add(_refGeometryList[_refGeometryList.Count - 1].transform.rotation);
		}
	}

	public void Init(ProjectionCurveContainer mc, Func<IMultiCurve> mcGen, DrawMode drawMode = DrawMode.UNITY_LINE_RENDERER) {
		if (isInitialized) {
			return;
		}
		_auxCurve = UnityEngine.Object.Instantiate(mc);
		_auxCurve.name = "AUX_DRAW_CURVE";
		_auxCurve.Init(mcGen, drawMode);
		_auxCurve.gameObject.SetActive(false);

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
		GameObject refGeometry = _refGeometryList[_activeRefGeometryIdx];
		if (!_isRotating) {
			return;
		}
		refGeometry.transform.RotateAround(refGeometry.transform.position, _rotationAxis, Time.deltaTime * _rotationSpeed);
		_auxCurve.transform.RotateAround(refGeometry.transform.position, _rotationAxis, Time.deltaTime * _rotationSpeed);
	}

	private float _cycleStartTime;
	private float _selectionCycleDuration = 1.0f;
	private bool _touchpadPressedOn = false;
	private bool _rotationControlOn = false;
	private Vector3 _onPosition;

	void IGlobalTouchpadPressDownHandler.OnGlobalTouchpadPressDown(VREventData eventData) {
		// toggle active state
		this.currActiveState = !this.currActiveState;
		this.stateIsModified = true;

		// toggle auxiliary curve on state
		_auxCurve.gameObject.SetActive(isActive);
		// toggle geometry reference object on state
		_refGeometryList[_activeRefGeometryIdx].SetActive(isActive);

		_rotationControlOn = false;

		if (!isActive) {
			// reset
			for (int i = 0; i < _refGeometryList.Count; i++) {
				_refGeometryList[i].transform.rotation = _refGeometryRotationIdents[i];
			}
			_activeRefGeometryIdx = 0;
			_touchpadPressedOn = false;
			return;
		}
		_touchpadPressedOn = true;

		// show geometry reference object
		GameObject refGeometry = _refGeometryList[_activeRefGeometryIdx];
		refGeometry.transform.position = eventData.module.transform.position + (Camera.main.transform.rotation * Vector3.forward);

		_onPosition = refGeometry.transform.position;

		_auxCurve.transform.position = refGeometry.transform.position;

		_cycleStartTime = Time.time;
	}
	void IGlobalTouchpadPressHandler.OnGlobalTouchpadPress(VREventData eventData) {
		if (!_touchpadPressedOn) {
			GameObject refGeometry = _refGeometryList[_activeRefGeometryIdx];
			refGeometry.SetActive(false);

			_activeRefGeometryIdx = 0;

			_rotationControlOn = false;
			_isRotating = false;
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
		if (!_rotationControlOn ) {
			return;
		}

		_isRotating = !_isRotating;
	}
	void IGlobalTouchpadTouchHandler.OnGlobalTouchpadTouch(VREventData eventData) {
		if (_isRotating) {
			_rotationAxis = GetGeometryRotationAxis(eventData.touchpadAxis);
		}
	}
	void IGlobalTouchpadTouchUpHandler.OnGlobalTouchpadTouchUp(VREventData eventData) {
	}
}

}