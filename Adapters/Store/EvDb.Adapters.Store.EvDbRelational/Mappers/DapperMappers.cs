using Dapper;

namespace EvDb.Core.Adapters;

/// <summary>
/// Dapper Mappers for EvDbTelemetryContextName.
/// </summary>
internal static class DapperMappers
{
    private static bool _isRegistered;
    private static readonly object _lock = new object();

    #region RegisterDapperTypesMapper

    /// <summary>
    /// Register the special types for Dapper mapping.
    /// </summary>
    public static void RegisterDapperTypesMapper()
    {
        lock (_lock)
        {
            if (_isRegistered)
            {
                return;
            }
            _isRegistered = true;
            SqlMapper.AddTypeHandler(EvDbTelemetryContextNameDapperMapper.Default);
        }
    }

    #endregion //  RegisterDapperTypesMapper
}

