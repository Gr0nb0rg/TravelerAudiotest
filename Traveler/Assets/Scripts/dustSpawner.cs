using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Author: Oliver Nilsson
    Last edited: 17/04/01

    SUMMARY:
    Spawns dust
*/


public class dustSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_dust;
    [SerializeField] private Vector3 m_positionAdjustments;

    private List<GameObject> m_dustList = new List<GameObject>();

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 0) //Layer 0 = default
        {
            GameObject g = Instantiate(m_dust, transform.position + m_positionAdjustments, Quaternion.AngleAxis(90, new Vector3(1,0,0)));
            m_dustList.Add(g);
        }
    }
}
