using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectSensor : MonoBehaviour
{
	public bool kinectV1;
	public bool kinectV2;

	public bool displayStreams;
	public bool displayPointCloud;
	public int downsampleSize;
	public float depthScale;
	public int kinectMotorAngle;

	void Start()
    {
		if (kinectV1)
		{
			kinectV2 = false;
			gameObject.AddComponent<KinectV1Manager>();
			gameObject.GetComponent<KinectV1Manager>().enabled = true;
			KinectManager instance = KinectManager.Instance;

			instance.displayStreams = displayStreams;
			instance.displayPointCloud = displayPointCloud;
			instance.SetDownsampleSize(downsampleSize);
			instance.SetMotorAngle(kinectMotorAngle);
			instance.depthScale = depthScale;
			instance.InitializeKinectManager();

			Destroy(GetComponent<SelectSensor>());
		}
		else if (kinectV2)
		{
			kinectV1 = false;
			gameObject.AddComponent<KinectV2Manager>();
			gameObject.GetComponent<KinectV2Manager>().enabled = true;
			KinectManager instance = KinectManager.Instance;

			instance.displayStreams = displayStreams;
			instance.displayPointCloud = displayPointCloud;
			instance.SetDownsampleSize(downsampleSize);
			instance.SetMotorAngle(kinectMotorAngle);
			instance.depthScale = depthScale;
			instance.InitializeKinectManager();

			Destroy(GetComponent<SelectSensor>());
		}
		else
		{
			Debug.Log("Please quit and select a sensor!!");
			Application.Quit();
		}
	}
}
