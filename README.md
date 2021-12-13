# agora-webrtc-unity
An unity package for agora webrtc.

## NOTE:
This project was based on MixedReality-WebRTC:
https://microsoft.github.io/MixedReality-WebRTC/index.html

Get documents for the unity components from:
https://microsoft.github.io/MixedReality-WebRTC/manual/unity/helloworld-unity.html

We have modified the Signaling scripts to support Agora WebRTC.

# QUICK START:

1. Create an account from https://sso2.agora.io/cn/v4/signup/with-sms?redirect_uri=https%3A%2F%2Fconsole.agora.io

2. Create a project in the Project Manager Tab from the website

3. Copy the AppID from the project

![This is an image](https://github.com/elefan/agora-webrtc-unity/blob/main/Docs/AppID.jpg)

4. Download and open agora-webrtc-unity with Unity-2019.04.28

5. In the Hierarchy Window, click the Media Object

6. In the Inspector Window, enter your AppID to the AgoraWebRTC Signaler Component

7.  In the Inspector Window, create a channel name in the AgoraWebRTC Signaler Component

![This is an image](https://github.com/elefan/agora-webrtc-unity/blob/main/Docs/AgoraWebRTCSignaler.jpg)

8. Click the Run Button above, then this project will run on your Unity Editor

9. Then, build and deploy the appx to your hololens2

- Create VS project by Unity

![This is an image](https://github.com/elefan/agora-webrtc-unity/blob/main/Docs/Build.jpg)

![This is an image](https://github.com/elefan/agora-webrtc-unity/blob/main/Docs/BuildForARM.jpg)

- Build and deploy to hololens2 by VS2019

![This is an image](https://github.com/elefan/agora-webrtc-unity/blob/main/Docs/VS.jpg)
