using System.Data;
using System.Diagnostics;

namespace EvDb.Core.Adapters;

[DebuggerDisplay("MessageType: {MessageType} ,EventType:{EventType}, Channel:{Channel} Offset:{Offset}, StreamId:{StreamId}")]
public struct EvDbMessageRecord : IDataRecord
{
    public string Domain { get; init; }
    public string Partition { get; init; }
    public string StreamId { get; init; }
    public long Offset { get; init; }
    public string EventType { get; init; }
    public string Channel { get; init; }
    public string? TraceId { get; init; }
    public string? SpanId { get; init; }
    public string MessageType { get; init; }
    public string SerializeType { get; init; }
    public byte[] Payload { get; init; }
    public string CapturedBy { get; init; }
    public DateTimeOffset CapturedAt { get; init; }

    #region static implicit operator EvDbMessageRecord(EvDbMessage e) ...

    public static implicit operator EvDbMessageRecord(EvDbMessage e)
    {
        Activity? activity = Activity.Current;
        var result = new EvDbMessageRecord
        {
            Domain = e.StreamCursor.Domain,
            Partition = e.StreamCursor.Partition,
            StreamId = e.StreamCursor.StreamId,
            Offset = e.StreamCursor.Offset,
            EventType = e.EventType,
            Channel = e.Channel,
            MessageType = e.MessageType,
            SerializeType = e.SerializeType,
            Payload = e.Payload,
            CapturedBy = e.CapturedBy,
            CapturedAt = e.CapturedAt,
            SpanId = activity?.SpanId.ToHexString(),
            TraceId = activity?.TraceId.ToHexString()
        };
        return result;
    }

    #endregion //  static implicit operator EvDbMessageRecord(EvDbMessage e) ...

    #region IDataRecord Implementation
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.

    int IDataRecord.FieldCount => 13;

    #region object? GetValue(int i)

    private object? GetValue(int i) => i switch
    {
        0 => Domain,
        1 => Partition,
        2 => StreamId,
        3 => Offset,
        4 => EventType,
        5 => Channel,
        6 => TraceId,
        7 => SpanId,
        8 => MessageType,
        9 => SerializeType,
        10 => Payload,
        11 => CapturedBy,
        12 => CapturedAt,
        _ => throw new IndexOutOfRangeException()
    };

    #endregion //  object? GetValue(int i)

    #region GetDataType

    Type GetDataType(int i) => i switch
    {
        0 => typeof(string), // Domain
        1 => typeof(string), // Partition
        2 => typeof(string), // StreamId
        3 => typeof(long), // Offset
        4 => typeof(string), // EventType
        5 => typeof(string), // Channel
        6 => typeof(string), // TraceId
        7 => typeof(string), //SpanId
        8 => typeof(string), //MessageType
        9 => typeof(string), // SerializeType
        10 => typeof(byte[]), //Payload
        11 => typeof(string), //CapturedBy,
        12 => typeof(DateTimeOffset), // CapturedAt,
        _ => throw new IndexOutOfRangeException()
    };

    #endregion //  GetDataType

    object IDataRecord.this[int i] => GetValue(i);

    #region object IDataRecord.this[string name]

    object IDataRecord.this[string name] => name switch
    {
        nameof(Domain) => Domain,
        nameof(Partition) => Partition,
        nameof(StreamId) => StreamId,
        nameof(Offset) => Offset,
        nameof(EventType) => EventType,
        nameof(Channel) => Channel,
        nameof(TraceId) => TraceId,
        nameof(SpanId) => SpanId,
        nameof(MessageType) => MessageType,
        nameof(SerializeType) => SerializeType,
        nameof(Payload) => Payload,
        nameof(CapturedBy) => CapturedBy,
        nameof(CapturedAt) => CapturedAt,
        _ => throw new IndexOutOfRangeException()
    };

    #endregion //  object IDataRecord.this[string name]

    bool IDataRecord.GetBoolean(int i) => throw new NotSupportedException();
    byte IDataRecord.GetByte(int i) =>  throw new NotSupportedException();
    long IDataRecord.GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();
    char IDataRecord.GetChar(int i) =>   throw new NotSupportedException();
    long IDataRecord.GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();
    IDataReader IDataRecord.GetData(int i) => throw new NotImplementedException();
    string IDataRecord.GetDataTypeName(int i) => GetDataType(i).Name;
    DateTime IDataRecord.GetDateTime(int i) => throw new NotImplementedException();
    decimal IDataRecord.GetDecimal(int i) => throw new NotImplementedException();
    double IDataRecord.GetDouble(int i) => throw new NotImplementedException();
    Type IDataRecord.GetFieldType(int i) => GetDataType(i);
    float IDataRecord.GetFloat(int i) => throw new NotImplementedException();
    Guid IDataRecord.GetGuid(int i) => throw new NotImplementedException();
    short IDataRecord.GetInt16(int i) => throw new NotImplementedException();
    int IDataRecord.GetInt32(int i) => throw new NotImplementedException();
    long IDataRecord.GetInt64(int i) => i == 3 ? Offset : throw new NotImplementedException();

    #region string IDataRecord.GetName(int i)

    string IDataRecord.GetName(int i) => i switch
    {
        0 => nameof(Domain),
        1 => nameof(Partition),
        2 => nameof(StreamId),
        3 => nameof(Offset),
        4 => nameof(EventType),
        5 => nameof(Channel),
        6 => nameof(TraceId),
        7 => nameof(SpanId),
        8 => nameof(MessageType),
        9 => nameof(SerializeType),
        10 => nameof(Payload),
        11 => nameof(CapturedBy),
        12 => nameof(CapturedAt),
        _ => throw new IndexOutOfRangeException()
    };

    #endregion //  string IDataRecord.GetName(int i)

    #region GetOrdinal

    int IDataRecord.GetOrdinal(string name) => name switch
    {
        nameof(Domain) => 0,
        nameof(Partition) => 1,
        nameof(StreamId) => 2,
        nameof(Offset) => 3,
        nameof(EventType) => 4,
        nameof(Channel) => 5,
        nameof(TraceId) => 6,
        nameof(SpanId) => 7,
        nameof(MessageType) => 8,
        nameof(SerializeType) => 9,
        nameof(Payload) => 10,
        nameof(CapturedBy) => 11,
        nameof(CapturedAt) => 12,
        _ => throw new IndexOutOfRangeException()
    };

    #endregion //  GetOrdinal

    string IDataRecord.GetString(int i) => (string)GetValue(i);
    object IDataRecord.GetValue(int i) => GetValue(i);
    int IDataRecord.GetValues(object[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = GetValue(i);
        }
        return values.Length;
    }

    bool IDataRecord.IsDBNull(int i) => i switch
    {
        6 => TraceId == null,
        7 => SpanId == null,
        _ => false
    };

#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.
    #endregion // IDataRecord Implementation
}
