using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // used for portal collision
    private void OnTriggerEnter(Collider other)
    {

        //load menu scene when player enters portal
        if (other.gameObject.tag == "Player")
        {
            //load win scene
            SceneManager.LoadScene(3);
        }

    }
}
