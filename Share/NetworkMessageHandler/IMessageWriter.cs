using System;
using System.Buffers;
using MessagePack;
using System.Runtime.InteropServices;

namespace NetworkMessageHandler;

/// <summary>
/// 패킷 작성용 Writer: 헤더(BodySize) 자리 비워두기 → 필드 쓰기 → BodySize 채우기
/// </summary>
public interface IMessageWriter : IDisposable
{
    /// <summary>시작 위치에 BodySize(4바이트) 자리를 비워둔다</summary>
    void WriteBodySizePlaceholder();

    /// <summary>MessageId(uint32, 리틀엔디안) 쓰기</summary>
    void WriteMessageId(uint messageId);

    /// <summary>Flags(byte) 쓰기</summary>
    void WriteFlags(byte flags);

    /// <summary>RequestId(int64, 리틀엔디안) 쓰기, 호출 전에 Flags bit를 세팅해야 한다</summary>
    void WriteRequestId(long requestId);

    /// <summary>Payload를 MessagePack으로 직렬화해 쓴다</summary>
    void WritePayload<T>(T payload, MessagePackSerializerOptions? options = null);

    /// <summary>placeholder 이후로 쓴 바이트 수를 기반으로 BodySize를 채운다</summary>
    void PatchBodySize();

    /// <summary>송신 가능한 버퍼를 가져온다</summary>
    ReadOnlyMemory<byte> WrittenMemory { get; }
}

/// <summary>
/// <see cref="ArrayBufferWriter{T}"/> 기반 기본 구현
/// </summary>
public sealed class PacketWriter : IMessageWriter
{
    private readonly ArrayBufferWriter<byte> _buffer = new();
    private bool _bodySizePatched;

    public void WriteBodySizePlaceholder()
    {
        EnsureNotPatched();
        _buffer.GetSpan(MessageConstants.BodySizeFieldLength); // capacity 확보
        _buffer.Advance(MessageConstants.BodySizeFieldLength);
    }

    public void WriteMessageId(uint messageId)
    {
        EnsureNotPatched();
        Span<byte> span = _buffer.GetSpan(MessageConstants.MessageIdSize);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(span, messageId);
        _buffer.Advance(MessageConstants.MessageIdSize);
    }

    public void WriteFlags(byte flags)
    {
        EnsureNotPatched();
        Span<byte> span = _buffer.GetSpan(MessageConstants.FlagsSize);
        span[0] = flags;
        _buffer.Advance(MessageConstants.FlagsSize);
    }

    public void WriteRequestId(long requestId)
    {
        EnsureNotPatched();
        Span<byte> span = _buffer.GetSpan(MessageConstants.RequestIdSize);
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(span, requestId);
        _buffer.Advance(MessageConstants.RequestIdSize);
    }

    public void WritePayload<T>(T payload, MessagePackSerializerOptions? options = null)
    {
        EnsureNotPatched();
        MessagePackSerializer.Serialize(_buffer, payload, options);
    }

    public void PatchBodySize()
    {
        if (_bodySizePatched)
            throw new InvalidOperationException("BodySize already patched.");

        int totalLength = _buffer.WrittenCount;
        if (totalLength < MessageConstants.BodySizeFieldLength + MessageConstants.MinimalBodySizeWithoutRequestId)
            throw new InvalidOperationException("Body is too small to patch BodySize.");

        int bodySize = totalLength - MessageConstants.BodySizeFieldLength;
        if (bodySize > MessageConstants.MaxBodySize)
            throw new InvalidOperationException($"BodySize {bodySize} exceeds MaxBodySize {MessageConstants.MaxBodySize}.");

        // ArrayBufferWriter exposes WrittenMemory/Span as 읽기 전용이므로 ArraySegment를 뽑아 실제 버퍼에 쓴다.
        if (!MemoryMarshal.TryGetArray(_buffer.WrittenMemory, out ArraySegment<byte> segment) || segment.Array is null)
            throw new InvalidOperationException("Failed to access underlying buffer to patch BodySize.");
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(segment.Array.AsSpan(segment.Offset), bodySize);
        _bodySizePatched = true;
    }

    public ReadOnlyMemory<byte> WrittenMemory
    {
        get
        {
            if (!_bodySizePatched)
                throw new InvalidOperationException("Call PatchBodySize before accessing WrittenMemory.");
            return _buffer.WrittenMemory;
        }
    }

    private void EnsureNotPatched()
    {
        if (_bodySizePatched)
            throw new InvalidOperationException("Cannot write after BodySize is patched.");
    }

    public void Dispose()
    {
        // ArrayBufferWriter uses managed memory; nothing to dispose.
    }
}
