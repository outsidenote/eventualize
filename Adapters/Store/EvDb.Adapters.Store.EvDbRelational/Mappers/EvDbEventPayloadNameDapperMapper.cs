using System.Collections.Immutable;
using System.Data;

namespace EvDb.Core.Adapters;

internal class EvDbEventPayloadNameDapperMapper : Dapper.SqlMapper.TypeHandler<EvDbEventPayloadName>
{

    #region Ctor

    private EvDbEventPayloadNameDapperMapper()
    {
    }

    #endregion //  Ctor

    internal static EvDbEventPayloadNameDapperMapper Default { get; } = new EvDbEventPayloadNameDapperMapper();

    #region Parse

    /// <summary>
    /// Parse the value from the database into an EvDbEventPayloadName instance.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="DataException"></exception>
    public override EvDbEventPayloadName Parse(object value)
    {
        switch (value)
        {
            case byte[] byteArray:
                return EvDbEventPayloadName.FromArray(byteArray);
            default:
                throw new DataException($"Cannot convert {value.GetType()} to EvDbEventPayloadName");
        }
    }

    #endregion //  Parse

    #region SetValue

    /// <summary>
    /// Set the value of the parameter to the EvDbEventPayloadName instance.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    public override void SetValue(IDbDataParameter parameter, EvDbEventPayloadName value)
    {
        if (!value.IsInitialized() || value.Length == 0)
        {
            parameter.Value = DBNull.Value;
            parameter.DbType = DbType.Binary;
            parameter.Size = 0;
            return;
        }

        IEvDbPayloadRawData raw = value;
        var bytes = raw.RawValue;
        parameter.Value = bytes;
        parameter.DbType = DbType.Binary;
        parameter.Size = bytes.Length;
    }

    #endregion //  SetValue
}

