using System;

namespace StandardAssets.Characters.ThirdPerson
{
    public interface IThirdPersonMotorStateMachine
    {
        Action idling { get; set; }
        Action walking { get; set; }
        Action running { get; set; }
    }
}