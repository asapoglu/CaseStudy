// Özelleştirilmiş MonoBehaviour Sınıfları
namespace Abdurrahman.Project_2.Core.Components
{
    using UnityEngine;
    using Abdurrahman.Project_2.Core.Signals;
    using Zenject;

    public class ContinueButtonController : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        
        public void OnClick()
        {
            _signalBus.Fire(new ContinueSignal());
        }
    }
}