// Özelleştirilmiş MonoBehaviour Sınıfları
namespace Abdurrahman.Project_2.Core.Components
{
    using UnityEngine;

    // Animasyonları yöneten bileşen
    public class CharacterAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        
        // Animator parametreleri için hash değerleri
        private static readonly int RunTrigger = Animator.StringToHash("Run");
        private static readonly int DanceTrigger = Animator.StringToHash("Dance");
        private static readonly int FailTrigger = Animator.StringToHash("Fail");
        private static readonly int IdleTrigger = Animator.StringToHash("Idle");
        
        public void Run()
        {
            _animator.ResetTrigger(IdleTrigger);
            _animator.SetTrigger(RunTrigger);
        }
        
        public void Dance()
        {
            _animator.SetTrigger(DanceTrigger);
            _animator.ResetTrigger(RunTrigger);
        }
        
        public void Fail()
        {
            _animator.SetTrigger(FailTrigger);
        }
        
        public void Idle()
        {
            _animator.SetTrigger(IdleTrigger);
            _animator.ResetTrigger(DanceTrigger);
            _animator.ResetTrigger(FailTrigger);
        }
    }
}