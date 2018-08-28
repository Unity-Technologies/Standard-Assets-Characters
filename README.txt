# Standard Assets: Character Package

## Overview

This repository contains the new **Standard Assets Character Controller Package**, consisting of both code to drive a more advanced version of First and Third Person Characters than the old Standard Assets Character, as well as a demo scene and setup for Character prototyping purposes and example setup of First and Third Person Characters.

For more detailed documentation, please see the WIP Google Docs:  
* Open Character Controller:
	
	https://docs.google.com/document/d/1TAodMmM8OlbFibyWvOKEU-zj-HI-yI4YJJx45S-hwTo/edit?usp=sharing
#

	***ALEC TO LINK MORE TO***



## Notable Content

	/Assets/_24BitDemo/
	
* Contains test / throwaway assets for use during the development phase of the project

#

	/Assets/_Standard Assets/Characters/
	
* Contains all code, models, and animation assets and related items for the logic of the First and Third Person Character Controllers
	* NOTE: The Input Manager setup for the Character Controllers is dependent on the mappings set up on ** InputManager.asset **

#

	/Assets/_Standard Assets/Prototyping/
	
* Contains the assets for the Pro Builder protoyping environment built to demonstrate the Character Controllers in

#

	/Assets/_Standard Assets/Characters/Exmpales/SimpleMovementController
	
* Contains an example scene and demonstration scripts for setting up a simple custom Third Person Movement Controller using both the Default Unity Character Controller as well as the new C# Open Character Controller

#

	/Assets/_Standard Assets/Characters/Exmpales/SimpleNavMeshInputController
	
* Contains an example scene and and script setup to demonstrate a simple point-and-click-to-move a character whose input is controlled by NavMesh navigation

#



## Scenes

	/Assets/_Standard Assets/Prototyping/Scenes/Protoland

* Main prototyping and demo scene for the Character Controller project

#

	/Assets/_Standard Assets/Characters/Exmpales/SimpleMovementController/Scenes/SimpleMovementController

* Example scene to demonstrate a simple custom character controller using both the Default Unity Character Controller as well as the new C# Open Character Controller

#

	/Assets/_Standard Assets/Characters/Exmpales/SimpleMovementController/Scenes/SimpleNavMeshInputController

* Example scene to demonstrate a simple NavMesh input driven character controller



## Character Controller Setup Instructions

### Base Setup:
	1) Drag in InputManager prefab
	2) Drag in Main Camera prefab


### First Person Controller:
	1) Drag in First Person Cameras prefab
	2) Drag in FirstPersonInput prefab
	3) Drag in FirstPerson prefab
	4) Select First Person Cameras
		a) On the Cinemachine State Driven Camera, drag FirstPerson onto the Follow property		
	5) Select FirstPerson
		a) On the First Person Brain, drag First Person Cameras onto the Camera Animations property
		b) On the Legacy Cross Platform Character Input, drag StandaloneInput (located under FirstPersonInput) onto the Standalone Input property
		c) Repeat for MobileInput and the Mobile Input property


### Third Person Controller:
	1) Drag in Third Person Cameras prefab
	2) Drag in ThirdPersonInput prefab
	3) Drag in MaleThirdPerson prefab
	4) Select Third Person Cameras
		a) On the Cinemachine State Driven Camera, drag MaleThirdPerson onto the Follow and Look At properties
	5) Select FreeLook Camera under Third Person Cameras
		a) On the Cinemachine State Driven Camera, drag MaleThirdPerson to the Animated Target property
	6) Repeat above step for Strafe under MaleThirdPerson
	7) Select MaleThirdPerson
		a) On the Third Person Brain, drag Third Person Cameras onto the Camera Animation Manager property
		b) On the Legacy Cross Platform Character Input, drag StandaloneInput (located under ThirdPersonInput) onto the Standalone Input property
		c) Repeat for MobileInput and the Mobile Input property


