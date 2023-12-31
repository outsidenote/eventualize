// Ignore Spelling: Env

namespace EvDb.Core;

using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using static System.StringComparison;

[JsonConverter(typeof(EnvJsonConverter))]
[Equatable]
[DebuggerDisplay("{_value}")]
public sealed partial class Env
{
    private readonly string _value;

    public Env(string value)
    {
        _value = Env.Format(value);
    }

    /// <summary>
    /// Formats the specified environment into convention.
    /// </summary>
    /// <returns></returns>
    private static string Format(string e)
    {
        bool comp(string candidate) => string.Compare(e, candidate, OrdinalIgnoreCase) == 0;

        if (comp("Production")) return "prod";
        if (comp("Prod")) return "prod";
        if (comp("Development")) return "dev";
        if (comp("Dev")) return "dev";

        return e;
    }

    #region Cast overloads

    /// <summary>
    /// Performs an implicit conversion from <see cref="string"/> to <see cref="Env"/>.
    /// </summary>
    /// <param name="env">The env.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator Env(string env) => new Env(env);

    /// <summary>
    /// Performs an implicit conversion from <see cref="Env"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="env">The env.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator string(Env env) => env._value;

    #endregion // Cast overloads 

    #region ToString

    /// <summary>
    /// Converts to string.
    /// </summary>
    public override string ToString() => _value;

    #endregion // ToString

    #region EnvJsonConverter

    /// <summary>
    /// Env Json Converter
    /// </summary>
    private sealed class EnvJsonConverter : JsonConverter<Env>
    {
        public override Env Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                new Env(reader.GetString()!);

        public override void Write(
            Utf8JsonWriter writer,
            Env env,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(env.ToString());
    }

    #endregion // EnvJsonConverter
}
