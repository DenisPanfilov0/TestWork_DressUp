using System;
using UnityEngine;

namespace Code.Gameplay.Services
{
    public interface IFallService
    {
        void StartFall(RectTransform rectTransform, Action<bool> onCollisionDetected);
    }
}