using System;

namespace StandardAssets.Characters.ThirdPerson
{
    /// <summary>
    /// Decouples the detection of state change from the response to state change
    /// ThirdPersonCameraManager depends on this
    /// </summary>
    public interface IThirdPersonMotorStateMachine
    {
        /// <summary>
        /// Fired on start of Idling
        /// </summary>
        Action idling { get; set; }
        
        /// <summary>
        /// Fired on start of Walking
        /// </summary>
        Action walking { get; set; }
        
        /// <summary>
        /// Fired on start of Running
        /// </summary>
        Action running { get; set; }
    }
}