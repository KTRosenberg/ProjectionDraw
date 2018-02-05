using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam.Tools;

[ExecuteInEditMode]
public class GVRBuildTarget : MonoBehaviour {

  Dictionary<int, string> targets = new Dictionary<int, string>() {
    {0 , "NONE" },
    {1 , "M1" },
    {2 , "M2" },
    {3 , "M3" },
    {4 , "M4" },
  };

  private GVRViveHeadset headset;

  void Start() {
    headset = this.GetComponent<GVRViveHeadset>();
  }

	// Update is called once per frame
	void Update () {
    headset.label = targets[BuildManager.BUILD_INDEX];
    headset.targetTransform = null;
	}
}
