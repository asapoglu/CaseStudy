// Özelleştirilmiş MonoBehaviour Sınıfları
namespace Abdurrahman.Project_2.Core.Components
{
    using UnityEngine;
    using Abdurrahman.Project_2.Core.Signals;
    using Zenject;

    // Düğmeleri yöneten bileşenler
    public class StartButtonController : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        
        public void OnClick()
        {
            _signalBus.Fire(new GameStartSignal());
        }
    }
}