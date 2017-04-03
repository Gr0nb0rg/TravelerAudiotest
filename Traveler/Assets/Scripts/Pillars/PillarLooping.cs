using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
    Author: Oliver Nilsson
    Last edited: 2017/02/04

    SUMMARY
    Looping pillar, loops around the set points
 */

public class PillarLooping : AbstractInteractable
{
    #region Serialized variables
    [SerializeField]private List<Transform> m_transforms;
    [SerializeField]private List<float> m_times;
    [SerializeField]private List<AbstractInteractable> m_signalList;

    [Header("Curve")]
    [Range(0.1f, 10)]
    [SerializeField]private float m_curveMultiplier;
    [SerializeField]private List<AnimationCurve> m_curve;

    [SerializeField]private bool m_alwaysActive;
    [SerializeField]private bool m_stopAtStart;
    [SerializeField]private bool m_usePhysics;
    [SerializeField]private float m_setDelayedStart;
    

    [Header("Rotation")]
    [SerializeField]private bool m_rotateToTarget;
    [SerializeField]private float m_rotationSpeed;

    [Header("Speed")]
    [SerializeField]private bool m_useMinSpeed;
    [SerializeField]private bool m_constantSpeed;
    [SerializeField]private float m_initalSpeed;
    [SerializeField]private float m_minSpeed;

    [Header("Pause")]
    [SerializeField]private bool m_pausable;
    [SerializeField]private float m_setPauseTime;
    #endregion

    private bool m_inRange = false;
    private bool m_active = false;
    private bool m_reachedEnd = false;
    private bool m_paused = false;

    private List<PillarTarget> m_targets;

    private Rigidbody m_rigidbody;

    private float m_time;
    private float m_distance = 0f;
    private float m_currentMaxDist = 0;
    private float m_delayedStart;
    private float m_pauseTimer;

    private Vector3 m_direction;

    private int m_currentNum = 1;

    void Start () {
        m_delayedStart = m_setDelayedStart;

        if (m_alwaysActive)
            m_active = true;

        m_targets = new List<PillarTarget>();

        try
        {
            m_time = m_times[0];
        }
        catch (Exception)
        {
            m_time = 0;
            Debug.Log("ADD TIME");
        }
        for (int i = 0; i < m_transforms.Count; i++)
        {
            try
            {
                m_targets.Add(new PillarTarget(m_transforms[i].position, m_times[i]));
            }
            catch (Exception e)
            {
                m_targets.Add(new PillarTarget(m_transforms[i].position, 0));
                Debug.Log("Exception in adding pillartargets: " + e);
            }

        }

        m_rigidbody = GetComponent<Rigidbody>();

        if (m_rotateToTarget)
        {
            transform.LookAt(m_targets[m_currentNum].m_position);
        }
        m_distance = Vector3.Distance(transform.position, m_targets[m_currentNum].m_position);
        m_currentMaxDist = m_distance;
        m_direction = m_targets[m_currentNum].m_position - transform.position;
    }

    void Update () {
        if (m_paused)
        {
            if (m_setPauseTime == 0)
                return;

            if(m_pauseTimer > 0)
            {
                m_pauseTimer -= Time.deltaTime;
                return;
            }

            m_paused = false;
            m_active = true;
            if (m_usePhysics)
                m_rigidbody.isKinematic = false;
        }
        if (!m_active) return;

        if (m_delayedStart > 0)
        {
            m_delayedStart -= Time.deltaTime;
            return;
        }


        if (m_usePhysics)
            m_rigidbody.isKinematic = false;

        if (m_rotateToTarget)
        {
            var targetDir = m_targets[m_currentNum].m_position - transform.position;
            var rot = Vector3.RotateTowards(transform.forward, targetDir, m_rotationSpeed * Time.deltaTime, 0);
            transform.rotation = Quaternion.LookRotation(rot);
        }

        if (m_time > 0)
        {
            m_time -= Time.deltaTime;
            return;
        }

        if (CheckDistance())
        {
            EditTarget();
            if (m_time > 0)
                return;
        }

        VelocityUpdate();
    }

