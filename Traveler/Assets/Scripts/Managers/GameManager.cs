using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("Editor settings")]
    [SerializeField]
    bool m_hideCursor = true;

	void Start () {
        SetCursorMode();
    }

    void SetCursorMode()
    {
        if (m_hideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
