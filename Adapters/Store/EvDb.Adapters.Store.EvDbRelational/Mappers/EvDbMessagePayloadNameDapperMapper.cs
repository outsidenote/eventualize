using System.Data;

namespace EvDb.Core.Adapters;

internal class EvDbMessagePayloadNameDapperMapper : Dapper.SqlMapper.TypeHandler<EvDbMessagePayloadName>
{

    #region Ctor

    private EvDbMessagePayloadNameDapperMapper()
    {
    }

    #endregion //  Ctor

    internal static EvDbMessagePayloadNameDapperMapper Default { get; } = new EvDbMessagePayloadNameDapperMapper();

    #region Parse

    /// <summary>
    /// Parse the value from the database into an EvDbMessagePayloadName instance.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="DataException"></exception>
    public override EvDbMessagePayloadName Parse(object value)
    {
        switch (value)
        {
            case byte[] byteArray:
                return EvDbMessagePayloadName.FromArray(byteArray);
            default:
                throw new DataException($"Cannot convert {value.GetType()} to EvDbMessagePayloadName");
        }
    }

    #endregion //  Parse

    #region SetValue

    /// <summary>
    /// Set the value of the parameter to the EvDbMessagePayloadName instance.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    public override void SetValue(IDbDataParameter parameter, EvDbMessagePayloadName value)
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

