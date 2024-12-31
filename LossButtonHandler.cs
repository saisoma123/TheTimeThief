using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LossButtonHandler : MonoBehaviour
{
    public Button buttonTryAgain;
    public Button buttonMenu;
    public AudioClip gOverSound;
    public AudioSource gOverSource; 

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        buttonTryAgain.onClick.AddListener( delegate { ProcessButton(1); });
        buttonMenu.onClick.AddListener( delegate { ProcessButton(0); });

        gOverSource = GetComponent<AudioSource>();
        if (gOverSource == null)
        {
            gOverSource = gameObject.AddComponent<AudioSource>();
        }
        if (gOverSound != null && gOverSource != null)
        {
            gOverSource.PlayOneShot(gOverSound);
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
