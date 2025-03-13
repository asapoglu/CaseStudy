// Özelleştirilmiş MonoBehaviour Sınıfları
namespace Abdurrahman.Project_2.Core.Components
{
    using UnityEngine;
    using Abdurrahman.Project_2.Core.Signals;
    using Zenject;

    public class ButtonController : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        
        public void OnClickContinue()
        {
            _signalBus.Fire(new ContinueSignal());
        }
        public void OnClickReplay()
        {
            _signalBus.Fire(new ReplaySignal());
        }
        public void OnClickStart()
        {
            _signalBus.Fire(new GameStartSignal());
        }
        public void OnClickFail()
        {
            _signalBus.Fire(new GameFailSignal());
        }
    }
}