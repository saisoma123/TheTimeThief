using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinButtonHandler : MonoBehaviour
{
    public Button buttonTryAgain;
    public Button buttonMenu;
    public AudioClip WinSound;
    public AudioSource WinSource; 
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        buttonTryAgain.onClick.AddListener( delegate { ProcessButton(1); });
        buttonMenu.onClick.AddListener( delegate { ProcessButton(0); });

        WinSource = GetComponent<AudioSource>();
        if (WinSource == null)
        {
            WinSource = gameObject.AddComponent<AudioSource>();
        }
        if (WinSource != null && WinSource != null)
        {
            WinSource.PlayOneShot(WinSound);
        }
    }

    void ProcessButton(int n) 
    {
        if (n == 1) {
            SceneManager.LoadScene(1);
        } else if (n == 0) {
            SceneManager.LoadScene(0);
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
