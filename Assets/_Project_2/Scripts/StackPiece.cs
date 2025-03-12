// Özelleştirilmiş MonoBehaviour Sınıfları
namespace Abdurrahman.Project_2.Core.Components
{
    using UnityEngine;

    // Platform hareketlerini ve görsellerini kontrol eden bileşen
    public class StackPiece : MonoBehaviour
    {
        private MeshRenderer _meshRenderer;
        private Rigidbody _rigidbody;
        
        public Material Material => _meshRenderer?.material;
        
        private void Awake()
        {
            // Bileşenleri al
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            _rigidbody = GetComponent<Rigidbody>();
            
            // Rigidbody varsa başlangıçta kinematik yap
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }
        }
        
        // Düşme davranışını aktifleştir
        public void ActivateFalling(Vector3 force)
        {
            if (_rigidbody == null) return;
            
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
}