    private void EditTarget()
    {
        if (m_targets.Count > 1 && m_currentNum == m_targets.Count - 1)
        {
            if (m_stopAtStart)
                m_reachedEnd = true;

            m_time = m_targets[m_targets.Count - 1].m_time;
            m_currentNum = 0;
            m_direction = m_targets[m_currentNum].m_position - transform.position;
            m_distance = Vector3.Distance(transform.position, m_targets[m_currentNum].m_position);
            m_currentMaxDist = m_distance;
        }
        else
        {
            m_currentNum++;
            m_time = m_targets[m_currentNum - 1].m_time;

            if (!m_alwaysActive && m_currentNum == 1 && m_reachedEnd)
            {
                m_active = false;
                m_rigidbody.isKinematic = true;
                return;
            }

            m_direction = m_targets[m_currentNum].m_position - transform.position;
            m_distance = Vector3.Distance(transform.position, m_targets[m_currentNum].m_position);
            m_currentMaxDist = m_distance;
        }
    }
    private bool CheckDistance()
    {
        if (m_distance < 0.4f && !m_inRange)
        {
            m_distance = 0f;
            m_inRange = true;
            return true;
        }

        m_distance = Vector3.Distance(transform.position, m_targets[m_currentNum].m_position);

        if (m_inRange && m_distance > 0.4f)
        {
            m_inRange = false;
        }
        return false;
    }

    private void VelocityUpdate()
    {
        float speed;
        if (m_constantSpeed)
            speed = m_initalSpeed / 25;
        else
            speed = (m_initalSpeed * m_distance) / 25;

        if (speed < m_minSpeed / 25 && m_useMinSpeed && m_distance != 0 && !m_constantSpeed)
            speed = m_minSpeed / 25;

        speed *= (1 + m_curveMultiplier * m_curve[m_currentNum].Evaluate(1 - (m_distance / m_currentMaxDist)));

        if (m_usePhysics)
            m_rigidbody.velocity = (m_direction * speed);
        else
            transform.position += (m_direction * speed * Time.deltaTime);
    }

    public override void Interact()
    {
        if (!m_pausable)
        {
            if (m_active) return;

            foreach (AbstractInteractable t in m_signalList)
                t.GetComponent<AbstractInteractable>().Signal();


            m_active = true;
            if (m_usePhysics)
                m_rigidbody.isKinematic = false;
        }
        else
        {
            if (m_active && !m_paused)
            {
                m_paused = true;
                m_active = false;

                m_pauseTimer = m_setPauseTime;

                m_rigidbody.isKinematic = true;
            }
            else if(!m_active && m_paused && m_setPauseTime == 0)
            {
                foreach (AbstractInteractable t in m_signalList)
                    t.GetComponent<AbstractInteractable>().Signal();
                

                m_active = true;
                m_paused = false;
                if (m_usePhysics)
                    m_rigidbody.isKinematic = false;
            }
            else if(!m_active && !m_paused)
            {
                foreach (AbstractInteractable t in m_signalList)
                    t.GetComponent<AbstractInteractable>().Signal();
                
                m_active = true;
                if (m_usePhysics)
                    m_rigidbody.isKinematic = false;
            }
        }
    }

    public override void Signal()
    {
        if(!m_active)
        {
            m_active = true;
            if (m_usePhysics)
                m_rigidbody.isKinematic = false;
        }
        else
        {
            m_active = false;
            m_rigidbody.isKinematic = true;
        }
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < m_transforms.Count - 1; i++)
        {
            if (m_transforms[i] == null || m_transforms[i + 1] == null)
            {
                break;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_transforms[i].position, m_transforms[i + 1].position);
            Gizmos.DrawCube(m_transforms[i].position, new Vector3(1, 1, 1));
        }

        if (m_transforms.Count > 0)
        {
            try
            {
                Gizmos.DrawLine(m_transforms[0].position, m_transforms[m_transforms.Count - 1].position);
                Gizmos.DrawCube(m_transforms[m_transforms.Count - 1].position, new Vector3(1, 1, 1));
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
