using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkMessageHandler;

/// <summary>
/// 패킷 송신과 RequestId 발급을 위한 추상 인터페이스
/// </summary>
public interface IMessageSession
{
    /// <summary>완성된 패킷을 송신</summary>
    Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken = default);

    /// <summary>새 RequestId 발급</summary>
    long NextRequestId();

    /// <summary>패킷 작성을 위한 Writer를 빌린다</summary>
    IMessageWriter RentWriter();
}
