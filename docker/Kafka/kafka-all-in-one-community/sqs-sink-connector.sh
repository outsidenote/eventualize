#!/bin/bash

echo "Waiting for Kafka Connect to be ready..."
sleep 10

curl -X POST http://localhost:8083/connectors -H "Content-Type: application/json" -d '{
  "name": "sqs-sink-connector",
  "config": {
    "connector.class": "com.datamountaineer.streamreactor.connect.aws.sqs.sink.SqsSinkConnector",
    "tasks.max": "1",
    "topics": "EvDb-Demo-Tests-Topic",
    "aws.sqs.url": "https://sqs.us-east-1.amazonaws.com/123456789012/EvDb-Demo-Tests-Queue",
    "aws.access.key": "'"${AWS_ACCESS_KEY_ID}"'",
    "aws.secret.key": "'"${AWS_SECRET_ACCESS_KEY}"'",
    "aws.region": "us-east-1",
    "value.converter": "org.apache.kafka.connect.storage.StringConverter",
    "key.converter": "org.apache.kafka.connect.storage.StringConverter"
  }
}'
