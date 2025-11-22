using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // 이동 속도 (Inspector에서 조정 가능)
    [SerializeField] private float moveSpeed = 5f;
    
    // Rigidbody 2D 참조
    private Rigidbody2D rb;
    
    // 이동 방향 저장
    private Vector2 moveDirection = Vector2.zero;

    void Start()
    {
        // 게임 시작 시 한 번 실행
        // 이 오브젝트의 Rigidbody 2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 매 프레임마다 실행 (입력 처리용)
        var keyboard = Keyboard.current;
        
        // WASD 입력
        float moveX = 0;
        float moveY = 0;
        
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            moveY = 1;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            moveY = -1;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveX = -1;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveX = 1;
        
        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // 매 물리 연산 프레임마다 실행 (일정한 시간 간격)
        // 물리 계산이 필요한 것은 FixedUpdate에서!
        
        // Unity 6의 새로운 문법
        // linearVelocity: 선속도 (물체의 이동 속도)
        rb.linearVelocity = moveDirection * moveSpeed;
    
    }
}
