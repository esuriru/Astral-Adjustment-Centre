using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    public void QuitApplication()
    {
        Quit();
    }

    // NOTE - Just put this function into QuitApplication
    private void Quit()
    {
        Application.Quit();
    }
}
