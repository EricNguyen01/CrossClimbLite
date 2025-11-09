using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class QuitGameButton : MonoBehaviour
    {
        public static void QuitToDesktop()
        {
            Application.Quit();
        }
    }
}
