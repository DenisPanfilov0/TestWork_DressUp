using System;
using UnityEngine;

namespace Code.Gameplay.Services
{
    public class MouseDirectionService : IMouseDirectionService
    {
        private Action _onStopTracking;
        private bool _isTracking;

        public event Action<Vector3> OnTargetPositionUpdated;

        public void StartTracking(Action onStopTracking)
        {
            _onStopTracking = onStopTracking;
            _isTracking = true;

            TrackMousePosition();
        }

        public void StopTracking()
        {
            _isTracking = false;
            _onStopTracking?.Invoke();
        }

        private async void TrackMousePosition()
        {
            while (_isTracking)
            {
                OnTargetPositionUpdated?.Invoke(Input.mousePosition);

                await System.Threading.Tasks.Task.Delay(16);
            }
        }
    }
}