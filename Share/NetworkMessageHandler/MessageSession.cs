using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkMessageHandler;

/// <summary>
/// 패킷 송신과 RequestId 발급을 담당하는 기본 구현
/// 실제 전송(TCP 등)은 하위 클래스에서 SendAsyncCore를 구현한다
/// </summary>
public abstract class MessageSession : IMessageSession
{
    // 필드값을 누락했을 경우 default value로 0이 채워지므로 해당 케이스 검출을 위해 실제 RequestId 발급은 1부터
    private long _requestId = 0;

    public IMessageWriter RentWriter() => new PacketWriter();

    public long NextRequestId()
    {
        var next = Interlocked.Increment(ref _requestId);
        if (next == 0)
        {
            next = Interlocked.Increment(ref _requestId);
        }
        return next;
    }

    public Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken = default)
        => SendAsyncCore(packet, cancellationToken);

    /// <summary>
    /// 실제 전송(TCP 소켓 등)을 하위 클래스에서 구현한다
    /// - 취소 토큰 존중
    /// - 버퍼 재사용(pool) 고려
    /// - 전송 실패 시 로깅/전파 방식을 명확히
    /// </summary>
    protected abstract Task SendAsyncCore(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken);
}
