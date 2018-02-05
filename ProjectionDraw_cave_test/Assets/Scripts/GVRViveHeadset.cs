using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam.Network;
using Holojam.Tools;

public class GVRViveHeadset : Trackable {


  [SerializeField] public Vector3 offset;
  [SerializeField] public Vector3 offsetRot;


  public Transform targetTransform;
  //private WonderlandConfig config;

  const float correctionThreshold = 0.98f;
  //Lower values allow greater deviation without correction
  Quaternion correction = Quaternion.identity;

  const float differenceThreshold = 0.9995f;
  //Lower values allow correction at greater angular speeds
  public float difference = 1;
  public float correctness = 1;
  public float correctY;

  const float timestep = 0.01f;
  float lastTime = 0;
  private Quaternion lastRot = Quaternion.identity;
  public string label = "Viewer";
  public string scope = "Vive";

  private const int MAX_LAG = 100;

  const int IMU_BUFFER_SIZE = 600;
  public Quaternion[] smoothedRotation = new Quaternion[2] { Quaternion.identity, Quaternion.identity };
  public Quaternion[] correctRotation = new Quaternion[2] { Quaternion.identity, Quaternion.identity };
  private float delayTime;
  private Dictionary<float, Quaternion> imuBuffer = new Dictionary<float, Quaternion>();
  private List<float> imuBufferIdx = new List<float>();

  /*
  private Dictionary<string, string> configNames = new Dictionary<string, string>() {
    {"ALICE-HMD", "AliceHMD" },
    {"RABBIT-HMD", "RabbitHMD" },
    {"VR1", "VR1HMD" },
    {"VR2", "VR2HMD" },
    {"VR3", "VR3HMD" },
    {"VR4", "VR4HMD" },
    {"VR5", "VR5HMD" },
    {"VR6", "VR6HMD" }
  };
  */

  protected override void Awake() {
    base.Awake();
    //config = FindObjectOfType<WonderlandConfig>();
  }

  protected override void Update() {
    /*
    if (configNames.ContainsKey(label)) {
      string name = configNames[label];
      Vector3 getPos;
      Vector3 getRot;
      if (config.TryGetValue(name + "Pos", out getPos) && config.TryGetValue(name + "Rot", out getRot)) {
        if (!getPos.Equals(Vector3.zero)) {
          offset = getPos;
          offsetRot = getRot;
          //Debug.Log("Setting " + label + "values to: " + getPos + "\t" + getRot);
        }
      }
    }
    */
    base.Update();
  }

  private void IMUUpdateTracking()
  {


    // vive tracker's data
    Vector3 sourcePosition = RawPosition;
    Quaternion sourceRotation = RawRotation;

    if (targetTransform) {
      sourcePosition = targetTransform.position;
    }
    sourcePosition += sourceRotation * offset;

    bool sourceTracked = Tracked;
    // for test
    //sourceTracked = false;

    //Don't use Camera.main (reference to Oculus' instantiated camera at runtime)
    //in the editor or standalone, reference the child camera instead
    Vector3 cameraPosition = BuildManager.IsMasterClient() ?
      GetComponentInChildren<Camera>().transform.position : Camera.main.transform.position;
    //Negate Oculus' automatic head offset (variable reliant on orientation) independent of recenters
    transform.position = sourcePosition - cameraPosition;

    if (sourceTracked)
    {
      Quaternion imu = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
      Quaternion optical = sourceRotation * Quaternion.Inverse(imu);

      //Calculate rotation difference since last timestep
      if (Time.time > lastTime + timestep)
      {
        difference = Quaternion.Dot(imu, lastRot);
        lastRot = imu;
        lastTime = Time.time;
      }

      //Ignore local space rotation in the IMU calculations
      Quaternion localRotation = transform.rotation;
      //if(actor!=null && actor.localSpace && actor.transform.parent!=null)
      //    localRotation=Quaternion.Inverse(actor.transform.parent.rotation)*transform.rotation;
      //else if(actor==null && localSpace && transform.parent!=null)
      //    localRotation=Quaternion.Inverse(transform.parent.rotation)*transform.rotation;

      //Recalculate IMU correction if stale (generally on init/recenter)
      if (Quaternion.Dot(localRotation * imu, sourceRotation) <= correctionThreshold
        && difference >= differenceThreshold) //But not if the headset is moving quickly
        correction = optical;

      //IMU orientation (applied automatically by Oculus) is assumed below as a precondition
      //switch (trackingType)
      //{
      //  case TrackingType.IMU: //IMU, absolutely oriented by optical tracking intermittently
      //    if (BuildManager.IsMasterClient())
      //      goto case TrackingType.OPTICAL; //Don't use IMU tracking in the editor or standalone
      //    transform.rotation = correction;
      //    break;
      //  case TrackingType.OPTICAL: //Purely optical tracking, no IMU
      //    transform.rotation = optical;
      //    break;
      //}
    }
    else
      transform.rotation = correction; //Transition seamlessly to IMU when untracked

    //Apply local rotation if necessary
    //if(actor!=null && actor.localSpace && actor.transform.parent!=null)
    //  transform.rotation=actor.transform.parent.rotation*transform.rotation;
    //else if(actor==null && localSpace && transform.parent!=null)
    //  transform.rotation=transform.parent.rotation*transform.rotation;
  }

