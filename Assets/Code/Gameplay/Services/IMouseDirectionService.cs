using System;
using UnityEngine;

namespace Code.Gameplay.Services
{
    public interface IMouseDirectionService
    {
        event Action<Vector3> OnTargetPositionUpdated;
        void StartTracking(Action onStopTracking);
        void StopTracking();
    }
}