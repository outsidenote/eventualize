using Avro;
using Avro.Specific;
using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox;

[EvDbDefineMessagePayload("message-2")]
[EvDbAttachChannel("Channel1")]
[EvDbAttachChannel("Channel3")]
public partial record Message2 : ISpecificRecord
{
    public double Value { get; set; }

    Schema ISpecificRecord.Schema => Avro.Schema.Parse(
        """
        {
            'name': 'Message2',
        	'type': 'record',
        	'namespace': 'EvDb.StressTestsWebApi.Outbox',
        	'fields': 
        	[
               {
                    'name': 'Value',
        			'type': 'double'
                }
        	]
        }
        """);

    object ISpecificRecord.Get(int fieldPos)
    {
        return Value;
    }

    void ISpecificRecord.Put(int fieldPos, object fieldValue)
    {
        Value = (double)fieldValue;
    }
}