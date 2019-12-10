# Standard Assets: Character Package (BETA)

Note: this project is work in progress and currently considered a beta. It contains dependency on packages such as our new Input System which is in preview, and as such will continue to be in beta so long as it relies on in Preview tech. We will ship this content as a package in 2019.

## READ ME FIRST

This package is intended for use in 2019.3.0f1 so we recommend you follow suit as fixes to aspects we use like new input will be applied to a 2019.3.0f1 compatible package first.


## Feedback

The following forum should be used for feedback on this project -

https://forum.unity.com/threads/new-standard-asset-characters-third-person.526612/

## Overview

This repository contains the new **Standard Assets Character Controller Package**, consisting of both code to drive a more advanced version of First and Third Person Characters than the old Standard Assets Character, as well as a demo scene and setup for Character prototyping purposes and example setup of First and Third Person Characters.

For more detailed documentation, please see the WIP Google Docs:  

Overview:

	https://docs.google.com/document/d/15bda-1wa4fn_ecUcWXTvxAZsNBIsI89zztKf2UxoQY4/edit?usp=sharing

Quick Start Guide:

	https://docs.google.com/document/d/1DiyRG4FYeMhka8LfzXbkPFBS7oaYJo0Cg9asBbvYsDs/edit?usp=sharing

Open Character Controller:

	https://docs.google.com/document/d/1TAodMmM8OlbFibyWvOKEU-zj-HI-yI4YJJx45S-hwTo/edit?usp=sharing	

First Person Controller:

	https://docs.google.com/document/d/14C-a3P5o3tAXWd_Dzl8Zc-jh3uQUTaKcfWxPvqXv9w0/edit?usp=sharing

Third Person Contoller:

	https://docs.google.com/document/d/1uU6SqWtDF0BPT81bYcVRBMZ2Li6XrKtGznItrfH8hvc/edit?usp=sharing

Movement Effects:

	https://docs.google.com/document/d/1544ZvaGuWcW47CViHq1lHZkiuuqwVaxz7Cd4htyea-A/edit?usp=sharing	
	
Character Input:

	https://docs.google.com/document/d/1f8HcBEjz-Fpd-FJPg7npfB8XOuSzAWaM4b_g_2aEhT0/edit?usp=sharing

	


## Getting Started

### From Repo
* Pull the repository and open the project in the relevant version of Unity
* Open the Protoland Scene
* Enable either the First Person or the Third Person Game Object under Characters
* Press Play

### Importing the Package
* Import the SAC package.
* Make sure that you have the following Unity Packages added in your manifest.json:
	* "com.unity.cinemachine": "2.4.0-preview.8"
	* "com.unity.inputsystem": "1.0.0-preview.3"
	* "com.unity.postprocessing": "2.2.2"
	* "com.unity.probuilder": "4.1.2"
* Open the Protoland Scene
* Enable either the First Person or the Third Person Game Object under Characters
* Press Play
    
### Base Setup
* Create a new Scene.
* Make sure that the main scene camera has the Main Camera Tag set on it
* Place a plane in the Scene to act as the floor and resize to preference.
* Optional set-up for Movement Effects:
    * Drag in Movement Zone Manager prefab.
    * To change the level default effects, create a new LevelMovementZoneConfiguration via Create -> Standard Assets -> Characters -> Level Movement Zone Configuration and set it on this prefab
    * Set the plane’s layer to Ground, in order for Third Person footsteps to work.
    
### First Person Setup
* Drag in the First Person prefab.

### Third Person Setup
* Drag in the Third Person (Male) prefab.
    * Ensure that the Y value is greater than the Ground Plane’s Y value.
* Drag in the Third Person Camera (Male) prefab.
* There is also a female third person character. To use this instead, delete the male prefabs from the scene and drag in the female prefabs:
	* Third Person (Female)
	* Third Person Camera (Female)

## Controls

* First Person
	* Keyboard + Mouse
		* Movement:		WASD / Arrow Keys
		* Look:			Mouse Cursor
		* Sprint:		Left Shift [HOLD]
		* Jump:			Spacebar
		* Crouch:		Left Ctrl [HOLD]
	* Gamepad
		* Movement:		Left Thumbstick
		* Look:			Right Thumbstick
		* Sprint:		Left Thumbstick Click [TOGGLE]
		* Jump:			PS4 [X]  Xbox [A]
		* Crouch:		Right Thumbstick Click [TOGGLE]
* Third Person
	* Keyboard + Mouse
		* Movement:		WASD / Arrow Keys
		* Camera:		Mouse Cursor
		* Sprint:		Left Shift [HOLD]
		* Jump:			Spacebar
		* Strafe:		Right Mouse Button [HOLD]
		* Recenter:		V Key
	* Gamepad
		* Movement:		Left Thumbstick
		* Camera:		Right Thumbstick
		* Sprint:		Left Thumbstick Click [TOGGLE]
		* Jump:			PS4 [X]  Xbox [A]
		* Strafe:		Left Trigger [TOGGLE]
		* Recenter:		Right Thumstick Click



## Notable Content


#

	/Assets/_Standard Assets/Characters/
	
* Contains all code, models, and animation assets and related items for the logic of the First and Third Person Character Controllers

#

	/Assets/_Standard Assets/Prototyping/
	
* Contains the assets for the Pro Builder protoyping environment built to demonstrate the Character Controllers in

#


## Scenes

	/Assets/_Standard Assets/Prototyping/Scenes/Protoland

* Main prototyping and demo scene for the Character Controller project

#

	/Assets/_Standard Assets/Characters/Exmpales/SimpleMovementController/Scenes/SimpleMovementController

* Example scene to demonstrate a simple custom character controller using both the Default Unity Character Controller as well as the new C# Open Character Controller

#

	/Assets/_Standard Assets/Characters/Exmpales/SimpleMovementEffects
	
* Example scene that demonstates how to set up different Movement Effects on the contactable meshes in a scene to allow for per-material footstep sounds and particles

#

	/Assets/_Standard Assets/Characters/Exmpales/SimpleMovementController/Scenes/SimpleNavMeshInputController

* Example scene to demonstrate a simple NavMesh input driven character controller

#

