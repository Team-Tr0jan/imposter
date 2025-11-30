using System;
using System.Buffers.Binary;
using MessagePack;

namespace NetworkMessageHandler;

/// <summary>
/// BodySize 기준으로 프레이밍된 단일 패킷을 읽기 위한 Reader 추상화
/// </summary>
public interface IMessageReader
{
    uint MessageId { get; }
    byte Flags { get; }
    bool HasRequestId { get; }
    long? RequestId { get; }

    /// <summary>Payload를 MessagePack으로 역직렬화</summary>
    T ReadPayload<T>(MessagePackSerializerOptions? options = null);
}

/// <summary>
/// 합의된 헤더 포맷을 파싱하는 기본 구현
/// </summary>
public sealed class PacketReader : IMessageReader
{
    private readonly ReadOnlyMemory<byte> _packet;
    private readonly int _bodyOffset;
    private readonly int _payloadOffset;

    public PacketReader(ReadOnlyMemory<byte> packet)
    {
        _packet = packet;
        var span = packet.Span;

        if (span.Length < MessageConstants.BodySizeFieldLength + MessageConstants.MinimalBodySizeWithoutRequestId)
            throw new InvalidOperationException("Packet too small.");

        int bodySize = BinaryPrimitives.ReadInt32LittleEndian(span);
        int expectedTotal = MessageConstants.BodySizeFieldLength + bodySize;
        if (bodySize < MessageConstants.MinimalBodySizeWithoutRequestId || bodySize > MessageConstants.MaxBodySize)
            throw new InvalidOperationException($"Invalid BodySize {bodySize}.");
        if (span.Length < expectedTotal)
            throw new InvalidOperationException("Incomplete packet received.");
        if (span.Length > expectedTotal)
            throw new InvalidOperationException("Packet contains extra bytes beyond BodySize.");

        int offset = MessageConstants.BodySizeFieldLength;
        MessageId = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, MessageConstants.MessageIdSize));
        offset += MessageConstants.MessageIdSize;

        Flags = span[offset];
        offset += MessageConstants.FlagsSize;

        if ((Flags & MessageConstants.FlagHasRequestId) != 0)
        {
            if (bodySize < MessageConstants.MinimalBodySizeWithRequestId)
                throw new InvalidOperationException("BodySize too small for RequestId.");
            RequestId = BinaryPrimitives.ReadInt64LittleEndian(span.Slice(offset, MessageConstants.RequestIdSize));
            offset += MessageConstants.RequestIdSize;
            HasRequestId = true;
        }

        _bodyOffset = MessageConstants.BodySizeFieldLength;
        _payloadOffset = offset;
        _payloadMemory = packet.Slice(_payloadOffset, bodySize - (_payloadOffset - _bodyOffset));
    }

    public uint MessageId { get; }
    public byte Flags { get; }
    public bool HasRequestId { get; }
    public long? RequestId { get; }

    private readonly ReadOnlyMemory<byte> _payloadMemory;

    public T ReadPayload<T>(MessagePackSerializerOptions? options = null)
    {
        return MessagePackSerializer.Deserialize<T>(_payloadMemory, options);
    }
}
