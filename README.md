# PROJECT FROZEN

# VDMRemoteDeviceTool
**VDMRemoteDeviceTool** is a developer tool that allows easy usage of "Remote Devices" functionality in "PSMoveServiceEx-Virtual-Device-Manager" by Timocop. The tool is presented as a C# DLL library, and is extremely easy to use (see documentation below). Since the tool is under development, errors and bugs may be present in it, if you help by reporting them to **[Issues](https://github.com/Amorale09/VDMRemoteDeviceTool/issues)**, the project will be updated faster and better!

## What it can and cannot do:

### Tool can:
 - Send a tracker rotation to VDM
 - Send a tracker acceleration to VDM
 - Change VDM address and port
 - Can add multiple trackers

### Tool cannot (development in progress):
 - Send a tracker position (**yet**)

# How does it work?
The tool uses a UDP connection to communicate with the VDM, and sends orientation data using the SlimeVr protocol. If you are interested in the sending protocol itself, then you can find its implementation in "DatagramPacket.cs"
### Special thanks:
 - [**PSMoveServiceEx - Virtual Device Manager by Timocop**](https://github.com/Timocop/PSMoveServiceEx-Virtual-Device-Manager)
 - [**SlimeVR Tracker ESP by SlimeVR**](https://github.com/SlimeVR/SlimeVR-Tracker-ESP)
 - [**PythonSlimeVRTracker by ameharoo**
](https://github.com/ameharoo/PythonSlimeVRTracker)

