using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KinectDisplay : MonoBehaviour
{
	// For holding reference to active kinect manager
	private static KinectManager kinectManagerInstance;

	// For holding imported processed kinect data
	private static KinectData processedKinectData;

	// Supporting textures for displaying streams
	private Texture2D colorStreamTexture;
	private Texture2D infraredStreamTexture;
	private Texture2D registeredColorStreamTexture;

	// Display kinect streams on UI
	public RawImage colorStreamDisplay;
	public RawImage infraredStreamDisplay;
	public RawImage registeredColorStreamDisplay;

	void Update()
	{
		if (kinectManagerInstance == null)
		{
			SetKinectManagerInstance();
			return;
		}

		if (processedKinectData == null)
		{
			kinectManagerInstance.GetProcessedKinectData(ref processedKinectData);
			if (kinectManagerInstance.displayStreams)
				SetStreamTextures();
			return;
		}

		if (kinectManagerInstance.displayStreams)
			UpdateStreamTextures();
	}

	private void SetKinectManagerInstance()
	{
		KinectManager kinectManagerObject = FindObjectOfType<KinectV1Manager>();
		if (kinectManagerObject != null)
		{
			Debug.Log("Found Kinect V1");
			kinectManagerInstance = KinectManager.Instance;
			
			// set sizes and positions of display images to suite aspect ratio
			colorStreamDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 150f);
			infraredStreamDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 150f);
			registeredColorStreamDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 150f);
			colorStreamDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(140f, -112f);
			infraredStreamDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(140f, 0f);
			registeredColorStreamDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(140f, 112f);

			return;
		}

		kinectManagerObject = FindObjectOfType<KinectV2Manager>();
		if (kinectManagerObject != null)
		{
			Debug.Log("Found Kinect V2");
			kinectManagerInstance = KinectManager.Instance;
			return;
		}

		Debug.Log("No active kinect manager instance found!");
		return;
	}

	void SetStreamTextures()
	{
		// Initialize textures
		colorStreamTexture = new Texture2D(processedKinectData.ColorWidth, processedKinectData.ColorHeight, TextureFormat.RGBA32, false);
		infraredStreamTexture = new Texture2D(processedKinectData.DepthWidth, processedKinectData.DepthHeight, TextureFormat.RGBA32, false);
		registeredColorStreamTexture = new Texture2D(processedKinectData.DepthWidth, processedKinectData.DepthHeight, TextureFormat.RGBA32, false);

		// Color stream
		if (colorStreamDisplay != null)
			colorStreamDisplay.texture = colorStreamTexture;

		// Infrared stream
		if (infraredStreamDisplay != null)
			infraredStreamDisplay.texture = infraredStreamTexture;

		// Registered color stream
		if (registeredColorStreamDisplay != null)
			registeredColorStreamDisplay.texture = registeredColorStreamTexture;
	}

	// Update depth data, registered RGB and graded depth images
	void UpdateStreamTextures()
	{
		// Update color stream
		colorStreamTexture.LoadRawTextureData(processedKinectData.ColorStreamData);
		
		// Update infrared stream
		infraredStreamTexture.SetPixels32(processedKinectData.InfraredStreamColors);

		// Update registered color stream
		registeredColorStreamTexture.SetPixels32(processedKinectData.RegisteredColorStreamColors);
		
		// Apply all texture changes 
		colorStreamTexture.Apply(false, false);
		infraredStreamTexture.Apply(false, false);
		registeredColorStreamTexture.Apply(false, false);
	}
}
