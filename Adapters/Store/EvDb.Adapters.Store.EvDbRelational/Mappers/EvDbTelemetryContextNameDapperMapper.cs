using System.Data;

namespace EvDb.Core.Adapters;

internal class EvDbTelemetryContextNameDapperMapper : Dapper.SqlMapper.TypeHandler<EvDbOtelTraceParent>
{

    #region Ctor

    private EvDbTelemetryContextNameDapperMapper()
    {
    }

    #endregion //  Ctor

    internal static EvDbTelemetryContextNameDapperMapper Default { get; } = new EvDbTelemetryContextNameDapperMapper();

    #region Parse

    /// <summary>
    /// Parse the value from the database into an EvDbOtelTraceParent instance.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="DataException"></exception>
    public override EvDbOtelTraceParent Parse(object value)
    {
        switch (value)
        {
            case string v:
                return EvDbOtelTraceParent.From(v);
            default:
                throw new DataException($"Cannot convert {value.GetType()} to EvDbOtelTraceParent");
        }
    }

    #endregion //  Parse

    #region SetValue

    /// <summary>
    /// Set the value of the parameter to the EvDbOtelTraceParent instance.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    public override void SetValue(IDbDataParameter parameter, EvDbOtelTraceParent value)
    {
        if (!value.IsInitialized() || value.IsEmpty)
        {
            parameter.Value = DBNull.Value;
            parameter.DbType = DbType.Binary;
            parameter.Size = 0;
            return;
        }

        parameter.Value = (string?)value;
        parameter.DbType = DbType.String;
        parameter.Size = 55;
    }

    #endregion //  SetValue
}

