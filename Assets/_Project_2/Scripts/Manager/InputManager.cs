namespace Abdurrahman.Project_2.Core.Managers
{
    using UnityEngine;
    
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