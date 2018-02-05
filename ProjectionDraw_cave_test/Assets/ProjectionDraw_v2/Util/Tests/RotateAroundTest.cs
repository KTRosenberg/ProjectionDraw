//#define DO_DEBUG_PRINT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundTest : MonoBehaviour {
	public Camera origin;
	public GameObject target;
	public bool isActive;
	public float speed;
	public Vector3 axis = Vector3.up;

	public void Update() {
		if (Input.GetKeyDown(KeyCode.R)) {
			isActive = !isActive;
		}
		if (isActive && origin != null && target != null) {
			origin.transform.RotateAround(
				target.transform.position,
				axis,
				speed * Time.deltaTime
			);
		}
	}
}
