using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Author: Oliver Nilsson
    Last edited: 2017/30/03

    SUMMARY
    Used for objects launched by PillarCannon
 */

public class p_Timer : MonoBehaviour {


    private float m_timer = 0f;
    private float m_destroyTimer;

	void Update () {
        m_timer += Time.deltaTime;
        
        if (m_timer >= m_destroyTimer)
            Destroy(gameObject);
	}

    public void setDestroyTimer(float t)
    {
        m_destroyTimer = t;
    }
}
