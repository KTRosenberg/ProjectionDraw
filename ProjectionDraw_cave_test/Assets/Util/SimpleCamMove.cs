using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamMove : MonoBehaviour {
	public Camera origin;
	public float speed;
	// Update is called once per frame
	void LateUpdate () {
		float[] keys = {
			(Input.GetKey(KeyCode.UpArrow) ? 1.0f : 0.0f),
			(Input.GetKey(KeyCode.DownArrow) ? 1.0f : 0.0f),
			(Input.GetKey(KeyCode.RightArrow) ? 1.0f : 0.0f),
			(Input.GetKey(KeyCode.LeftArrow) ? 1.0f : 0.0f)
		};

		Vector3 newPos = origin.transform.position;
		Vector3 f = origin.transform.forward;
		Vector3 d = -f;
		Vector3 r = origin.transform.right;
		Vector3 l = -r;

		Vector3[] dir = { f, d, r, l };
		for (int i = 0; i < keys.Length; i++) {
			newPos += (keys[i] * dir[i] * speed);
		}

		origin.transform.position = newPos;
	}
}
