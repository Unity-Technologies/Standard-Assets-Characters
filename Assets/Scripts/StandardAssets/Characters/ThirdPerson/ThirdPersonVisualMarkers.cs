using System;
using System.Security.Permissions;
using UnityEngine;
using StandardAssets.Characters.Physics;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.GizmosHelpers;
using Util;


namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	[RequireComponent(typeof(ThirdPersonBrain))]
    public class ThirdPersonVisualMarkers : MonoBehaviour
    {
	    public bool enablePowerDebug;
	    
	    /// <summary>
	    /// The Physic implementation used to do the movement
	    /// e.g. CharacterController or Rigidbody (or New C# CharacterController analog)
	    /// </summary>
	    protected ICharacterPhysics characterPhysics;

	    /// <summary>
	    /// The Input implementation to be used
	    /// e.g. Default unity input or (in future) the new new input system
	    /// </summary>
	    protected ICharacterInput characterInput;


	    protected ThirdPersonBrain characterMotor;

	    public ICharacterPhysics physicsForCharacter
	    {
		    get { return characterPhysics; }
	    }

	    public ICharacterInput inputForCharacter
	    {
		    get { return characterInput; }
	    }
	    
	    public ThirdPersonBrain motorForCharacter
	    {
		    get { return characterMotor; }
	    }
	    
	    /// <summary>
	    /// Get physics and input on Awake
	    /// </summary>
	    protected virtual void Awake()
	    {
		    characterPhysics = GetComponent<ICharacterPhysics>();
		    characterInput = GetComponent<ICharacterInput>();
		    characterMotor = GetComponent<ThirdPersonBrain>();
	    }
	    
	    /// <summary>
	    /// Rotate around an implied circle to give the vector at a certain point along the circumference 
	    /// </summary>
	    public Vector3 rotateByDegrees(Vector3 centre, float radius, float angle)
	    {
		    var centreX = centre.x;
		    var centreY = centre.z;

		    angle = angle * Mathf.Deg2Rad;
		    
		    var rotationPoint = new Vector3();
		    rotationPoint.x = (Mathf.Sin(angle) * radius) + centreX;
		    rotationPoint.y = centre.y;
		    rotationPoint.z = (Mathf.Cos(angle) * radius) + centreY;
		    
		    return rotationPoint;
	    }
	    
	    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
	        if (enablePowerDebug)
	        {
		        if (Application.isPlaying)
		        {
			        //Forward direction of the character transform
			        Debug.DrawLine(transform.position, transform.position + transform.forward * 5, Color.green);
			        
			        //Translate move input from vector2 into 3D space
			        var translatedMoveInput = new Vector3(characterInput.moveInput.x, 0, characterInput.moveInput.y);
			        
			        //Find the direction of input relative to both the character and the main camera
			        Debug.DrawLine(transform.position, transform.position + Camera.main.transform.TransformDirection(translatedMoveInput) * 10, Color.blue);
			        
			        //Intended rotation by degrees
			        float angle = characterMotor.rootMotionThirdPersonMotor.targetYRotation;
			        
			        //Find vector rotated by given degrees
			        Vector3 targetPoint = rotateByDegrees(transform.position, 1, angle);
			        
			        //Draw the line to the intended rotation
			        Debug.DrawLine(transform.position, targetPoint, Color.red);
			        
		        }
	        }
        }
	    #endif
    }
}


