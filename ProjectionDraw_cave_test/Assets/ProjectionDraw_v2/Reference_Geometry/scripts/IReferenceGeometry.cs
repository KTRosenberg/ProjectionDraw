using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FRL2 {

public interface IReferenceGeometry {
	void Init(ProjectionCurveContainer mc, Func<IMultiCurve> mcGen, DrawMode drawMode);
}

}