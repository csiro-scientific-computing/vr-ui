# vr-ui
Created by Ben Skinner as a Vacation Scholar with the CSIRO.

## About
Simple Virtual Reality User Interface elements. They've been designed to work across different VR interface devices and rely primarily on collision with unity colliders, however it has been developed with, and has native support for the HTC Vive.

## Requirements
 * The [SteamVR Unity Plugin v1.2.0](https://www.assetstore.unity3d.com/en/#!/content/32647) is currently required to interface with the tracked controllers. It is not included in the download of this repo.

## Installation
Simply drag the vr-ui.unitypackage into your unity project!

## Usage
VR-UI has been design to be as easy to use and as flexible in customization as possible. In vr-ui/Prefabs/Interactables you will find all the available interactable user interface prefabs available to use. Simply drag any of these into your scene and position it how you need.

The only other necessary step to do is to provide something for the interactable to collide with. The interactables all work off of Unity Triggers which mean a GameObject with a collider and a kinematic RigidBody component is required. In the demos a small sphere is positioned at a good location for reference. (In the future this will be able to be spawned automatically).

At this point the button should be able to be interacted with and you can add events and effects as desired.

In order for haptic feedback to correctly function you must attach the corresponding VR controller script to each controller. This has been only tested and implemented on the HTC Vive as I do not have access to an Oculus touch, however feel free to add the changes and make a pull request! The necessary script is in vr-ui/Scripts/Controllers/.

## Design
The design of VR-UI draws inspiration from traditional 2D user interface paradigms and combines years of UI development with this new technology and medium. Currently we have two forms of interaction designs available, an Interaction Surface, and an Interaction Volume.

The interaction functionality has been designed around Unity triggers and events, so theoretically they should work with any VR system and controller configuration as long as the controller collider has a kinematic RigidBody component attached.

The surfaces are more akin to traditional, flat, ui elements. They require a physical "press" to activate and to manipulate. Their activation threshold, design, and functionality can be customized through the Unity Editor, with support for more options planned.

The volumes differ in design, and require the user to physically enter a volume with their cursor and then press a customizable controller button to activate.

Interaction Surface	//Push past threshold
 * IS_Button
 * IS_Slider
 * IS_Radial

Interaction Volume	//Activate within trigger
 * IV_Draggable


## License
[CSIRO Open Source Software License Agreement (variation of the BSD / MIT License)](LICENSE)
