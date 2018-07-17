///Scenes:
* ThirdPersonLegacy: testbed for setting up the third person character
* FirstPersonLegacy: testbed for setting up the first person character
* ProbuilderScene00: the prototype environment

///Third Person Setup:
ThirdPersonAnimationController: dependency of IThirdPersonMotor. Sends normalized values exposed on IThirdPersonMotor to the Animator via configurable named paramaters. Allows the animation setup to be agnostic of who is controlling the character:
* ThirdPersonMotor: player controlled
* NavMeshThirdPersonMotor: AI controlled

Can switch out the physics and input components:
* ThirdPersonMotor - implements IThirdPersonMotor, the main controller which maps inputs (dependency of ICharacterInput) to physical movement (dependency on ICharacterPhysics) via root motion (OnAnimatorMove). There is an optional dependency on TurnaroundBehaviour which allows us to swap out realistic animation turnarounds, blendspace turnarounds or responsive animation turnarounds.
* Physics:
	- OpenCharacterController: a C# functional analog of the Unity C++ CharacterController. Requires CharacterCapsule and CharacterCapsuleMover components.
	- OpenCharacterControllerPhysics: the implementation of ICharacterPhysics that uses the OpenCharacterController
* Input:
	- LegacyCrossPlatformCharacterInput: the implementation of ICharacterInput that selects and setups onscreen controls for mobile, and mouse/keyboard/gamepad inputs for PC/OSX builds.
* TurnaroundBehaviour:
	- BlendspaceTurnaroundBehaviour: turns the character 180 (or the angle it receives from the ThirdPersonMotor) around over a specified time, while conditioning the values of turning speed and forward speed in the ThirdPersonAnimationController.
	- AnimationTurnaroundBehaviour: WIP. turns the character using the animation and crossfade..

Movement Event Components:
For more detail on movement events see below.
* MovementEventListener: uses the footfall events to play particles for footfalls
* ColliderMovementEventBroadcaster: sends events for each footfall using colliders on the feet. an example implementation of the generic MovementEventBroadcaster

//Movement Events:
A system for broadcasting various 



