using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // 이동 속도 (Inspector에서 조정 가능)
    [SerializeField] private float moveSpeed = 5f;
    
    
    // Rigidbody 2D 참조
    private Rigidbody2D rb;
    public InputActionReference moveAction;   // Move 액션 참조
    
    // 이동 방향 저장
    private Vector2 moveDirection = Vector2.zero;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        moveAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
    }

    void Update()
    {
    Vector2 rawInput = moveAction.action.ReadValue<Vector2>();

    // 입력이 거의 0이면 멈춤
    if (rawInput.sqrMagnitude < 0.0001f)
    {
        rb.linearVelocity = Vector2.zero;
        return;
    }

    // 방향만 추출 (길이 1)
    Vector2 dir = rawInput.normalized;

    // 항상 일정한 속도
    rb.linearVelocity = dir * moveSpeed;
    Debug.Log("speed = " + rb.linearVelocity.magnitude);
    }
}
