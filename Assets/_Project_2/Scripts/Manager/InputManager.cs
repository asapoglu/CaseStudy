// Girdi Yöneticisi
namespace Abdurrahman.Project_2.Core.Managers
{
    using Abdurrahman.Project_2.Core.Interfaces;
    using UnityEngine;
    using Zenject;
    
    public class InputManager : MonoBehaviour
    {
        public bool IsInputReceived()
        {
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }
            
            return false;
        }
    }
}