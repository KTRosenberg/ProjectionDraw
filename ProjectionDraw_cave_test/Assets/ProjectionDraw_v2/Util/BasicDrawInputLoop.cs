//#define DO_DEBUG_PRINT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using System;

namespace FRL_KTR {

public class BasicDrawInputLoop : MonoBehaviour, 
IGlobalGripPressUpHandler,
IGlobalGripPressHandler,
IGlobalGripPressDownHandler
{
	public ProjectionCurveContainer projectionMultiCurve;
	private ProjectionCurveContainer _projectionMultiCurve;

	public ProjectionDrawInputHandler input;
	public ReferenceSingleGeometry drawWithRefManager;

	public Camera origin;
	public bool useLineRenderer = false;
	public bool autoSampleProject = false;
	public double autoSampleProjectTVal = 0.1;

	public bool autoReprojectScreenPoints = false;

	void OnValidate() {
		if (autoSampleProjectTVal <= 0.0f) {
			autoSampleProjectTVal = 0.04;
		}
	}

	void Update() {
		if (projectionMultiCurve == null || origin == null || input == null) {
			return;
		} else if (!projectionMultiCurve.isInitialized) {
			// TODO refactor so only one enum is necessary to pass to the curve
			if (useLineRenderer) {
				projectionMultiCurve.Init(MultiCurveFactoryStatic.Curve(
					DrawFunctions.DrawWithLineRenderer/*, PreprocessPipelineFunctions.TestStreamingCurvePreprocessFunc*/
				), DrawMode.UNITY_LINE_RENDERER);

				drawWithRefManager.Init(projectionMultiCurve, MultiCurveFactoryStatic.Curve(
					DrawFunctions.DrawWithLineRenderer/*, PreprocessPipelineFunctions.TestStreamingCurvePreprocessFunc*/
				),	DrawMode.UNITY_LINE_RENDERER);

			} else {
				projectionMultiCurve.Init(MultiCurveFactoryStatic.Curve(
					DrawFunctions.Default
				),	DrawMode.DEBUG_LINE);

				drawWithRefManager.Init(projectionMultiCurve, MultiCurveFactoryStatic.Curve(
					DrawFunctions.Default
				),	DrawMode.DEBUG_LINE);
			}
			_projectionMultiCurve = projectionMultiCurve;
		}
		// clear multi-curve
		if (Input.GetKeyDown(KeyCode.C)) {
			_projectionMultiCurve.Clear();
		}
		//
		// re-project screen points (overwrites default curve)
		if (Input.GetKeyDown(KeyCode.P)) {
			autoReprojectScreenPoints = !autoReprojectScreenPoints;
		}
		if (autoReprojectScreenPoints) {
			_projectionMultiCurve.ReProjectUsingScreenPoints(Camera.main);
		}
		//

		ProjectionDrawInputHandler.InputStatus status = input.updateInputStatus();

		if (drawWithRefManager.stateIsModified) {
			drawWithRefManager.stateIsModified = false;
			if (drawWithRefManager.isActive) {
				_projectionMultiCurve = drawWithRefManager.auxCurve;
			} else {
				_projectionMultiCurve = projectionMultiCurve;
				_projectionMultiCurve.MergeFrom(drawWithRefManager.auxCurve);
			}
		}

		// temp clean up branches
		if (input.inputType == ProjectionDrawInputHandler.InputDeviceOpt.VIVE) {
			if (autoSampleProject) {
				#if DO_DEBUG_PRINT
				Debug.Log("VIEWPORT");
				#endif
				_projectionMultiCurve.ProjectionDrawAutoSample(Camera.main, status, autoSampleProjectTVal, ProjectionCurveContainer.ScreenRay);
			} else {
				#if DO_DEBUG_PRINT
				Debug.Log("VIEWPORT");
				#endif
				_projectionMultiCurve.ProjectionDraw(Camera.main, status, ProjectionCurveContainer.ScreenRay);
			}
		} else {
			if (autoSampleProject) {
				#if DO_DEBUG_PRINT
				Debug.Log("SCREEN");
				#endif
				_projectionMultiCurve.ProjectionDrawAutoSample(origin, status, autoSampleProjectTVal, ProjectionCurveContainer.ScreenRay);
			} else {
				#if DO_DEBUG_PRINT
				Debug.Log("SCREEN");
				#endif
				_projectionMultiCurve.ProjectionDraw(origin, status, ProjectionCurveContainer.ScreenRay);
			}
		}
	}

	// TODO possibly replace with "gesture command control toggle"
	void IGlobalGripPressDownHandler.OnGlobalGripPressDown(VREventData eventData) {
		_projectionMultiCurve.Clear();
	}
	void IGlobalGripPressHandler.OnGlobalGripPress(VREventData eventData) {
	}
	void IGlobalGripPressUpHandler.OnGlobalGripPressUp(VREventData eventData) {
	}
		
}

}
