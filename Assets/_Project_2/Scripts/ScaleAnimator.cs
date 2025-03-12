// Özelleştirilmiş MonoBehaviour Sınıfları
namespace Abdurrahman.Project_2.Core.Components
{
    using UnityEngine;
    using DG.Tweening;

    // Ölçek animasyonu yapan bileşen
    public class ScaleAnimator : MonoBehaviour
    {
        [SerializeField] private float _scale = 1.2f;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _delay = 0f;
        
        private Tween _scaleTween;
        
        private void OnEnable()
        {
            // Ölçek animasyonunu başlat
            _scaleTween = transform.DOScale(Vector3.one * _scale, _duration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(_delay);
        }
        
        private void OnDisable()
        {
            // Animasyonu durdur ve temizle
            _scaleTween?.Kill();
            _scaleTween = null;
        }
    }
}