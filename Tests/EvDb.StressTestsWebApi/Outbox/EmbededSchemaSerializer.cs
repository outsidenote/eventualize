using Avro.File;
using Avro.Generic;
using Avro.Specific;
using System.Runtime.Intrinsics.Arm;
using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox
{
    public class EmbeddedSchemaSerializer : IEvDbOutboxSerializer
    {
        private const int maxResultCapacity = 1024 * 5;
        string IEvDbOutboxSerializer.Name => throw new NotImplementedException();

        byte[] IEvDbOutboxSerializer.Serialize<T>(EvDbChannelName channel, EvDbShardName shardName, T payload)
        {
            //   private async Task<byte[]> ToBytesAsync<TPayload>(TPayload payLoad) where TPayload : AvroBase

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

        bool IEvDbOutboxSerializer.ShouldSerialize<T>(EvDbChannelName channel, EvDbShardName shardName, T payload)
        {
            throw new NotImplementedException();
        }
    }
}
