using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;

namespace StandardAssets.Characters.Common
{
    /// <summary>
    /// Handles the Cinemachine look override and applies acceleration and deceleration to look input when
    /// using Input Value Gain speed mode on a Cinemachine camera.
    /// </summary>
    [RequireComponent(typeof(CharacterInput))]
    public class CameraInputGainAcceleration: MonoBehaviour
    {
        [SerializeField, Tooltip("Time in seconds to accelerate look inputs to max speed")]
        protected float m_Acceleration = 0.1f;
    
        [SerializeField, Tooltip("Time in seconds to decelerate look inputs to 0.0f")]
        protected float m_Deceleration = 0.1f;
    
        [SerializeField, Tooltip("X Axis multiplier that can be applied to camera axis input. " +
                                 "Depending on the type of camera being used and the range of movement " +
                                 "for each axis, this can be used in addition to sensitivity to keep the " +
                                 "perceived vertical and horizontal camera movement speed the same")] 
        protected float m_XAxisMultiplier = 1.0f;
    
        [SerializeField, Tooltip("Y Axis multiplier that can be applied to camera axis input. " +
                                 "Depending on the type of camera being used and the range of movement " +
                                 "for each axis, this can be used in addition to sensitivity to keep the " +
                                 "perceived vertical and horizontal camera movement speed the same")] 
        protected float m_YAxisMultiplier = 0.01f;
        
        // Check if look input was processed
        bool m_HasProcessedMouseLookInput;
            
        // The frame count when an input axis was processed 
        int m_LookInputProcessedFrame;
            
        // The delta time used to calculate the input acceleration
        float accelerationAppliedDeltaTime;
    
        // The delta time of the Cinemachine update calls 
        float cameraUpdateDeltaTime;
    
        // The look vector used by Cinemachine with acceleration or deceleration applied
        Vector2 acceleratedLookInput;
        
        // Character input component that handles setting of the actual look inputs from a given input device 
        CharacterInput characterInput;
    
        // Has cinemachine consumed the current look input value? This is to ensure it is cleared in the CharacterInput 
        // component so look input values are not used more than once 
        public bool hasProcessedMouseLookInput
        {
            get { return m_HasProcessedMouseLookInput; }
            set { m_HasProcessedMouseLookInput = value; }
        }
        
        // Constant Epsilon, 0.0001f
        const float Epsilon = UnityVectorExtensions.Epsilon;
        
        private void Awake()
        {
            if (characterInput == null)
            {
                characterInput = GetComponent<CharacterInput>();
            }
        }
        
        /// <summary>
        /// Sets up the Cinemachine delegate
        /// </summary>
        private void OnEnable()
        {
            CinemachineCore.GetInputAxis += LookInputOverride;
        }
    
        /// <summary>
        /// Disables the Cinemachine delegate
        /// </summary>
        private void OnDisable()
        {
            CinemachineCore.GetInputAxis -= LookInputOverride;
        }
        
        // Apply acceleration and/or deceleration to an input vector
        Vector2 ApplyAcceleration(Vector2 actualInput, Vector2 acceleratedInput, float deltaTime, float acceleration, float deceleration)
        {
            acceleratedInput.y =  Accelerate(actualInput.y, acceleratedInput.y, deltaTime, acceleration, deceleration); 
            acceleratedInput.x =  Accelerate(actualInput.x, acceleratedInput.x, deltaTime, acceleration, deceleration);
    
            return acceleratedInput;
        }
            
