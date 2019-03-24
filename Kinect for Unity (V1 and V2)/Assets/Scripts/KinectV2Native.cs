// comment or uncomment the following #define directives
// depending on whether you use KinectExtras together with KinectManager

//#define USE_KINECT_INTERACTION_OR_FACETRACKING
//#define USE_SPEECH_RECOGNITION

using UnityEngine;
using System;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;

// Wrapper class that holds the various structs and dll imports
// needed to set up a model with the Kinect.
public class KinectV2Wrapper
{
	public static class Constants
	{
		public const float NuiDepthHorizontalFOV = 70.6f;
		public const float NuiDepthVerticalFOV = 60.0f;

		public const ushort MaxRange = 8000;
		public const ushort MinRange = 500;

		public const int ColorImageWidth = 1920;
		public const int ColorImageHeight = 1080;
		public const int BytesPerPixel = 4;
		public const NuiImageResolution ColorImageResolution = NuiImageResolution.resolution1920x1080;

		public const int DepthImageWidth = 512;
		public const int DepthImageHeight = 424;
		public const NuiImageResolution DepthImageResolution = NuiImageResolution.resolution512x424;

		public const bool IsNearMode = false;
	}

	public enum NuiImageResolution
	{
		resolutionInvalid = -1,
		resolution1920x1080 = 0,
		resolution512x424 = 1,
	}

	[Flags]
	public enum FrameEdges
	{
		None = 0,
		Right = 1,
		Left = 2,
		Top = 4,
		Bottom = 8
	}

	public struct NuiImageViewArea
	{
		public int eDigitalZoom;
		public int lCenterX;
		public int lCenterY;
	}

	public class NuiImageBuffer
	{
		public int m_Width;
		public int m_Height;
		public int m_BytesPerPixel;
		public IntPtr m_pBuffer;
	}

	public struct NuiLockedRect
	{
		public int pitch;
		public int size;
		public IntPtr pBits;
	}

	public static int GetDepthWidth()
	{
		return Constants.DepthImageWidth;
	}

	public static int GetDepthHeight()
	{
		return Constants.DepthImageHeight;
	}

	public static int GetColorWidth()
	{
		return Constants.ColorImageWidth;
	}

	public static int GetColorHeight()
	{
		return Constants.ColorImageHeight;
	}

	public static bool PollColor(ColorFrameReader reader, ref byte[] colorFrameData)
	{
		bool newColor = false;
		if (reader != null)
		{
			var frame = reader.AcquireLatestFrame();
			if (frame != null)
			{
				newColor = true;
				frame.CopyConvertedFrameDataToArray(colorFrameData, ColorImageFormat.Rgba);
				frame.Dispose();
				frame = null;
			}
		}
		
		return newColor;
	}

	public static bool PollInfrared(InfraredFrameReader reader, ref ushort[] infraredFrameData)
	{
		bool newInfrared = false;
		if (reader != null)
		{
			var frame = reader.AcquireLatestFrame();
			if (frame != null)
			{
				newInfrared = true;
				frame.CopyFrameDataToArray(infraredFrameData);
				frame.Dispose();
				frame = null;
			}
		}

		return newInfrared;
	}

	public static bool PollDepth(DepthFrameReader reader, bool isNearMode, ref ushort[] depthFrameData)
	{
		bool newDepth = false;
		if (reader != null)
		{
			var frame = reader.AcquireLatestFrame();
			if (frame != null)
			{
				newDepth = true;
				frame.CopyFrameDataToArray(depthFrameData);
				frame.Dispose();
				frame = null;
			}
		}
		
		return newDepth;
	}
}