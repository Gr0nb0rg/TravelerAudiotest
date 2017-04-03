using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    [SerializeField]
    [Range(0, 0.4f)]
    private float m_deadzoneLimit;

	void Start () {
		
	}

    public bool MovingCharacter()
    {
        return (
               (Input.GetAxisRaw("Horizontal") > m_deadzoneLimit) 
            || (Input.GetAxisRaw("Horizontal") < -m_deadzoneLimit)
            || (Input.GetAxisRaw("Vertical") > m_deadzoneLimit) 
            || (Input.GetAxisRaw("Vertical") < -m_deadzoneLimit));
    }

    public bool Jumping()
    {
        return (Input.GetKeyDown(KeyCode.Space));
    }
}
