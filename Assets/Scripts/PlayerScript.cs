using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool m_drawDebugAlways = true;

    [Header("Input References")]
    [SerializeField] private InputActionReference m_movementInput;
    [SerializeField] private InputActionReference m_jumpInput;
    [SerializeField] private InputActionReference m_reflectInput;

    [Header("Controller Parameters")]
    [SerializeField] private float m_maxLateralVelocity;
    [SerializeField] private float m_acceleration;

    [Space]

    [SerializeField] private float m_jumpImpulse;
    [SerializeField] private float m_verticalJumpCap = 0.5f; // if yvelo above this value, cannot receive a jump impulse (prevents jumping multiple times if input mashed)
    [SerializeField] private float m_jumpStifleMax;

    [Space]

    [SerializeField] private float m_coyoteDuration = 0.05f;

    [Space]

    [SerializeField] private float m_gravity = -9.8f;
    [SerializeField] private float m_stifleGravity = -9.8f * 3;
    [SerializeField] private float m_maxFallSpeed = -3f;

    [Header("Boxcast Settings")]
    [SerializeField] private Transform m_boxcastOrigin;
    [SerializeField] private Vector2 m_boxcastSize;
    [SerializeField] private float m_travelDistance;
    [SerializeField] private LayerMask m_validGround;

    private Rigidbody2D m_rigidbody;
    private ReflectionScript m_reflBehavior;

    private float m_normalizedLateralInput;
    private bool m_isJumpDown;
    private bool m_isReflectPerformed;

    private float m_lateralVelocity;
    private float m_verticalVelocity;

    private float m_currentGravity;
    private bool m_wasGroundedPreviousFrame;
    private float m_coyoteTimestamp;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_reflBehavior = GetComponent<ReflectionScript>();
        m_currentGravity = m_gravity;
    }

    private void Update()
    {
        GatherInput();

        // TODO reflection
        // DEBUG
        if (m_reflectInput.action.WasPerformedThisFrame()) 
        {
            if (m_reflBehavior.HasCopy())
            {
                m_reflBehavior.PasteRegion();
            }
            else
            {
                m_reflBehavior.CopyRegion();
            }
        }
    }

    private void FixedUpdate()
    {
        // using physframe timescale, not frame timescale
        float timescale = Time.fixedDeltaTime;

        // compute x and y velo for physframe
        ComputeLateralMovement(timescale);
        ComputeVerticalVelocity(timescale);

        m_rigidbody.MovePosition(m_rigidbody.position + new Vector2(m_lateralVelocity * timescale, m_verticalVelocity * timescale));
    }

    private void GatherInput()
    {
        m_normalizedLateralInput = m_movementInput.action.ReadValue<float>(); // inputaction asset processor auto-normalizes
        m_isJumpDown = m_jumpInput.action.IsPressed();
        m_isReflectPerformed = m_reflectInput.action.WasPerformedThisFrame();
    }

    private void ComputeLateralMovement(float timescale)
    {
        // change lateral velocity by moving it towards the target value, changing it at most by the acceleration for the frame
        m_lateralVelocity = Mathf.MoveTowards(m_lateralVelocity, m_normalizedLateralInput * m_maxLateralVelocity, m_acceleration * timescale);
    }

    /// <summary>
    /// Handles computing the vertical velocity for this player on this frame. Performs a boxcast to check for a ground, allowing
    /// the jump functionality (adding a vertical impulse) if the player is in a coyote time period or grounded. If not grounded,
    /// gravity is applied (using the stifle variant if the user is no longer holding the jump bind).
    /// 
    /// Coyote time is determined in a separate method, HandleCoyoteTime.
    /// </summary>
    /// <param name="timescale"></param>
    private void ComputeVerticalVelocity(float timescale)
    {
        bool is_grounded = BoxcastCheck(out var _); // discard out bc we dont need it

        bool is_under_vertical_cap = m_verticalVelocity <= m_verticalJumpCap;

        // if grounded, apply gravity accel for frame
        if (!is_grounded)
        {
            m_verticalVelocity += m_currentGravity * timescale; 
        }
        else if (m_verticalVelocity <= 0.05f) // if grounded and we're not currently moving up (i.e. just jumped), set yvelo to 0.
        {
            m_verticalVelocity = 0f;
        }

        HandleCoyoteTime(is_grounded);

        // if (grounded or in coyote time), jump down, and under the yvelo limit to jump, apply impulse
        bool jump_eligible = is_grounded || IsInCoyoteTime();
        if (jump_eligible && m_isJumpDown && is_under_vertical_cap)
        {
            m_verticalVelocity += m_jumpImpulse; // don't scale this as it is an impulse
        }

        // behavior to handle cutting off jumps early
        bool is_within_jumpstifle_range = m_verticalVelocity < m_jumpStifleMax && m_verticalVelocity > 0f;
        if (!is_grounded && !m_isJumpDown && is_within_jumpstifle_range) // if we release jump early and are within stifling range, increase gravity
        {
            m_currentGravity = m_stifleGravity;
        }
        else // otherwise use normal gravity
        {
            m_currentGravity = m_gravity;
        }

        // cap velocity between max fall speed and max rise speed (impulse)
        m_verticalVelocity = Mathf.Clamp(m_verticalVelocity, m_maxFallSpeed, m_jumpImpulse);

        m_wasGroundedPreviousFrame = is_grounded;
    }

    private bool BoxcastCheck(out RaycastHit2D hit)
    {
        hit = Physics2D.BoxCast(m_boxcastOrigin.position, m_boxcastSize, 0f, Vector2.down, m_travelDistance, m_validGround);

        return hit.collider != null;
    }

    private void HandleCoyoteTime(bool is_grounded_this_frame)
    {
        // if grounded state change...
        if (m_wasGroundedPreviousFrame != is_grounded_this_frame)
        {
            // if we're airborne and we've walked off of something...
            if (!is_grounded_this_frame && m_verticalVelocity <= 0f)
            {
                // begin coyote time
                m_coyoteTimestamp = Time.time;
            }

            // OTHER CASES:
            // if not grounded and velo is positive, we're rising from a jump
            // if grounded, we've just landed.
        }
    }

    private bool IsInCoyoteTime()
    {
        return Time.time < m_coyoteTimestamp + m_coyoteDuration;
    }

    #region Debug
    private void OnDrawGizmos()
    {
        if (m_drawDebugAlways) DebugDraw();
    }

    private void OnDrawGizmosSelected()
    {
        if (!m_drawDebugAlways) DebugDraw();
    }

    private void DebugDraw()
    {
        if (m_boxcastOrigin == null) return;

        Gizmos.color = BoxcastCheck(out var _) ? Color.green : Color.red;

        var box_size = new Vector3(m_boxcastSize.x, m_boxcastSize.y, 1f);
        var target_pos = m_boxcastOrigin.position + Vector3.down * m_travelDistance;
        for (int i = 0; i < 11; ++i)
        {
            // draws 11 boxes so that there's one at the start and one at the end
            var locus = Vector3.Lerp(m_boxcastOrigin.position, target_pos, i / 10.0f);

            Gizmos.DrawWireCube(locus, box_size);
        }
    }
    #endregion
}
