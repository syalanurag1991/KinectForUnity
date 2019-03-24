using UnityEngine;
using System;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class KinectV2Manager : KinectManager
{
	// Image stream handles for the kinect
	private KinectSensor sensor;
	private CoordinateMapper coordinateMapper;
	private ColorFrameReader colorFrameReader;
	private InfraredFrameReader infraredFrameReader;
	private DepthFrameReader depthFrameReader;

	// Update variables
	private bool colorStreamUpdated = false;
	private bool infraredStreamUpdated = false;
	private bool depthDataUpdated = false;

	protected void Awake()
	{
		Instance = this;
	}

	public override void InitializeKinectManager()
	{
		kinectInitialized = false;

		try
		{
			sensor = KinectSensor.GetDefault();
			if (sensor != null)
			{
				coordinateMapper = sensor.CoordinateMapper;
				if (!sensor.IsOpen)
					sensor.Open();
			}
			else
			{
				throw new Exception("Kinect SDK V2.0 initialization Failed");
			}

			colorFrameReader = sensor.ColorFrameSource.OpenReader();
			if (colorFrameReader == null)
			{
				throw new Exception("Cannot open color stream");
			}

			infraredFrameReader = sensor.InfraredFrameSource.OpenReader();
			if (infraredFrameReader == null)
			{
				throw new Exception("Cannot open long-exposure infrared stream");
			}

			depthFrameReader = sensor.DepthFrameSource.OpenReader();
			if (depthFrameReader == null)
			{
				throw new Exception("Cannot open depth stream");
			}

			DontDestroyOnLoad(gameObject);
		}
		catch (DllNotFoundException e)
		{
			string message = "Please import KinectUnityAddin.dll.";
			Debug.LogError(message);
			Debug.LogError(e.ToString());

			return;
		}
		catch (Exception e)
		{
			string message = e.Message;
			Debug.LogError(message);
			Debug.LogError(e.ToString());
			return;
		}

		InitializeStorage();

		Instance = this;

		Debug.Log("Kinect V2 is initialized");
		kinectInitialized = true;
	}

	// Checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
	public override bool IsKinectInitialized()
	{
		return Instance != null ? ((KinectV2Manager)Instance).kinectInitialized : false;
	}

	// Initialize storage for processed kinect data
	protected override void InitializeStorage()
	{
		processedKinectData = new KinectData(
			KinectV2Wrapper.Constants.ColorImageWidth,KinectV2Wrapper.Constants.ColorImageHeight,
			KinectV2Wrapper.Constants.DepthImageWidth, KinectV2Wrapper.Constants.DepthImageHeight,
			KinectV2Wrapper.Constants.BytesPerPixel, downsampleSize, gameObject.transform);
		return;
	}

	// Returns the whole block of processed kinect data
	public override void GetProcessedKinectData(ref KinectData processedKinectDataReference)
	{
		processedKinectDataReference = processedKinectData;
	}

	// Returns color for a pixel by index
	public override void GetColorForPixelByIndex(int colorSpaceIndex, ref Color32 color)
	{
		int actualIndex = KinectV2Wrapper.Constants.BytesPerPixel * colorSpaceIndex;
		color = new Color32(
			base.processedKinectData.ColorStreamData[actualIndex],
			base.processedKinectData.ColorStreamData[actualIndex + 1],
			base.processedKinectData.ColorStreamData[actualIndex + 2],
			255);
	}

	// Returns color for a pixel by position
	public override void GetColorForPixelByPosition(int x, int y, ref Color32 color)
	{
		int actualIndex = KinectV2Wrapper.Constants.BytesPerPixel * (y * KinectV2Wrapper.Constants.ColorImageWidth + x);
		color = new Color32(
			base.processedKinectData.ColorStreamData[actualIndex],
			base.processedKinectData.ColorStreamData[actualIndex + 1],
			base.processedKinectData.ColorStreamData[actualIndex + 2],
			255);
	}

	// Returns depth for a pixel by index
	public override void GetDepthForPixelByIndex(int index, ref ushort depth)
	{
		depth = base.processedKinectData.CorrectedDepths[index];
	}

	// Returns depth for a pixel by position
	public override void GetDepthForPixelByPosition(int x, int y, ref ushort depth)
	{
		depth = base.processedKinectData.CorrectedDepths[y * KinectWrapper.Constants.DepthImageWidth + x];
	}

	// Returns infrared for a pixel by index
	public override void GetInfraredColorForPixelByIndex(int depthSpaceIndex, ref Color32 color)
	{
		int actualIndex = KinectV2Wrapper.Constants.BytesPerPixel * depthSpaceIndex;
		color = base.processedKinectData.InfraredStreamColors[actualIndex];
	}

	// Returns infrared for a pixel by position
	public override void GetInfraredColorForPixelByPosition(int x, int y, ref Color32 color)
	{
		int actualIndex = KinectV2Wrapper.Constants.BytesPerPixel * (y * KinectV2Wrapper.Constants.DepthImageWidth + x);
		color = base.processedKinectData.InfraredStreamColors[actualIndex];
	}

	// Returns registered color for a pixel by index
	public override void GetRegisteredColorForPixelByIndex(int depthSpaceindex, ref Color32 color)
	{
		color = base.processedKinectData.RegisteredColorStreamColors[depthSpaceindex];
	}

	// Returns registered color for a pixel by position
	public override void GetRegisteredColorForPixelByPosition(int x, int y, ref Color32 color)
	{
		int colorSpaceIndex = KinectV2Wrapper.Constants.BytesPerPixel * (y * KinectV2Wrapper.Constants.DepthImageWidth + x);
		color = base.processedKinectData.RegisteredColorStreamColors[colorSpaceIndex];
	}

	void Update()
	{
		if (kinectInitialized)
		{
			colorStreamUpdated = KinectV2Wrapper.PollColor(colorFrameReader, ref processedKinectData.ColorStreamData);
			infraredStreamUpdated = KinectV2Wrapper.PollInfrared(infraredFrameReader, ref processedKinectData.RawDepths);
			depthDataUpdated = KinectV2Wrapper.PollDepth(depthFrameReader, KinectV2Wrapper.Constants.IsNearMode, ref processedKinectData.CorrectedDepths);
			if (colorStreamUpdated && depthDataUpdated && (displayStreams || displayPointCloud))
				RefreshStreamsAndPointCloud();
			if (Input.GetKeyDown(KeyCode.Escape))
				Application.Quit();
		}
	}

	// Refresh all depth-based streams and point-cloud
	protected override void RefreshStreamsAndPointCloud()
	{
		if (processedKinectData == null || processedKinectData.DepthWidth == 0 || processedKinectData.DepthHeight == 0 || processedKinectData.ColorWidth == 0 || processedKinectData.ColorHeight == 0)
		{
			Debug.Log("Kinect data not initialized");
			return;
		}

		coordinateMapper.MapDepthFrameToColorSpace(base.processedKinectData.CorrectedDepths, base.processedKinectData.ColorSpace);
		int meshIndex = 0;
		for (int meshY = 0; meshY < downsampleSize; meshY++)
		{
			for (int meshX = 0; meshX < downsampleSize; meshX++)
			{
				int smallIndex = 0;
				for (int y = meshY * processedKinectData.MeshHeight; y < (meshY + 1) * processedKinectData.MeshHeight; y++)
				{
					for (int x = meshX * processedKinectData.MeshWidth; x < (meshX + 1) * processedKinectData.MeshWidth; x++)
					{
						int depthSpaceIndex = y * KinectV2Wrapper.Constants.DepthImageWidth + x;
						ColorSpacePoint colorSpacePoint = base.processedKinectData.ColorSpace[depthSpaceIndex];
						if (!float.IsNegativeInfinity(colorSpacePoint.X) &&
							!float.IsNegativeInfinity(colorSpacePoint.Y) &&
							base.processedKinectData.CorrectedDepths[depthSpaceIndex] >= KinectV2Wrapper.Constants.MinRange &&
							base.processedKinectData.CorrectedDepths[depthSpaceIndex] < KinectV2Wrapper.Constants.MaxRange)
						{
							int actualColorIndex = KinectV2Wrapper.Constants.BytesPerPixel * (
								((int)(colorSpacePoint.Y + 0.5f) * KinectV2Wrapper.Constants.ColorImageWidth) +	(int)(colorSpacePoint.X + 0.5f));
							Color32 pixelColor = new Color32(0, 0, 0, 0);
							if (actualColorIndex >= 0 && actualColorIndex < processedKinectData.ColorFrameDataLength)
							{
								pixelColor.r = base.processedKinectData.ColorStreamData[actualColorIndex];
								pixelColor.g = base.processedKinectData.ColorStreamData[actualColorIndex + 1];
								pixelColor.b = base.processedKinectData.ColorStreamData[actualColorIndex + 2];
								pixelColor.a = 255;
							}

							if (displayPointCloud)
							{
								processedKinectData.MeshColors[meshIndex][smallIndex] = pixelColor;                                                                         // update mesh colors
								processedKinectData.MeshVertices[meshIndex][smallIndex++].z = base.processedKinectData.CorrectedDepths[depthSpaceIndex] * depthScale;      // update mesh geometry
							}

							if (displayStreams)
							{
								byte infraredPixelIntensity = (byte)(base.processedKinectData.RawDepths[depthSpaceIndex] >> 7);
								base.processedKinectData.InfraredStreamColors[depthSpaceIndex] = new Color32(infraredPixelIntensity, infraredPixelIntensity, infraredPixelIntensity, 255);
								base.processedKinectData.RegisteredColorStreamColors[depthSpaceIndex] = pixelColor;					// update registered colorstream texture data
							}
						} else
						{
							if (displayPointCloud)
							{
								processedKinectData.MeshVertices[meshIndex][smallIndex].z = float.NegativeInfinity;						// set 'z' to -Infinity
								processedKinectData.MeshColors[meshIndex][smallIndex++].a = 0;                                          // set alpha values to 0, update voxel index
							}

							if (displayStreams)
							{
								base.processedKinectData.InfraredStreamColors[depthSpaceIndex] = new Color32(0, 0, 0, 255);
								base.processedKinectData.RegisteredColorStreamColors[depthSpaceIndex].a = 0;
							}
								
						}																																			
					}
				}

				if(displayPointCloud)																									// update mesh texture and geometry
				{
					processedKinectData.Meshes[meshIndex].vertices = processedKinectData.MeshVertices[meshIndex];
					processedKinectData.Meshes[meshIndex].colors32 = processedKinectData.MeshColors[meshIndex];
					processedKinectData.Meshes[meshIndex++].RecalculateNormals();
				}
			}
		}

		//if (OpenCVInterface.Instance != null)
		//{
		//	//OpenCVInterface.Instance.ConvertRGBDataToOpenCVFormat(rgbDataArray);
		//	//OpenCVInterface.Instance.ConvertDepthDataToOpenCVFormat(depthDataArray);
		//}
	}

	// Make sure to kill the Kinect on quitting
	public override void OnApplicationQuit()
	{
		if (kinectInitialized)
		{
			if (colorFrameReader != null)
			{
				colorFrameReader.Dispose();
				colorFrameReader = null;
			}

			if (infraredFrameReader != null)
			{
				infraredFrameReader.Dispose();
				infraredFrameReader = null;
			}

			if (depthFrameReader != null)
			{
				depthFrameReader.Dispose();
				depthFrameReader = null;
			}

			if (coordinateMapper != null)
				coordinateMapper = null;

			if (sensor != null)																											// Shutdown Kinect V2
			{
				if (sensor.IsOpen)
					sensor.Close();

				sensor = null;
			}
			Instance = null;
		}
	}
}
