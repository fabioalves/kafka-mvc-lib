# Changelog

## [4.5.0] - 2021-11-22
- Add JsonSchema validation on the consumer

## [4.4.0] - 2021-09-29

### Added
- Consume using byte[] instead of string
- Support to JsonSchema deserialization
- Support to consume reprocessing through AsyncResult implementation return


## [4.0.0] - 2021-03-26

### Added
- Topic mapping using [Athena]
- Ignore null messages by configuration (IgnoreNullMessages - default:false)
- Async controllers
- Async Kafka message consumption configurable (AsyncConsumeEnabled - default:false)
- Logging CorrelationId in the message consumption scope
- Use the message CorrelationId as X-Request-Id in http requests
- Allow polymorphic deserialization of messages
- Read and write of headers in Kafka messages (MessageContext)

[4.4.0]: https://github.com/agilecontent/tvopenplatform-messaging/compare/master...90_Support_SchemaRegistry
[4.0.0]: https://github.com/agilecontent/tvopenplatform-messaging/compare/master...90_0_Get_Topics_From_Athena
[Athena]: https://github.com/agilecontent/tvopenplatform-settings
