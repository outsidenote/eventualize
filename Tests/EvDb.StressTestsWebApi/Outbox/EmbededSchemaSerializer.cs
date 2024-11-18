using Avro.File;
using Avro.Generic;
using Avro.Specific;
using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox
{
    public class EmbeddedSchemaSerializer : IEvDbOutboxSerializer
    {
        private const int maxResultCapacity = 1024 * 5;
        string IEvDbOutboxSerializer.Name => nameof(EmbeddedSchemaSerializer);

        byte[] IEvDbOutboxSerializer.Serialize<T>(EvDbChannelName channel, EvDbShardName shardName, T payload)
        {

            if (payload is not ISpecificRecord record)
            { 
                throw new NotSupportedException();
            }

            DatumWriter<T> writer = new SpecificDatumWriter<T>(record.Schema);
            using var output = new MemoryStream(maxResultCapacity);

            var specDataWriter = DataFileWriter<T>.OpenWriter(writer, output);
            specDataWriter.Append(payload);
            specDataWriter.Flush();
            specDataWriter.Close();

            var result = output.ToArray();

            return result;
        }

        bool IEvDbOutboxSerializer.ShouldSerialize<T>(EvDbChannelName channel, EvDbShardName shardName, T payload) => 
            shardName == OutboxShards.Table1;
    }
}
