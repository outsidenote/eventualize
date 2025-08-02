# EvDb

<picture>
    <source srcset="images/dark-theme-logo.png"  media="(prefers-color-scheme: dark)">
    <source srcset="images/light-theme-logo.png"  media="(prefers-color-scheme: light)">
    <img src="light-theme-logo.png" width="250">
</picture>

[![Deploy](https://github.com/outsidenote/eventualize/actions/workflows/dotnet-deploy.yml/badge.svg)](https://github.com/outsidenote/eventualize/actions/workflows/dotnet-deploy.yml) [![Verify](https://github.com/outsidenote/eventualize/actions/workflows/dotnet-verify.yml/badge.svg)](https://github.com/outsidenote/eventualize/actions/workflows/dotnet-verify.yml)

EvDb is an opinionated event-sourcing framework that unlocks the untapped potential of transactional data while eliminating many challenges related to management of transactional data and its schema.
EvDb is quick & easy to integrate, and while it is working with new paradigms and patterns under the hood, it abstracts most of it away and does not distrupt development.

## Watch a Short Intro Video on The Website

[<img src="images/watch-video-cta.png" width="600">](https://eventualizedb.com/)

## Quick Start

If you want to jump right into it, go to [Quick Start](https://eventualizedb.com/quick-start)

## Learn More

If you want to learn more, go to [Learn More](https://eventualizedb.comlearn-more).

## Contribute

You can contribute to this project in many ways (not just coding)!
If you are interested to learn more about how you can do this, please visit the [Contribution](https://eventualizedb.comcontribution) page.

## Sharding setup

- [Clode](https://claude.ai/share/076cd430-53ea-4149-9ffb-549331451dc4)

## License

MIT License

## Flexable References

Regex Replace
� Find what: Version="([0-8])\.([^"]+)"
� Replace with: Version="[$1.$2,)"

## Cloud Events

- [Cloud Events](https://cloudevents.io/)
- [Cloud Events Attributes](https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#required-attributes)

### Mapping

```json
{
  "specversion": "1.0",
  "source": "comp-name.com/domain/crew/app",
  "id": " 49b4e376-8601-5df9-aa94-b953e4e5b0fa",
  "type": "com.comp-name.payment_succeded.v1",
  "time": " 2025-07-13T14:30:45+02:00",
  "datacontenttype": "application/json",
  "dataref": "http://s3.comp-name.com/here/it/is.json",
  "dataschema": "https://schemamanager/{type}",
  "traceparent": "00-{trace-id}-{span-id}-01",
  "sequence": 1,
  "partitionkey": "data.payee_id" // a dot-notation to a specific field in the dataschema. Set to "id" for no partioning/grouping.
}
```

| Cloud Event     | EvDbCloudEventEnvalope | EvDB Message Record   | Sample Value                                        |
| --------------- | ---------------------- | --------------------- | --------------------------------------------------- |
| ⁕ specversion   |                        |                       | 1.0                                                 |
| ⁕ source        | Source                 |                       | comp-name.com/domain/crew/app                       |
| ⁕ id            |                        | Id                    |                                                     |
| ⁕ type          |                        | MessageType           | payment_succeded.v1                                 |
| time            |                        | CapturedAt            |                                                     |
| datacontenttype |                        | SerializeType         | application/json                                    |
| dataschema      | DataSchemaUri          | MessageType?          | https://schemamanager.comp-name/payment_succeded.v1 |
| partitionkey    |                        | StreamType / StreamId |                                                     |
| evdbchannel     |                        | Channel               |                                                     |
| traceparent     |                        | TraceParent           |                                                     |
| evdbeventtype   |                        | EventType             |                                                     |
| evdboffset      |                        | Offset                |                                                     |
| data            |                        | payload               |                                                     |
| dataref         |                        |                       |                                                     |

### Binding

- [Http Cloud Events](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/http-protocol-binding.md)
- [Kafka Cloud Events](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/kafka-protocol-binding.md)
