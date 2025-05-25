using System.Data;

namespace EvDb.Core.Adapters;

internal class EvDbSnapshotStateNameDapperMapper : Dapper.SqlMapper.TypeHandler<EvDbSnapshotStateName>
{

    #region Ctor

    private EvDbSnapshotStateNameDapperMapper()
    {
    }

    #endregion //  Ctor

    internal static EvDbSnapshotStateNameDapperMapper Default { get; } = new EvDbSnapshotStateNameDapperMapper();

    #region Parse

    /// <summary>
    /// Parse the value from the database into an EvDbSnapshotStateName instance.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="DataException"></exception>
    public override EvDbSnapshotStateName Parse(object value)
    {
        switch (value)
        {
            case byte[] byteArray:
                return EvDbSnapshotStateName.FromArray(byteArray);
            default:
                throw new DataException($"Cannot convert {value.GetType()} to EvDbSnapshotStateName");
        }
    }

    #endregion //  Parse

    #region SetValue

    /// <summary>
    /// Set the value of the parameter to the EvDbSnapshotStateName instance.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    public override void SetValue(IDbDataParameter parameter, EvDbSnapshotStateName value)
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

