using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour {

	public Transform goal;
	public float moveSpeed = 1f;
	public float lerpVal = 1.5f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (goal) {
			Quaternion old = transform.rotation;
			transform.LookAt(goal.position);
			Quaternion newest = transform.rotation;
			transform.rotation = Quaternion.Slerp(old, newest, lerpVal * Time.deltaTime);
			this.transform.position = this.transform.position + this.transform.forward * Time.deltaTime * moveSpeed;
		}
	}
}
