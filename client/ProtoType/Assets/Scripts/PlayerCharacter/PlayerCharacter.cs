using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
// 플레이어 정보
    [SerializeField] private string characterName = "Player";
    [SerializeField] private Color characterColor = Color.red;
    
    // 플레이어 타입 (로컬/원격 구분)
    public enum PlayerType
    {
        Local,    // 내 플레이어
        Remote    // 다른 플레이어
    }
    
    private PlayerType playerType = PlayerType.Local;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    void Start()
    {
        // 컴포넌트 참조
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();

        if (playerController != null)
        {
            if (playerController.enabled)
            {
                Debug.Log("✅ PlayerController가 활성화되어 있습니다!");
            }
            else
            {
                Debug.Log("❌ PlayerController가 비활성화되어 있습니다!");
            }
        }
        
        // 색상 적용
        if (spriteRenderer != null)
        {
            spriteRenderer.color = characterColor;
        }
        
        Debug.Log($"{characterName} ({playerType}) 스폰됨!");
    }

    // ===== 플레이어 타입 관련 =====
    
    /// <summary>
    /// 플레이어를 Local로 설정 (조종 가능)
    /// </summary>
    public void SetAsLocalPlayer()
    {
        playerType = PlayerType.Local;
        
        // Local 플레이어만 PlayerController 활성화
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        Debug.Log($"{characterName}을(를) Local Player로 설정했습니다!");
    }

    /// <summary>
    /// 플레이어를 Remote로 설정 (다른 플레이어)
    /// </summary>
    public void SetAsRemotePlayer()
    {
        playerType = PlayerType.Remote;
        
        // Remote 플레이어는 PlayerController 비활성화 (네트워크로부터 위치 동기화)
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        Debug.Log($"{characterName}을(를) Remote Player로 설정했습니다!");
    }

    /// <summary>
    /// 현재 플레이어 타입 반환
    /// </summary>
    public PlayerType GetPlayerType() => playerType;

    /// <summary>
    /// Local 플레이어인지 확인
    /// </summary>
    public bool IsLocalPlayer() => playerType == PlayerType.Local;

    /// <summary>
    /// Remote 플레이어인지 확인
    /// </summary>
    public bool IsRemotePlayer() => playerType == PlayerType.Remote;

    // ===== 플레이어 정보 관련 =====

    /// <summary>
    /// 플레이어 정보 출력
    /// </summary>
    public void PrintInfo()
    {
        Debug.Log($"=== {characterName} 정보 ===");
        Debug.Log($"타입: {playerType}");
        Debug.Log($"위치: {transform.position}");
    }

    /// <summary>
    /// 플레이어명 반환
    /// </summary>
    public string GetCharacterName() => characterName;

    /// <summary>
    /// 플레이어명 설정
    /// </summary>
    public void SetCharacterName(string newName)
    {
        characterName = newName;
    }

    /// <summary>
    /// 플레이어 색상 반환
    /// </summary>
    public Color GetCharacterColor() => characterColor;

    /// <summary>
    /// 플레이어 색상 설정
    /// </summary>
    public void SetCharacterColor(Color newColor)
    {
        characterColor = newColor;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = characterColor;
        }
    }

    // ===== Remote 플레이어 위치 동기화 =====

    /// <summary>
    /// Remote 플레이어의 위치 업데이트
    /// (네트워크에서 수신한 위치 데이터로 업데이트)
    /// </summary>
    public void SetRemotePosition(Vector2 newPosition)
    {
        if (playerType != PlayerType.Remote)
        {
            Debug.LogWarning($"{characterName}은(는) Remote Player가 아닙니다!");
            return;
        }
        
        // Remote 플레이어 위치 직접 설정
        transform.position = newPosition;
    }

    /// <summary>
    /// Remote 플레이어의 속도 업데이트
    /// (네트워크에서 수신한 속도 데이터로 업데이트)
    /// </summary>
    public void SetRemoteVelocity(Vector2 newVelocity)
    {
        if (playerType != PlayerType.Remote)
        {
            Debug.LogWarning($"{characterName}은(는) Remote Player가 아닙니다!");
            return;
        }
        
        if (rb != null)
        {
            rb.linearVelocity = newVelocity;
        }
    }

    /// <summary>
    /// 현재 위치 반환
    /// </summary>
    public Vector2 GetPosition() => transform.position;

    /// <summary>
    /// 현재 속도 반환
    /// </summary>
    public Vector2 GetVelocity() => rb != null ? rb.linearVelocity : Vector2.zero;
}
