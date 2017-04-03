using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Author: Oliver Nilsson
    Last edited: 2017/30/03

    SUMMARY
    Instansiates and launches an object on a timer when active
 */ 


public class PillarCannon : AbstractInteractable {

    #region Serialized Variables
    [SerializeField]
    private GameObject m_objectToLaunch;

    [SerializeField]
    private float m_launchTimer;
    [SerializeField]
    private float m_launchForce;
    [SerializeField]
    private float m_objectTimer;
    [SerializeField]
    private float m_setActiveTimer;
    [SerializeField]
    private float m_setPauseTimer;
    [SerializeField]
    private float m_setDelayedStart;

    [SerializeField]
    private bool m_shootOnce;
    [SerializeField]
    private bool m_activateOnce;
    [SerializeField]
    private bool m_alwaysActive;
    [SerializeField]
    private bool m_startActive;

    #endregion


    private List<GameObject> m_objects = new List<GameObject>();

    private bool m_active = false;
    private bool m_used = false;
    private bool m_useActiveTimer;
    private bool m_usePauseTimer;
    private bool m_delayedStart;

    private float m_timer = 0f;
    private float m_activeTimer = 0f;
    private float m_pauseTimer = 0f;

	void Start () {
        m_timer = m_launchTimer;
        m_activeTimer = m_setActiveTimer;
        m_useActiveTimer = m_activeTimer > 0 ? true : false;
        m_pauseTimer = m_setPauseTimer;
        m_delayedStart = m_setDelayedStart > 0 ? true : false;

        if (m_alwaysActive || m_startActive)
        {
            m_active = true;
        }

        m_delayedStart = m_setDelayedStart > 0 ? true : false;
        
    }
	
	void Update () {
        if(m_delayedStart)
        {
            StartCoroutine(delay());
            m_delayedStart = false;
        }

        if (!m_active)
        {
            if (!m_usePauseTimer)
                return;

            m_pauseTimer -= Time.deltaTime;
            if (m_pauseTimer <= 0)
            {
                m_active = true;
                m_activeTimer = m_setActiveTimer;
            }
            else
                return;
        }
            

        if(m_useActiveTimer)
        {
            m_activeTimer -= Time.deltaTime;
            if (m_activeTimer <= 0)
            {
                m_active = false;
                m_pauseTimer = m_setPauseTimer;
                m_usePauseTimer = m_pauseTimer > 0 ? true : false;
                return;
            }
        }


        if(m_timer > 0)
        {
            m_timer -= Time.deltaTime;
            return;
        }

        launchPillar();
        m_timer = m_launchTimer;
    }

    private void launchPillar()
    {
        GameObject g = Instantiate(m_objectToLaunch, transform.position, transform.rotation);
        g.GetComponent<p_Timer>().setDestroyTimer(m_objectTimer);
        g.GetComponent<Rigidbody>().AddForce(transform.up * m_launchForce * 10f);
        

        if (m_shootOnce)
            m_active = false;

        if (m_activateOnce)
            m_used = false;

    }

    public override void Interact()
    {
        if (m_used)
            return;

        if (m_usePauseTimer && !m_active)
            return;
        else if (m_useActiveTimer && m_active)
            return;

        m_activeTimer = m_setActiveTimer;
        m_pauseTimer = m_setPauseTimer;

        m_active = !m_active;
    }

    public override void Signal()
    {
        m_active = !m_active;
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(m_setDelayedStart);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 5);
    }
}
