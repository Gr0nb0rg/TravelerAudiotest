using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Authors: Ludvig Grönborg
    Last Edited: 2017/04/02
*/

/*
    SUMMARY
    Displays a gizmo icon and let's designers save notes as a string 
*/

public class Note : MonoBehaviour {

    [SerializeField]
    private string m_noteMessage;

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Note_Gizmo.svg");
    }
}
