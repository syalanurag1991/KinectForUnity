# Kinect for Unity (works for both Kinect V1 and Kinect V2)
Library for using Kinect with Unity.
Unity version = 2018.3 with .Net 4.x.

The project contains just a main scene as of now:

![KinectManager](https://user-images.githubusercontent.com/32419039/54879708-5af96500-4df9-11e9-859d-d89d9d9b5a4c.jpg)

## About 'Main' scene
This is a slightly modified version of the original demo-scene developed by Microsoft. Displays color, infrared and body streams along with streams along with a point-cloud view. I replaced the 3D cube object by a UI canvas to get more FPS.

## About point-cloud
### Kinect V1
There is a single loop managing assignment of pixels for all color, infrared and registered color streams as they all have the exact same resolution. The FPS barely crosses 20, this is because the pixel-by-pixel mapping-function is very slow. There is a better mapper function available in C++, called as "MapColorFrameToDepthFrame". Upon running dependency walker, I found that this function is not exposed in kinect10.dll.

![KinectV1 - Point-cloud and streams](https://user-images.githubusercontent.com/32419039/54879630-73b54b00-4df8-11e9-84ae-8017f2926339.JPG)


### Kinect V2
This scene was developed by me using “registered frames” from Kinect. The bindings to get color and depth frame data were changed from multi-frame reader to color and depth frame readers for Kinect V2. Reason being that using multi-frame reader limits Kinect’s output of depth data to 15 FPS. This is because getting color-frame data is an added stress for the multi-frame reader API. Using color-frame reader and depth-frame readers separately, along with the CoordinateMapper class, generates the output at 30 FPS which is more desirable. 

![KinectV2 - Point-cloud and streams](https://user-images.githubusercontent.com/32419039/54879641-816ad080-4df8-11e9-8961-5ab7438ca41b.JPG)
