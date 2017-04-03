using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Author: Oliver Nilsson
    Last edited: 2017/02/04

    SUMMARY
    Pillar travels from point to point, needs to be activated at each point
 */

public class PillarPointbyPoint : AbstractInteractable
{
    #region Serialized Variables
    [SerializeField]private List<Transform> m_transforms;
    [SerializeField]private List<AbstractInteractable> m_signalList;

    [Header("Curve")]
    [Range(0.1f, 10)]
    [SerializeField]private float m_curveMultiplier;
    [SerializeField]private List<AnimationCurve> m_curve;

    [SerializeField]private bool m_reverse;
    [SerializeField]private bool m_usePhysics;

    [Header("Rotation")]
    [SerializeField]private bool m_rotateToTarget;
    [SerializeField]private float m_rotationSpeed;

    [Header("Speed")]
    [SerializeField]private bool m_useMinSpeed;
    [SerializeField]private bool m_constantSpeed;
    [SerializeField]private float m_initalSpeed;
    [SerializeField]private float m_minSpeed;
    #endregion

    private bool m_active = false;
    private bool m_inRange = false;
    private bool m_end = false;
    private bool m_goingBack = false;

    private Rigidbody m_rigidbody;

    private float m_distance = 0f;
    private float m_currentMaxDist = 0;

    private List<Vector3> m_targets;
    private Vector3 m_direction;

    private int m_currentNum = 1;

    void Start()
    {
        m_targets = new List<Vector3>();

        foreach (Transform t in m_transforms)
            m_targets.Add(t.position);
        

        m_rigidbody = GetComponent<Rigidbody>();
        m_distance = Vector3.Distance(transform.position, m_targets[m_currentNum]);
        m_currentMaxDist = m_distance;
        m_direction = m_targets[m_currentNum] - transform.position;
    }

    void Update()
    {
        if (!m_active) return;

        if (m_rotateToTarget)
        {
            var targetDir = m_targets[m_currentNum] - transform.position;
            var rot = Vector3.RotateTowards(transform.forward, targetDir, m_rotationSpeed * Time.deltaTime, 0);
            transform.rotation = Quaternion.LookRotation(rot);
        }

        if (CheckDistance())
        {
            EditTarget();
        }
        if (!m_active) return;

        VelocityUpdate();
    }

    private void EditTarget()
    {
        if (m_targets.Count > 1 && m_currentNum < m_targets.Count && !m_goingBack)
        {
            m_currentNum++;
            m_active = false;
            m_rigidbody.isKinematic = true;
        }
        else if (m_reverse)
        {
            m_currentNum--;
            m_active = false;
            m_rigidbody.isKinematic = true;

            if(m_currentNum == -1)
            {
                m_currentNum = 1;
                m_goingBack = false;
            }
        }

        if (m_currentNum == m_targets.Count && !m_reverse)
        {
            m_end = true;
            m_active = false;
            m_rigidbody.isKinematic = true;
        }
        else if (m_currentNum == m_targets.Count && m_reverse)
        {
            m_currentNum = m_targets.Count - 2;
            m_goingBack = true;
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

        m_distance = Vector3.Distance(transform.position, m_targets[m_currentNum]);

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
        {
            m_rigidbody.velocity = (m_direction * speed);
            return;
        }
        else
        {
            transform.position += (m_direction * speed * Time.deltaTime);
        }
    }

    public override void Interact()
    {
        if (m_active || m_end)
            return;

        foreach (AbstractInteractable t in m_signalList)
            t.GetComponent<AbstractInteractable>().Signal();


        m_active = true;
        m_direction = m_targets[m_currentNum] - transform.position;
        m_distance = Vector3.Distance(transform.position, m_targets[m_currentNum]);
        m_currentMaxDist = m_distance;
        if (m_usePhysics)
            m_rigidbody.isKinematic = false;
    }

    public override void Signal()
    {
        if (!m_active)
        {
            if(m_end)
                return;

            m_active = true;
            m_direction = m_targets[m_currentNum] - transform.position;
            m_distance = Vector3.Distance(transform.position, m_targets[m_currentNum]);
            m_currentMaxDist = m_distance;
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

        try
        {
            Gizmos.DrawCube(m_transforms[m_transforms.Count - 1].position, new Vector3(1, 1, 1));
        }
        catch (Exception)
        {
            throw;
        }

    }
}
