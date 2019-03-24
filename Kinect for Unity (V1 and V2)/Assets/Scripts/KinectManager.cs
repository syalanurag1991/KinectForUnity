using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;


public abstract class KinectManager : MonoBehaviour
{
	// Display color, infrared and registrered color streams on UI
	public bool displayStreams;

	// Display point-cloud
	public bool displayPointCloud;

	// Point-cloud variables
	protected int downsampleSize { get; private set; }
	public float depthScale;

	// Motor angle (only available for Kinect v1)
	protected int kinectMotorAngle { get; private set; }

	// Bool to keep track of whether Kinect has been initialized
	protected bool kinectInitialized;

	// Kinect data storage
	public KinectData processedKinectData;

	// Returns the single KinectManager instance
	public static KinectManager Instance;

	// Initializes and creates sensor instance
	public abstract void InitializeKinectManager();

	// Checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
	public abstract bool IsKinectInitialized();

	// Set downsample size
	public void SetDownsampleSize(int size)
	{
		downsampleSize = size;
	}

	// Set motor angle
	public void SetMotorAngle(int angle)
	{
		kinectMotorAngle = angle;
	}

	// Initialize storage for processed kinect data
	protected abstract void InitializeStorage();

	// Returns the whole block of processed kinect data
	public abstract void GetProcessedKinectData(ref KinectData processedKinectDataReference);

	// Returns depth for a pixel by index
	public abstract void GetDepthForPixelByIndex(int index, ref ushort depth);

	// Returns depth for a pixel by position
	public abstract void GetDepthForPixelByPosition(int x, int y, ref ushort depth);

	// Returns color for a infrared pixel by index
	public abstract void GetInfraredColorForPixelByIndex(int index, ref Color32 color);

	// Returns color for a infrared pixel by position
	public abstract void GetInfraredColorForPixelByPosition(int x, int y, ref Color32 color);

	// Returns color for a pixel by index
	public abstract void GetColorForPixelByIndex(int index, ref Color32 color);

	// Returns color for a pixel by position
	public abstract void GetColorForPixelByPosition(int x, int y, ref Color32 color);

	// Returns registered color for a pixel by index
	public abstract void GetRegisteredColorForPixelByIndex(int index, ref Color32 color);

	// Returns registered color for a pixel by position
	public abstract void GetRegisteredColorForPixelByPosition(int x, int y, ref Color32 color);

	// Refresh all depth-based streams and point-cloud
	protected abstract void RefreshStreamsAndPointCloud();

	// Kills sensor object and assocaited instances
	public abstract void OnApplicationQuit();
}


	