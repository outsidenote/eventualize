using System.Collections.Immutable;
using System.Data;

namespace EvDb.Core.Adapters;

internal class EvDbTelemetryContextNameDapperMapper : Dapper.SqlMapper.TypeHandler<EvDbTelemetryContextName>
{

    #region Ctor

    private EvDbTelemetryContextNameDapperMapper()
    {
    }

    #endregion //  Ctor

    internal static EvDbTelemetryContextNameDapperMapper Default { get; } = new EvDbTelemetryContextNameDapperMapper();

    #region Parse

    /// <summary>
    /// Parse the value from the database into an EvDbTelemetryContextName instance.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="DataException"></exception>
    public override EvDbTelemetryContextName Parse(object value)
    {
        switch (value)
        {
            case byte[] byteArray:
                return EvDbTelemetryContextName.From(byteArray);
            case ImmutableArray<byte> byteArray:
                return EvDbTelemetryContextName.From(byteArray);
            default:
                throw new DataException($"Cannot convert {value.GetType()} to EvDbTelemetryContextName");
        }
    }

    #endregion //  Parse

    #region SetValue

    /// <summary>
    /// Set the value of the parameter to the EvDbTelemetryContextName instance.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    public override void SetValue(IDbDataParameter parameter, EvDbTelemetryContextName value)
    {
        if (!value.IsInitialized() || value.Length == 0)
        {
            parameter.Value = DBNull.Value;
            parameter.DbType = DbType.Binary;
            parameter.Size = 0;
            return;
        }

        var bytes = value.Value;
        parameter.Value = bytes;
        parameter.DbType = DbType.Binary;
        parameter.Size = bytes.Length;
    }

    #endregion //  SetValue
}