        // Apply acceleration and/or deceleration to a component of a vector
        float Accelerate(float actualInput, float acceleratedInput , float deltaTime, float acceleration, float deceleration)
        {
            // If the acceleration and deceleration values are negligible,
            // or a negative number, then no calculations need be done.
            if (acceleration < Epsilon && deceleration < Epsilon)
            {
                return actualInput;
            }
            
            if (Mathf.Abs(actualInput) < Epsilon
                || (Mathf.Sign(acceleratedInput) == Mathf.Sign(actualInput)
                    && Mathf.Abs(actualInput) <  Mathf.Abs(acceleratedInput)))
            {
                // Need to decelerate
                var a = Mathf.Abs(actualInput - acceleratedInput) / Mathf.Max(Epsilon, deceleration);
                var delta = Mathf.Min(a * deltaTime, Mathf.Abs(acceleratedInput));
                acceleratedInput -= Mathf.Sign(acceleratedInput) * delta;
    
                if (Mathf.Abs(acceleratedInput) < Epsilon)
                {
                    acceleratedInput = 0.0f;
                }
            }
            else
            {
                // Need to accelerate
                var a = Mathf.Abs(actualInput - acceleratedInput) / Mathf.Max(Epsilon, acceleration);
                acceleratedInput += Mathf.Sign(actualInput) * a * deltaTime;
                if (Mathf.Sign(acceleratedInput) == Mathf.Sign(actualInput)
                    && Mathf.Abs(acceleratedInput) > Mathf.Abs(actualInput))
                {
                    acceleratedInput = actualInput;
                }
            }
          
            return acceleratedInput;
        }
        
        // Handles the Cinemachine delegate
        float LookInputOverride(string axis)
        {
            // This is to ensure that mouse look inputs are properly cleared once they have been processed as mouse
            // input has no canceled action event subscribed to it, and can be set more than once per frame
            
            // If the input device is a Mouse, then the lookInput value will have no acceleration or deceleration 
            // applied to it before it is consumed by Cinemachine.
            
            // Sensitivity is applied after acceleration or deceleration.
            
            if (characterInput.usingMouseInput)
            {
                var currentFrame = Time.frameCount;
                if ((m_LookInputProcessedFrame < currentFrame) && m_HasProcessedMouseLookInput)
                {
                    characterInput.lookInput = Vector2.zero;
                }
                m_LookInputProcessedFrame = currentFrame;
                m_HasProcessedMouseLookInput = true;
            }
            else
            {
                accelerationAppliedDeltaTime = Time.fixedUnscaledTime - cameraUpdateDeltaTime;
                acceleratedLookInput = ApplyAcceleration(characterInput.lookInput, 
                    acceleratedLookInput, accelerationAppliedDeltaTime,
                    m_Acceleration, m_Deceleration);
                cameraUpdateDeltaTime = Time.fixedUnscaledTime;
            }

            
            if (axis == "Vertical")
            {
                float lookVertical = 0.0f;
                
                if (characterInput.usingMouseInput)
                {
                    lookVertical = characterInput.m_InvertY 
                        ? characterInput.lookInput.y 
                        : -characterInput.lookInput.y;
                }
                else
                {
                    lookVertical = characterInput.m_InvertY 
                        ? acceleratedLookInput.y 
                        : -acceleratedLookInput.y;
                }

                if (characterInput.UseTouchControls())
                {
                    lookVertical *= characterInput.cameraLookSensitivity.touchVerticalSensitivity;
                }
                else
                {
                    lookVertical *= characterInput.usingMouseInput
                        ? characterInput.cameraLookSensitivity.mouseVerticalSensitivity
                        : characterInput.cameraLookSensitivity.gamepadVerticalSensitivity;
                }

                lookVertical *= m_YAxisMultiplier;
                
                return lookVertical;
            }


            if (axis == "Horizontal")
            {
                float lookHorizontal = 0.0f;

                if (characterInput.usingMouseInput)
                {
                    lookHorizontal = characterInput.m_InvertX
                        ? characterInput.lookInput.x + characterInput.movingPlatformLookInput.x
                        : -characterInput.lookInput.x + characterInput.movingPlatformLookInput.x;
                }
                else
                {
                    lookHorizontal = characterInput.m_InvertX
                        ? acceleratedLookInput.x + characterInput.movingPlatformLookInput.x
                        : -acceleratedLookInput.x + characterInput.movingPlatformLookInput.x;
                }

                if (characterInput.UseTouchControls())
                {
                    lookHorizontal *= characterInput.cameraLookSensitivity.touchHorizontalSensitivity;
                }
                else
                {
                    lookHorizontal *= characterInput.usingMouseInput 
                    ? characterInput.cameraLookSensitivity.mouseHorizontalSensitivity 
                    :characterInput.cameraLookSensitivity.gamepadHorizontalSensitivity;
                }
                
                lookHorizontal *= m_XAxisMultiplier;
                
                return lookHorizontal;
            }
            
            return 0;
        }
    }
}