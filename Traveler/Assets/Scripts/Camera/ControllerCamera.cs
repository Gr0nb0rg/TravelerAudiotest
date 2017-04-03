using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
    Author: Ludwig Gustavsson, Ludvig Grönborg
    Last Edited: 2017/03/26
*/

public enum Mode
{
    FollowPlayer,
    LookAt,
    Static
}

public class ControllerCamera : MonoBehaviour
{
    //Public vars
    [SerializeField]
    private LayerMask m_zoomMask;
    [SerializeField]
    private Mode m_mode = Mode.FollowPlayer;
    [SerializeField]
    private Vector3 m_offset;
    [SerializeField]
    private Vector2 m_sensitivity = new Vector2(5, 5);
    [SerializeField]
    private bool m_invertY = false;
    [SerializeField]
    private Vector2 m_cameraLimitsY = new Vector2(40, 160);
    [SerializeField]
    private float m_playerDistance = 2.0f;
    [SerializeField]
    private Material[] m_playerMats;

    private Transform[] m_lookAtTransforms;

    //Component vars
    private Transform m_player;
    private SkinnedMeshRenderer m_playerRenderer;

    //Rotation vars
    private float m_absoluteY;
    private float m_absoluteX;
    private int m_invertVal = 1;

    //Zoom vars
    private RaycastHit m_rayHit;
    private Vector3 m_desiredPosition;
    private Vector3 m_target;
    private Vector3 m_startOffset;
    private Vector3 m_nonZoomPosition;

    //Input vars
    private Vector2 m_inputs;

    //Pause vars
    private bool m_isPaused = false;

	void Start ()
    {
        CameraInverted = m_invertY;

        m_player = GameObject.Find("Player").transform;
        m_playerRenderer = m_player.GetComponentInChildren<SkinnedMeshRenderer>();
        m_startOffset = m_offset;

        for (int i = 0; i < m_playerMats.Length; i++)
            m_playerMats[i].SetInt("_ZWrite", 1);
    }

    void LateUpdate()
    {
        if (!m_isPaused)
            ModeUpdate();
    }

    void ModeUpdate()
    {
        //Get input values
        m_inputs = new Vector2(Input.GetAxis("Mouse X") * m_sensitivity.x, Input.GetAxis("Mouse Y") * m_invertVal * m_sensitivity.y);

        //Set absolute X
        m_absoluteX += m_inputs.x;
        if (m_absoluteX > 360)
            m_absoluteX -= 360;
        else if (m_absoluteX < -360)
            m_absoluteX += 360;

        //Set absolute Y
        m_absoluteY += m_inputs.y;
        m_absoluteY = Mathf.Clamp(m_absoluteY, m_cameraLimitsY.x, m_cameraLimitsY.y);

        //Get player rotation and set camera rotation/position relative to Y input and player rotation
        Quaternion rot = Quaternion.Euler(m_absoluteY, m_absoluteX, 0);

        //Rotate camera and change position depending on mode
        switch (m_mode)
        {
            case Mode.FollowPlayer:
                m_nonZoomPosition = m_player.transform.position - (rot * m_startOffset);
                transform.position = m_player.transform.position - (rot * m_offset);

                m_desiredPosition = m_player.transform.position - (rot * m_offset);
                m_target = m_player.transform.position;

                transform.LookAt(m_player.transform.position + rot * new Vector3(m_offset.x, m_offset.y, 0));
                break;

            default:
                break;
        }

        //Raycast from player to camera, set camera to hit point if raycast hits something
        Debug.DrawRay(m_target, m_nonZoomPosition - m_target, Color.green);
        if (Physics.Raycast(m_target, m_nonZoomPosition - m_target, out m_rayHit, (m_nonZoomPosition - m_target).magnitude, m_zoomMask))
        {
            transform.position = (m_rayHit.point - m_target) * 0.8f + m_target;
            Debug.DrawRay(m_rayHit.point, Vector3.up * 2, Color.red);
        }

        //Change player material opacity if camera is too close
        if (Vector3.Distance(transform.position, m_player.transform.position) < m_playerDistance)
        {
            for (int i = 0; i < m_playerMats.Length; i++)
            {
                Color col = m_playerMats[i].color;
                col.a = Mathf.Lerp(col.a, 0.3f, 6 * Time.deltaTime);
                m_playerMats[i].color = col;
            }
        }
        else
        {
            for (int i = 0; i < m_playerMats.Length; i++)
            {
                Color col = m_playerMats[i].color;
                col.a = Mathf.Lerp(col.a, 1.0f, 6 * Time.deltaTime);
                m_playerMats[i].color = col;
            }
        }
    }

    bool IsColliding()
    {
        float distance = 1f;

        Vector3 tempV = Vector3.zero;
        Vector3 v = transform.position;
        Ray ray = new Ray();

        for (int i = 0; i < 6; i++)
        {
            switch (i)
            {
                case (0):
                    tempV = transform.forward;
                    break;

                case (1):
                    tempV = -tempV;
                    break;

                case (2):
                    tempV = transform.right;
                    break;

                case (3):
                    tempV = -tempV;
                    break;

                case (4):
                    tempV = transform.up;
                    break;

                case (5):
                    tempV = -tempV;
                    break;

                default:
                    break;
            }
            ray = new Ray(v, tempV);
            Debug.DrawRay(v, tempV * distance, Color.red);
            if (Physics.Raycast(ray, distance))
            {
                return true;
            }
        }

        return false;
    }

    void SetMode(Mode newMode)
    {
        m_mode = newMode;
    }

    public Mode GetMode()
    {
        return m_mode;
    }

    public Vector2 GetInput()
    {
        return m_inputs;
    }

    public void SetPaused(bool state)
    {
        m_isPaused = state;
    }

    public bool GetIsPaused()
    {
        return m_isPaused;
    }


    private Vector2 CameraSensitivity { get { return m_sensitivity; } set { m_sensitivity = value; } }
    private bool CameraInverted { get { return (m_invertVal > 0); } set { m_invertVal = (value == true) ? 1 : -1; } }
}
