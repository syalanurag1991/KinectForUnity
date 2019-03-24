using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class KinectData
{
	// Kinect streams variables
	public int ColorWidth { get; protected set; }
	public int ColorHeight { get; protected set; }
	public long ColorFrameSize { get; protected set; }
	public long ColorFrameDataLength { get; protected set; }

	public int DepthWidth { get; protected set; }
	public int DepthHeight { get; protected set; }
	public long DepthFrameSize { get; protected set; }
	public long DepthFrameDataLength { get; protected set; }

	public ushort[] RawDepths;
	public ushort[] CorrectedDepths;
	public byte[] ColorStreamData;
	public Color32[] InfraredStreamColors;
	public Color32[] RegisteredColorStreamColors;
	public ColorSpacePoint[] ColorSpace;

	// Point-cloud variables
	public int NumberOfMeshes { get; protected set; }
	public int DownsampleSize { get; protected set; }
	public int MeshWidth { get; protected set; }
	public int MeshHeight { get; protected set; }
	public GameObject[] MeshObjects;
	public Mesh[] Meshes;
	public Color32[][] MeshColors;
	public Vector3[][] MeshVertices;

	public KinectData(
		int colorWidth, int colorHeight, int depthWidth, int depthHeight, int bytesPerPixel,
		int downsampleSize, Transform kinectManagerTransform)
	{
		// Initializing kinect streams properties
		ColorWidth = colorWidth;
		ColorHeight = colorHeight;
		ColorFrameSize = colorWidth * colorHeight;
		ColorFrameDataLength = ColorFrameSize * bytesPerPixel;

		DepthWidth = depthWidth;
		DepthHeight = depthHeight;
		DepthFrameSize = depthWidth * depthHeight;
		DepthFrameDataLength = DepthFrameSize * bytesPerPixel;

		// Initialize point-cloud properties
		DownsampleSize = downsampleSize;
		NumberOfMeshes = DownsampleSize * DownsampleSize;
		MeshWidth = DepthWidth / DownsampleSize;                                                                                        // downsample to lower resolution
		MeshHeight = DepthHeight / DownsampleSize;
		GameObject prefab = Resources.Load("MeshPrefab") as GameObject;

		CreateStorageForKinectStreams();
		CreateMeshesForPointCloud();
	}

	private void CreateStorageForKinectStreams()
	{
		ColorStreamData = new byte[4 * ColorFrameSize];
		RawDepths = new ushort[DepthFrameSize];
		CorrectedDepths = new ushort[DepthFrameSize];
		ColorSpace = new ColorSpacePoint[DepthFrameSize];
		InfraredStreamColors = new Color32[DepthFrameSize];
		RegisteredColorStreamColors = new Color32[DepthFrameSize];
	}

	private void CreateMeshesForPointCloud()
	{
		Meshes = new Mesh[NumberOfMeshes];                                                                                              // create mesh components for mesh objects
		MeshVertices = new Vector3[NumberOfMeshes][];
		MeshColors = new Color32[NumberOfMeshes][];
		int[][] meshTriangles = new int[NumberOfMeshes][];

		for (int i = 0; i < NumberOfMeshes; i++)                                                                                        // assign mesh components to mesh objects
		{
			Meshes[i] = new Mesh();
			MeshVertices[i] = new Vector3[MeshWidth * MeshHeight];
			MeshColors[i] = new Color32[MeshWidth * MeshHeight];
			meshTriangles[i] = new int[6 * ((MeshWidth - 1) * (MeshHeight - 1))];

			int triangleIndex = 0;
			for (int y = 0; y < MeshHeight; y++)
			{
				for (int x = 0; x < MeshWidth; x++)
				{
					int index = (y * MeshWidth) + x;
					MeshVertices[i][index] = new Vector3(x, -y, 0);
					MeshColors[i][index] = new Color32(0, 0, 255, 255);
					if (x != (MeshWidth - 1) && y != (MeshHeight - 1))                                                                  // skip the last row/col
					{
						int topLeft = index;
						int topRight = topLeft + 1;
						int bottomLeft = topLeft + MeshWidth;
						int bottomRight = bottomLeft + 1;
						meshTriangles[i][triangleIndex++] = topLeft;
						meshTriangles[i][triangleIndex++] = topRight;
						meshTriangles[i][triangleIndex++] = bottomLeft;
						meshTriangles[i][triangleIndex++] = bottomLeft;
						meshTriangles[i][triangleIndex++] = topRight;
						meshTriangles[i][triangleIndex++] = bottomRight;
					}
				}
			}

			Meshes[i].vertices = MeshVertices[i];
			Meshes[i].colors32 = MeshColors[i];
			Meshes[i].triangles = meshTriangles[i];
			Meshes[i].RecalculateNormals();
		}
	}
}
