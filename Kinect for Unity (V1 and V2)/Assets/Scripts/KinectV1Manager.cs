﻿using UnityEngine;
using System;
using System.Collections;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class KinectV1Manager : KinectManager
{
	// Image stream handles for the kinect
	private IntPtr colorStreamHandle;
	private IntPtr depthStreamHandle;

	// Color frame buffer
	private static byte[] colorBuffer;

	// Structure needed by the coordinate mapper
	private static KinectV1Wrapper.NuiImageViewArea KinectCoordinatesAdjustment = new KinectV1Wrapper.NuiImageViewArea
	{
		eDigitalZoom = 0,
		lCenterX = 0,
		lCenterY = 0
	};

	protected override void Awake()
	{
		Instance = this;
	}

	public override void ActivateWithParameters(bool displayStreams, bool displayPointCloud, int downsampleSize, float depthScale, int kinectMotorAngle)
	{
		base.displayStreams = displayStreams;
		base.displayPointCloud = displayPointCloud;
		base.downsampleSize = downsampleSize;
		base.depthScale = depthScale;
		base.kinectMotorAngle = kinectMotorAngle;
		Instance = this;
		InitializeSensor();
	}

	public override void InitializeSensor()
	{
		kinectInitialized = false;
		colorBuffer = new byte[KinectV1Wrapper.Constants.ColorImageWidth * KinectV1Wrapper.Constants.ColorImageHeight * KinectV1Wrapper.Constants.BytesPerPixel];

		int hr = 0;

		try
		{
			hr = KinectV1Wrapper.NuiInitialize(KinectV1Wrapper.NuiInitializeFlags.UsesColor | KinectV1Wrapper.NuiInitializeFlags.UsesDepth);
			if (hr != 0)
			{
				throw new Exception("NuiInitialize Failed");
			}

			colorStreamHandle = IntPtr.Zero;
			hr = KinectV1Wrapper.NuiImageStreamOpen(KinectV1Wrapper.NuiImageType.Color,
				KinectV1Wrapper.Constants.ColorImageResolution, 0, 2, IntPtr.Zero, ref colorStreamHandle);
			if (hr != 0 && colorStreamHandle != IntPtr.Zero)
			{
				throw new Exception("Cannot open color stream");
			}

			depthStreamHandle = IntPtr.Zero;
			hr = KinectV1Wrapper.NuiImageStreamOpen(KinectV1Wrapper.NuiImageType.Depth,
				KinectV1Wrapper.Constants.DepthImageResolution, 0, 2, IntPtr.Zero, ref depthStreamHandle);
			if (hr != 0)
			{
				throw new Exception("Cannot open depth stream");
			}

			// set kinect elevation angle
			KinectV1Wrapper.NuiCameraElevationSetAngle(kinectMotorAngle);

			DontDestroyOnLoad(gameObject);
		}
		catch (DllNotFoundException e)
		{
			string message = "Please check the Kinect SDK installation.";
			Debug.LogError(message);
			Debug.LogError(e.ToString());

			return;
		}
		catch (Exception e)
		{
			string message = e.Message + " - " + KinectV1Wrapper.GetNuiErrorString(hr);
			Debug.LogError(message);
			Debug.LogError(e.ToString());
			return;
		}

		InitializeStorage();
		Instance = this;

		Debug.Log("Kinect V1 is initialized");
		kinectInitialized = true;
	}

	// Checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
	public override bool IsKinectInitialized()
	{
		return Instance != null ? ((KinectV1Manager)Instance).kinectInitialized : false;
	}

	// Initialize storage for processed kinect data
	protected override void InitializeStorage()
	{
		processedKinectData = new KinectData(
			KinectV1Wrapper.Constants.ColorImageWidth, KinectV1Wrapper.Constants.ColorImageHeight,
			KinectV1Wrapper.Constants.DepthImageWidth, KinectV1Wrapper.Constants.DepthImageHeight,
			KinectV1Wrapper.Constants.BytesPerPixel, downsampleSize, gameObject.transform);
	}

	// Returns the whole block of processed kinect data
	public override void GetProcessedKinectData(ref KinectData processedKinectDataReference)
	{
		processedKinectDataReference = processedKinectData;
	}

	// Returns color for a pixel by index
	public override void GetColorForPixelByIndex(int colorSpaceIndex, ref Color32 color)
	{
		int actualIndex = KinectV1Wrapper.Constants.BytesPerPixel * colorSpaceIndex;
		color = new Color32(
			processedKinectData.ColorStreamData[actualIndex],
			processedKinectData.ColorStreamData[actualIndex + 1],
			processedKinectData.ColorStreamData[actualIndex + 2],
			255);
	}

	// Returns color for a pixel by position
	public override void GetColorForPixelByPosition(int x, int y, ref Color32 color)
	{
		int actualIndex = KinectV1Wrapper.Constants.BytesPerPixel * (y * KinectV1Wrapper.Constants.ColorImageWidth + x);
		color = new Color32(
			processedKinectData.ColorStreamData[actualIndex],
			processedKinectData.ColorStreamData[actualIndex + 1],
			processedKinectData.ColorStreamData[actualIndex + 2],
			255);
	}

	// Returns depth for a pixel by index
	public override void GetDepthForPixelByIndex(int index, ref ushort depth)
	{
		depth = processedKinectData.CorrectedDepths[index];
	}

	// Returns depth for a pixel by position
	public override void GetDepthForPixelByPosition(int x, int y, ref ushort depth)
	{
		depth = processedKinectData.CorrectedDepths[y * KinectV1Wrapper.Constants.DepthImageWidth + x];
	}

	// Returns infrared for a pixel by index
	public override void GetInfraredColorForPixelByIndex(int depthSpaceIndex, ref Color32 color)
	{
		int actualIndex = KinectV1Wrapper.Constants.BytesPerPixel * depthSpaceIndex;
		color = base.processedKinectData.InfraredStreamColors[actualIndex];
	}

	// Returns infrared for a pixel by position
	public override void GetInfraredColorForPixelByPosition(int x, int y, ref Color32 color)
	{
		int actualIndex = KinectV1Wrapper.Constants.BytesPerPixel * (y * KinectV1Wrapper.Constants.DepthImageWidth + x);
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
		int colorSpaceIndex = KinectV1Wrapper.Constants.BytesPerPixel * (y * KinectV1Wrapper.Constants.DepthImageWidth + x);
		color = base.processedKinectData.RegisteredColorStreamColors[colorSpaceIndex];
	}

	void Update()
	{
		if (kinectInitialized)
		{
			colorStreamUpdated = KinectV1Wrapper.PollColor(colorStreamHandle, ref colorBuffer);
			depthDataUpdated = KinectV1Wrapper.PollDepth(depthStreamHandle, KinectV1Wrapper.Constants.IsNearMode, ref processedKinectData.RawDepths);

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
						int depthSpaceIndex = y * KinectV1Wrapper.Constants.DepthImageWidth + x;
						processedKinectData.CorrectedDepths[depthSpaceIndex] = (ushort)(processedKinectData.RawDepths[depthSpaceIndex] >> 3);

						if (displayStreams)
						{
							int colorDataIndex = depthSpaceIndex * KinectV1Wrapper.Constants.BytesPerPixel;
							processedKinectData.ColorStreamData[colorDataIndex] = colorBuffer[colorDataIndex + 2];														// creating color stream 
							processedKinectData.ColorStreamData[colorDataIndex + 1] = colorBuffer[colorDataIndex + 1];
							processedKinectData.ColorStreamData[colorDataIndex + 2] = colorBuffer[colorDataIndex];
							processedKinectData.ColorStreamData[colorDataIndex + 3] = 255;
						}

						if (base.processedKinectData.CorrectedDepths[depthSpaceIndex] >= KinectV1Wrapper.Constants.MinRange &&
							base.processedKinectData.CorrectedDepths[depthSpaceIndex] < KinectV1Wrapper.Constants.MaxRange)
						{
							int colorX = 0, colorY = 0;
							KinectV1Wrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
								KinectV1Wrapper.Constants.ColorImageResolution,
								KinectV1Wrapper.Constants.DepthImageResolution,
								ref KinectCoordinatesAdjustment,
								x, y, processedKinectData.RawDepths[depthSpaceIndex], out colorX, out colorY);
							int actualColorIndex = KinectV1Wrapper.Constants.BytesPerPixel * (colorY * KinectV1Wrapper.Constants.ColorImageWidth + colorX);
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
								processedKinectData.MeshColors[meshIndex][smallIndex] = pixelColor;																		// update mesh colors
								processedKinectData.MeshVertices[meshIndex][smallIndex++].z = base.processedKinectData.CorrectedDepths[depthSpaceIndex] * depthScale;	// update mesh geometry
							}

							if (displayStreams)
							{
								byte infraredIntensity = (byte)(processedKinectData.RawDepths[depthSpaceIndex] >> 7);
								processedKinectData.InfraredStreamColors[depthSpaceIndex] = new Color32(infraredIntensity, infraredIntensity, infraredIntensity, 255);  // creating infrared stream
								base.processedKinectData.RegisteredColorStreamColors[depthSpaceIndex] = pixelColor;														// update registered colorstream texture data
							}
						}
						else
						{
							if (displayPointCloud)
							{
								processedKinectData.MeshVertices[meshIndex][smallIndex].z = float.NegativeInfinity;														// set 'z' to -Infinity
								processedKinectData.MeshColors[meshIndex][smallIndex++].a = 0;																			// set alpha values to 0, update voxel index
							}

							if (displayStreams)
							{
								base.processedKinectData.InfraredStreamColors[depthSpaceIndex] = new Color32(0, 0, 0, 255);
								base.processedKinectData.RegisteredColorStreamColors[depthSpaceIndex].a = 0;
							}

						}
					}
				}

				if (displayPointCloud)																																	// update mesh texture and geometry
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
			KinectV1Wrapper.NuiShutdown();																						// Shutdown OpenNI
			Instance = null;
		}
	}
}

