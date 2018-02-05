//#define DO_DEBUG_PRINT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

public class ProjectionDrawInputHandler : 
	MonoBehaviour, 
	IGlobalTriggerPressSetHandler/*, 
	IGlobalTouchpadTouchSetHandler*/ {
	
	public enum InputDeviceOpt {
		NONE = 0,
		MOUSE = 1,
		VIVE  = 2
	}
	public InputDeviceOpt inputType;

	// TODO temp
	public bool useViveWithHeadMovement = false;

	public struct InputStatus {
		public bool    strokeStart;
		public bool    strokeDuring;
		public bool    strokeEnd;
		public Vector3 pos;

		public override string ToString() {
			return string.Format("[InputStatus : strokeStart=" + strokeStart + 
				", strokeEnd=" + strokeEnd + 
				", pos=" + pos);
		}
	}

	private InputStatus _status;
	private InputDeviceOpt dev;

	public void Awake() {
		this.dev = inputType;
		_status = new InputStatus();
	}

	public void Start() {
		// temp
		//if (this.dev == InputDeviceOpt.VIVE && useViveWithHeadMovement && gazeLinePrefab != null) {
		//	gazeLine = Object.Instantiate(gazeLinePrefab);
		//	gazeLine.positionCount = 2;
		//}
	}

	public void SwitchInputDeviceOpt(InputDeviceOpt dev) {
		this.dev = dev;
		_status = new InputStatus();
	}

	public InputStatus updateInputStatus() {
		InputStatus toReturn;
		switch (dev) {
		case InputDeviceOpt.NONE:
			toReturn = new InputStatus();
			break;
		case InputDeviceOpt.MOUSE:
			_status.strokeStart = Input.GetMouseButtonDown(0);
			_status.strokeDuring = Input.GetMouseButton(0);
			_status.strokeEnd = Input.GetMouseButtonUp(0);
			_status.pos = Input.mousePosition;
			toReturn = _status;
			break;
		case InputDeviceOpt.VIVE:
			toReturn = new InputStatus {
				strokeStart = _status.strokeStart,
				strokeEnd = _status.strokeEnd,
				pos = _status.pos
			};
			_status.strokeStart = false;
			_status.strokeEnd = false;
			break;
		default:
			toReturn = new InputStatus();
			break;
		}

		return toReturn;
	}

	// Vive Handlers
	const float MULT = 20.0f;
	void IGlobalTriggerPressDownHandler.OnGlobalTriggerPressDown(VREventData eventData) {
		if (dev != InputDeviceOpt.VIVE) {
			return;
		}

		_status.strokeStart = true;
		_status.strokeEnd = false;

		if (useViveWithHeadMovement) {
			_status.pos = Camera.main.WorldToScreenPoint(
				Camera.main.transform.position + Vector3.Scale(Camera.main.transform.forward, new Vector3(MULT, MULT, MULT))
			);
		} else {
			_status.pos = Camera.main.WorldToScreenPoint(eventData.module.transform.position);

			// TEMP
//			Vector3 wee = Camera.main.WorldToScreenPoint(eventData.module.transform.position);
//			Ray r = Camera.main.ScreenPointToRay(wee);
//			RaycastHit hit;
//			if (Physics.Raycast(r, out hit)) {
//				targetIndicator.SetActive(true);
//				targetIndicator.transform.position = hit.point;
//			} else {
//				targetIndicator.SetActive(false);
//			}
			//
		}
	}
	void IGlobalTriggerPressHandler.OnGlobalTriggerPress(VREventData eventData) {
		if (dev != InputDeviceOpt.VIVE) {
			return;
		}
			
		_status.strokeStart = false;
		_status.strokeEnd = false;

		if (useViveWithHeadMovement) {
			_status.pos = Camera.main.WorldToScreenPoint(
				Camera.main.transform.position + Vector3.Scale(Camera.main.transform.forward, new Vector3(MULT, MULT, MULT))
			);
		} else {
			_status.pos = Camera.main.WorldToScreenPoint(eventData.module.transform.position);

			// TEMP
//			Vector3 wee = Camera.main.WorldToScreenPoint(eventData.module.transform.position);
//			Ray r = Camera.main.ScreenPointToRay(wee);
//			Debug.DrawRay(r.origin, r.direction + r.direction);
//			RaycastHit hit;
//			if (Physics.Raycast(r, out hit)) {
//				targetIndicator.SetActive(true);
//				targetIndicator.transform.position = hit.point;
//			} else {
//				targetIndicator.SetActive(false);
//			}
			//

		}
	}

	void IGlobalTriggerPressUpHandler.OnGlobalTriggerPressUp(VREventData eventData) {
		if (dev != InputDeviceOpt.VIVE) {
			return;
		}

		_status.strokeStart = false;
		_status.strokeEnd = true;
	}
	/*
	double MapRange(double val, double a1, double a2, double b1, double b2) {
		return b1 + (val - a1) * (b2 - b1) / (a2 - a1);
	}

	uint RadiansToRGB(double val) {
		return (uint)((val * rgbMax) / (2 * Mathf.PI));
	}

	const uint rgbMax = 0xFFFFFF;

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

	public Color c;
	void CalcColor(VREventData eventData) {
		// TODO
		return;

		Vector2 axis = eventData.touchpadAxis;
		double angle = (double)Atan2_2PI(Mathf.Atan2(axis.y, axis.x) - (Mathf.PI / 2));
		Debug.Log(angle);

		uint colorVal = RadiansToRGB(angle);
		uint r = (colorVal >> 16) & 255;
		uint g = (colorVal >>  8) & 255;
		uint b =  colorVal        & 255;
		Debug.Log(colorVal + " / " + rgbMax);
		Debug.Log("[" + r + ", " + g + ", " + b + "]");
		c = new Color(r, g, b);
	}

	void IGlobalTouchpadTouchDownHandler.OnGlobalTouchpadTouchDown(VREventData eventData) {
		if (dev != InputDeviceOpt.VIVE) {
			return;
		}

		CalcColor(eventData);
	}
	void IGlobalTouchpadTouchHandler.OnGlobalTouchpadTouch(VREventData eventData) {
		if (dev != InputDeviceOpt.VIVE) {
			return;
		}

		//CalcColor(eventData);
	}
	void IGlobalTouchpadTouchUpHandler.OnGlobalTouchpadTouchUp(VREventData eventData) {
		if (dev != InputDeviceOpt.VIVE) {
			return;
		}

		//CalcColor(eventData);
	}
	*/

	//public LineRenderer gazeLinePrefab;
	//private LineRenderer gazeLine;
	//private Vector3[] gazeLinePoints = new Vector3[2];
	private bool strokeDuring = false;
	public GameObject targetIndicator;

	void Update() {
		if (dev != InputDeviceOpt.VIVE /*|| gazeLine == null*/) {
			return;
		}
		if (_status.strokeStart) {
			//gazeLine.positionCount = 0;
			strokeDuring = true;
			targetIndicator.SetActive(false);
		} else if (_status.strokeEnd) {
			strokeDuring = false;
		} else if (!strokeDuring && useViveWithHeadMovement) {
				//gazeLine.positionCount = 2;
			Vector3 screenPos = Camera.main.WorldToScreenPoint(
				Camera.main.transform.position + Vector3.Scale(Camera.main.transform.forward, new Vector3(MULT, MULT, MULT))
			);
				                    
			Ray r = Camera.main.ScreenPointToRay(screenPos);

			RaycastHit hit;
			if (Physics.Raycast(r, out hit)) {
				targetIndicator.SetActive(true);
				targetIndicator.transform.position = hit.point;
				//Debug.DrawLine(gazeLinePoints[0], gazeLinePoints[1], Color.green);
				//gazeLine.SetPositions(gazeLinePoints);
			} else {
				targetIndicator.SetActive(false);
			}


		}
	}


}
