namespace NetworkMessageHandler;

/// <summary>
/// 커스텀 TCP 패킷 포맷 상수
/// 본문 구조(리틀엔디안)
/// [int32 BodySize][uint32 MessageId][byte Flags][int64 RequestId?][Payload(MessagePack)]
/// BodySize는 자기 자신(4바이트)을 포함하지 않는다
/// </summary>
public static class MessageConstants
{
    /// <summary>BodySize 필드 길이(바이트)</summary>
    public const int BodySizeFieldLength = sizeof(int);

    /// <summary>MessageId 크기: 4바이트(uint32)</summary>
    public const int MessageIdSize = sizeof(uint);

    /// <summary>Flags 크기: 1바이트, bit0=1이면 RequestId 포함</summary>
    public const int FlagsSize = sizeof(byte);

    /// <summary>RequestId 크기: 8바이트(int64), 있을 때만 사용</summary>
    public const int RequestIdSize = sizeof(long);

    /// <summary>Flags bit: RequestId 포함 여부(1=있음)</summary>
    public const byte FlagHasRequestId = 0b_0000_0001;

    /// <summary>허용 최대 BodySize(MessageId~Payload): 64KB, 과도한 할당 방지용</summary>
    public const int MaxBodySize = 64 * 1024; // 64KB

    /// <summary>RequestId가 없을 때 최소 BodySize</summary>
    public const int MinimalBodySizeWithoutRequestId = MessageIdSize + FlagsSize;

    /// <summary>RequestId가 있을 때 최소 BodySize</summary>
    public const int MinimalBodySizeWithRequestId = MinimalBodySizeWithoutRequestId + RequestIdSize;
}
