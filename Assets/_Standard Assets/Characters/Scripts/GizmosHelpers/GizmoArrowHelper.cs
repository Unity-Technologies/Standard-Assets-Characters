using System.Runtime.InteropServices;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.GizmosHelpers
{
    public class GizmoArrowHelper : MonoBehaviour
    {
        private GameObject cylinderPrefab;
        
        
        public bool enablePowerDebug;
	    
        /// <summary>
        /// The Input implementation to be used
        /// e.g. Default unity input or (in future) the new new input system
        /// </summary>
        protected ICharacterInput characterInput;


        protected CharacterBrain characterMotor;

        public ICharacterInput inputForCharacter
        {
            get { return characterInput; }
        }
	    
        public CharacterBrain motorForCharacter
        {
            get { return characterMotor; }
        }
	    
        /// <summary>
        /// Get physics and input on Awake
        /// </summary>
        protected virtual void Awake()
        {
            characterInput = GetComponent<ICharacterInput>();
            characterMotor = GetComponent<CharacterBrain>();
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
        
        //Testing Code for arrow models
        private GameObject forwardDirection;
        
        //Testing Code for arrow models
        private void Start()
        {
            cylinderPrefab = GameObject.Find("GizmoArrow");
            CreateCylinderBetweenPoints(transform.position, transform.position + transform.forward * 5, 0.5f, Color.cyan, out forwardDirection);
        }

        
        //Testing Code for arrow models
        void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, float width, Color color, out GameObject cylinderObject)
        {
            var offset = end - start;
            // var scale = new Vector3(width, offset.magnitude / 2.0f, width);
            var position = start + (offset / 2.0f);
            cylinderObject = Instantiate(cylinderPrefab);
            cylinderObject.transform.forward = offset;
            cylinderObject.transform.position = position;
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
                    forwardDirection.transform.position = transform.position + transform.forward * 5;
                    
			        
                    //Translate move input from vector2 into 3D space
                    var translatedMoveInput = new Vector3(characterInput.moveInput.x, 0, characterInput.moveInput.y);
			        
                    //Find the direction of input relative to both the character and the main camera
                    Debug.DrawLine(transform.position, transform.position + Camera.main.transform.TransformDirection(translatedMoveInput) * 10, Color.blue);
			        
                    //Intended rotation by degrees
                    float angle = characterMotor.targetYRotation;
			        
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