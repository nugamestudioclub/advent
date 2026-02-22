using System.Collections;
using UnityEngine;

public class LitePlayerAnimator : MonoBehaviour
{
    private enum State
    {
        Idle,
        Walk,
        Jump,
        Fall
    }

    [SerializeField] private SpriteRenderer m_animatedRenderer;

    [Space]

    [SerializeField] private float m_magnitudeThreshold = 0.05f;

    [Header("Single Frame")]

    [SerializeField] private Sprite m_idleSprite;
    [SerializeField] private Sprite m_risingSprite;
    [SerializeField] private Sprite m_airborneSprite;

    [Header("Walking")]

    [SerializeField] private float m_frameDuration;
    [SerializeField] private Sprite[] m_walkSprites;

    private PlayerScript m_observed;
    private State m_state;
    private State m_previousState;

    private bool m_previousFlipState;

    private Coroutine m_animatingRoutine;

    private void Awake()
    {
        m_observed = FindFirstObjectByType<PlayerScript>();
        if (m_observed == null)
        {
            Debug.LogError("No player found in scene!");
            return;
        }

        m_observed.OnVelocityUpdate += AnimatorUpdate;
    }

    private void OnDestroy()
    {
        if (m_observed != null) m_observed.OnVelocityUpdate -= AnimatorUpdate;
    }

    private void Start()
    {
        m_animatedRenderer.sprite = m_idleSprite;
    }

    private void Update()
    {
        if (m_state == m_previousState) return;

        if (m_previousState == State.Walk) StopCoroutine(m_animatingRoutine);

        switch (m_state)
        {
            case State.Idle:
                m_animatedRenderer.sprite = m_idleSprite;
                break;

            case State.Jump:
                m_animatedRenderer.sprite = m_risingSprite;
                break;

            case State.Fall:
                m_animatedRenderer.sprite = m_airborneSprite;
                break;

            case State.Walk:
                m_animatingRoutine = StartCoroutine(IE_Animate(m_walkSprites, m_frameDuration));
                break;

            default:
                Debug.LogError("State not supported: " + m_state);
                break;
        }

        m_previousState = m_state;
    }

    private IEnumerator IE_Animate(Sprite[] frames, float frame_duration)
    {
        int frame = 0;
        while (true)
        {
            m_animatedRenderer.sprite = frames[frame];

            frame = (frame + 1) % frames.Length;

            yield return new WaitForSeconds(frame_duration);
        }
    }

    private void AnimatorUpdate(Vector2 new_velo, bool grounded)
    {
        float x_mag = Mathf.Abs(new_velo.x);
        float y_mag = Mathf.Abs(new_velo.y);

        m_animatedRenderer.flipX = x_mag != 0f ? (new_velo.x / x_mag) < 0f : m_previousFlipState;
        m_previousFlipState = m_animatedRenderer.flipX;

        if (!grounded || y_mag > m_magnitudeThreshold)
        {
            m_state = y_mag == 0f || (new_velo.y / y_mag < 0) ? State.Fall : State.Jump;
            return;
        }

        if (grounded && x_mag > m_magnitudeThreshold)
        {
            m_state = State.Walk;
            return;
        }

        m_state = State.Idle;
        return;
    }
}