### Demo Scene (optional, for dev testing purposes):
	1) Drag in PlayerManager and Select it
		a) Drag Current Camera Text onto the Third Person Camera Mode Text property
		b) Drag MaleThirdPerson onto the Third Person Brain property, as well as under the List of Third Person Game Objects
		c) Drag FirstPersonLegacyInput onto the First Person Brain property, as well as under the List of First Person Game Objects
		d) Drag Third Person Camaeras onto the Third Person Main State Driven Camera property
		e) Drag Third Person Camaeras onto the Third Person Main State Driven Camera property



## Camera Setup Overview

	1) It is all located in the Protoland scene under:
		* Characters -> ThirdPerson -> Third Person Cameras
	2) There are 2 State Driven Camera camera setups:
		* Forward-Unlocked Transposing, called *Exploration* for ESL understanding
			* This has Freelook Cameras set up for Idle, Run, and Sprint states
		* Forward-Locked Rectilinear, called *Strafe* for ESL understanding
			* This only has a single Freelook Camera set up for the Strafe state
	3) A simple *Third Person Camera Animation Manager* component exists on the *Third Person Cameras* GameObject.  
		* It is designd to control how the cameras recieve input as well as for tracking all of the various cameras in the scene that the character controller can use (as well as the starting one), with the idea that this can also help handle any potential additional logic with the transitions between the cameras.
	4) A quick overview of the properties on this Camera Manager is as follows:
		* Starting Camera Mode:  Use this change whether you start is free look or strafe
		* Camera Mode Input:  Will be used to change from Free Look to Strafe and vice versa, at run-time
		* Camera Toggle Input:  Used to switch between cameras in a specific mode (Strafe: strafe to aim)
		* Free Look Camera Objects:  GameObjects that are enabled when switching to Free Look mode. e.g. custom UI
		* Strafe Camera Objects:  GameObjects that are enabled when switching to Strafe mode. e.g. crosshair
		* Free Look Camera States:  States on the animator for the different Free Look Camera modes.
		* Strafe Camera States:  States on the animator for the different Strafe Camera modes (e.g. rectilinear aiming, orbiting target)



## Project Status Overview: Phase 1

### Ready for Review and Feedback:
	* Input Manager (Legacy)
		** Code and Design Structure
		** Keyboard & Mouse, Gamepad (PS4, Xbox360, XboxOne), Mobile Touch Screen Support
	
	* Open Character Controller
		** Code and Design Structure
		** Behaviour and Functionality

	* First Person Controller
		** Code and Design Structure
		** Behaviour and Functionality for Walk, Sprint, Crouch, Jump

	* Third Person Controller 
		** Code and Design Structure
		** Behaviour and Functionality for Idle, Walk, Run, Sprint, Turning Locomotion
		** Behaviour and Functionality for Falling and Landing
		** Standing-Forward Jump Behaviour
		** Nav Mesh Input Controller
		** Animation State Machine

	* Simple Movement Controller Example
		** Demo scene to test out Legacy Character Controller and Open Character Controller controlled Cylinders


### Currently Being Worked On:
	* Input Manager (Legacy)
		** Inspector UX
	
	* Open Character Controller
		** Inspector UX

	* First Person Controller
		** Inspector UX
		** Cinemachine Camera Change Bug

	* Third Person Controller 
		** Inspector UX
		** New Standing and Running Jump Animations
		** Tweaks to Jump Motion Curve Feel
		** Strafing Locomotion Behaviour and Animations
		** Effects Detection System for Character Feet
		** Collision Capsule Resizing Based on Animation Frames
	
	* Cinemachine Camera Setups
		** First Person Cameras
		** Third Person Cameras

	* First Pass Documentation of the Systems and Behaviours
	* General Internal commenting and code review pass


### Still Requires Work:
	* Input Manager (Legacy)
		** Code Comments
	
	* Open Character Controller
		** Code Comments

	* First Person Controller
		** Code Comments

	* Third Person Controller 
		** Code Comments

	* Garbage Generation and Optimation Pass
	* Addition of Female Avatar & Aniamtion Set

