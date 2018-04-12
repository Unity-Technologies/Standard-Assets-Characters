using UnityEngine;
using System.Collections;

/// <summary>
/// Implement that interface to define input for controllers. Allow to get input from various source
/// (e.g. an AI script, a file with pre recorder input etc.)
/// cf. DefaultControllerInput for a sample using basic Input class
/// </summary>
public interface IControllerInput
{
    Vector2 MoveInput { get; }
    Vector2 CameraInput { get; }
    bool JumpInput { get; }
    bool SprintInput { get; }
}
