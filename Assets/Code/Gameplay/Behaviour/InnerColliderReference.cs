using UnityEngine;

namespace Code.Gameplay.Behaviour
{
    public class InnerColliderReference : MonoBehaviour
    {
        [Tooltip("Ссылка на внутренний коллайдер.")]
        public Collider2D innerCollider;
    }
}