  private void updateIMU()
  {
    Quaternion imu = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
    //		print ("imuBuffer.Add " + Timestamp);
    if (imuBuffer.ContainsKey(Timestamp))
    {
      imuBuffer.Remove(Timestamp);
      imuBufferIdx.Remove(Timestamp);
    }
    imuBuffer.Add(Timestamp, imu);
    imuBufferIdx.Add(Timestamp);
    if (imuBuffer.Count > IMU_BUFFER_SIZE)
    {
      imuBuffer.Remove(imuBufferIdx[0]);
      imuBufferIdx.Remove(imuBufferIdx[0]);
      //print ("imuBuffer.Remove " + imuBufferIdx[0]);
    }
  }


  private Quaternion getIMU(float dt)
  {
    float minDT = 100000;
    Quaternion imu_dt = Quaternion.identity;
    KeyValuePair<float, Quaternion> min_pair = new KeyValuePair<float, Quaternion>(0.0f, Quaternion.identity);
    foreach (KeyValuePair<float, Quaternion> imupair in imuBuffer)
    {
      if (Mathf.Abs(imupair.Key - dt) < minDT)
      {
        minDT = Mathf.Abs(imupair.Key - dt);
        imu_dt = imupair.Value;
        min_pair = imupair;
      }
    }
    print(min_pair);
    return imu_dt;
  }

  //Update late to catch local space updates
  protected override void UpdateTracking()
  {
    // old version only need IMU
    //		IMUUpdateTracking();
    //updateIMU();
    // new version support sensor fusion
    ViveTrackerUpdateTracking();
    calculateDiff();
    if (Tracked)
      correctRotation[0] = transform.rotation;
  }

  // from Connor's python script
  private const float dt = 15.0f / 1000.0f;
  private const float C = 0.1f;
  private const float epsilon = dt / (C + dt);
  // end

  // aaron
  readonly Holojam.Utility.AccurateSmoother smoother = new Holojam.Utility.AccurateSmoother(1, 2);

  private Quaternion QSlerp(Quaternion a, Quaternion b, float t)
  {
    return QPow(b * Quaternion.Inverse(a), t) * a;
  }

  private Quaternion QPow(Quaternion q, float t)
  {
    return QExp(QScale(QLn(q), t));
  }

  private Quaternion QExp(Quaternion q)
  {
    float r = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
    float et = Mathf.Exp(q.w);
    float s = r > 0.00001f ? et * Mathf.Sin(r) / r : 0f;
    Quaternion res = new Quaternion(q.x * s, q.y * s, q.z * s, Mathf.Cos(r));
    return res;
  }

  private Quaternion QLn(Quaternion q)
  {
    float r = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
    float t = r > 0.00001f ? Mathf.Atan2(r, q.w) / r : 0f;
    Quaternion res = new Quaternion(q.x * t, q.y * t, q.z * t, 0.5f * Mathf.Log(Quaternion.Dot(q, q)));
    return res;
  }

  private Quaternion QScale(Quaternion q, float s)
  {
    Quaternion res = new Quaternion(q.x * s, q.y * s, q.z * s, q.w * s);
    return res;
  }

  private void ViveTrackerUpdateTracking()
  {
    // vive tracker's data
    Vector3 sourcePosition = RawPosition;
    Quaternion sourceRotation = RawRotation;
    Quaternion offsetRotationQ = Quaternion.Euler(offsetRot.x, offsetRot.y, offsetRot.z);
    sourceRotation = sourceRotation * offsetRotationQ;
    sourcePosition += sourceRotation * offset;

    bool sourceTracked = Tracked;

    //Don't use Camera.main (reference to Oculus' instantiated camera at runtime) in the editor or standalone, reference the child camera instead
    Vector3 cameraPosition = transform.position;// GetComponentInChildren<Camera>().transform.position;

    // applied position directly from vive tracker
    //print("source position:" + sourcePosition);

    //Negate Oculus' automatic head offset (variable reliant on orientation) independent of recenters
    transform.position += sourcePosition - cameraPosition;

    if (sourceTracked)
    {
      Quaternion imu = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);

      Quaternion inv = Quaternion.Inverse(imu);
      Quaternion optical = sourceRotation * inv;
      //Quaternion localRotation = transform.rotation;

      Quaternion oldOrientation = this.transform.rotation;
      //Quaternion mul = oldOrientation * imu;
      //float d = 0.5f + 0.5f * Quaternion.Dot(mul, sourceRotation);

      //float t = (1f - d);
      //t = 0.5f;
      //Quaternion res = QSlerp (oldOrientation, optical, t);
      //this.transform.rotation = res;

      float yOpt = optical.eulerAngles.y;
      float yOld = oldOrientation.eulerAngles.y;
      float yDiff = Mathf.Abs(yOpt - yOld);
      if (yDiff > 180f)
      {
        if (yOpt < yOld)
        {
          yOpt += 360f;
        }
        else
        {
          yOld += 360f;
        }
        yDiff = Mathf.Abs(yOpt - yOld);
      }
      float t = yDiff / 180f;
      t = t * t;
      float yNew = Mathf.LerpAngle(yOld, yOpt, t);
      this.transform.rotation = Quaternion.AngleAxis(yNew, Vector3.up);
    }
    else
    {
      transform.rotation = correctRotation[0]; //Transition seamlessly to IMU when untracked
                                               //print ("not tracked," + transform.rotation);
    }
  }

  private void calculateDiff()
  {
    //Calculate rotation difference since last timestep
    if (Time.time > lastTime + timestep)
    {
      difference = Quaternion.Dot(correctRotation[0], correctRotation[1]);
      correctness = Quaternion.Dot(RawRotation, correctRotation[1]);
    }
  }

  public override string Label { get { return label; } }
  public override string Scope { get { return scope; } }
}
