using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRL_KTR {

public class StackLoop : MonoBehaviour {
	CTUtil.MatrixStack M;
	void Awake() {
		M = new CTUtil.MatrixStack();
	}

	private bool isRotating = false;

	void Update() {
		if (Input.GetKeyDown(KeyCode.S)) {
			Debug.Log("SAVE");
			M.Save();
		} else if (Input.GetKeyDown(KeyCode.R)) {
			Debug.Log("RESTORE");
			M.Restore();
		} else if (Input.GetKeyDown(KeyCode.T)) {
			Debug.Log("TRANSFORM");
			isRotating = !isRotating;
		} else if (Input.GetKeyDown(KeyCode.P)) {
			Debug.Log(M);
		} else if (Input.GetKeyDown(KeyCode.C)) {
			Debug.Log(M.Count);
		}

		if (isRotating) {
			M.RotateZ(1.0 * Time.time);
			Debug.Log(M);
		}
	}
}

}
