using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private BluetoothConnect _bluetoothConnect;
    
    public void LoadNextScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void LoadStartScene() => SceneManager.LoadScene(0);

    public void LoadSettings() => SceneManager.LoadScene("Settings");

    public void LoadColourMatching() => SceneManager.LoadScene("Colour Matching");

    public void LoadEllipse() => SceneManager.LoadScene("Ellipse");

    public void LoadTarget() => SceneManager.LoadScene("Target");

    public void LoadHunting() => SceneManager.LoadScene("Hunting");

    public void ConnectToBoard() => Instantiate(_bluetoothConnect, new Vector3(0, 0, 0), Quaternion.identity);

    public void QuitGame() => Application.Quit();
}
