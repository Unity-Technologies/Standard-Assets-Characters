using System;
using UnityEngine;

namespace StandardAssets.Characters.Input
{
    public interface INewInput
    {
        Vector2 moveInput { get; }
        bool isMoveInput { get; }
        Vector2 lookInput { get; }
    }
}