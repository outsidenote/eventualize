// Ignore Spelling: Channel

using Avro;
using Avro.Specific;
using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox;

[EvDbDefineMessagePayload("message-1")]
[EvDbAttachChannel("Channel1")]
[EvDbAttachChannel("Channel2")]
public partial record Message1() : ISpecificRecord
{
    public double Amount { get; set; }

    Schema ISpecificRecord.Schema => Avro.Schema.Parse(
        """
        {
            'name': 'Message1',
        	'type': 'record',
        	'namespace': 'EvDb.StressTestsWebApi.Outbox',
        	'fields': 
        	[
               {
                    'name': 'Amount',
        			'type': 'double'
                }
        	]
        }
        """);

    object ISpecificRecord.Get(int fieldPos)
    {
        return Amount;
    }

    void ISpecificRecord.Put(int fieldPos, object fieldValue)
    {
        Amount = (double)fieldValue;
    }
}