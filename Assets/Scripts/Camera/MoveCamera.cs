// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveCamera : MonoBehaviour
{
    // NOTE - Should be cameraTransform/cameraPositionTransform
    private Transform cameraPosition;

    private void Awake()
    {
        // NOTE - Useless this
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetCameraPosition()
    {
        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            cameraPosition = GameObject.FindGameObjectWithTag("CameraPosition").transform;
        }
    }

    // NOTE - Missing access specifier, remove default comment
    // Update is called once per frame
    void Update()
    {
        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }
    }
}
