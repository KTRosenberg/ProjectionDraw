using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FRL_KTR {

public interface IReferenceGeometry {
	void Init(ProjectionCurveContainer mc, Func<IMultiCurve> mcGen, DrawMode drawMode);
}

}