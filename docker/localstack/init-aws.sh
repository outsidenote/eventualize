#!/bin/bash
set -e

echo "Setting up LocalStack AWS services..."

# Create SNS topic
echo "Creating SNS topic: local-topic"
awslocal sns create-topic --name local-topic

# Create SQS queue
echo "Creating SQS queue: local-queue"
awslocal sqs create-queue --queue-name local-queue

# Get the ARN of the SQS queue
QUEUE_ARN=$(awslocal sqs get-queue-attributes --queue-url http://localhost:4566/000000000000/local-queue --attribute-names QueueArn --query 'Attributes.QueueArn' --output text)

# Set more permissive Queue attributes for Debezium
echo "Setting up SQS Queue permissions"
awslocal sqs set-queue-attributes \
  --queue-url http://localhost:4566/000000000000/local-queue \
  --attributes '{"VisibilityTimeout":"60"}'

# Log the ARN for debugging
echo "Subscribing SQS to SNS using ARN: $QUEUE_ARN"

# Subscribe the SQS queue to the SNS topic
awslocal sns subscribe \
  --topic-arn arn:aws:sns:us-east-1:000000000000:local-topic \
  --protocol sqs \
  --notification-endpoint $QUEUE_ARN

# Allow SNS to send messages to the SQS queue by setting the appropriate policy
echo "Setting up SQS access policy"
awslocal sqs set-queue-attributes \
  --queue-url http://localhost:4566/000000000000/local-queue \
  --attributes '{
    "Policy": "{
      \"Version\": \"2012-10-17\",
      \"Statement\": [
        {
          \"Effect\": \"Allow\",
          \"Principal\": \"*\",
          \"Action\": \"sqs:SendMessage\",
          \"Resource\": \"'"$QUEUE_ARN"'\",
          \"Condition\": {
            \"ArnEquals\": {
              \"aws:SourceArn\": \"arn:aws:sns:us-east-1:000000000000:local-topic\"
            }
          }
        }
      ]
    }"
  }'

# Send a test notification to verify everything is set up
echo "Sending test message to SNS topic"
awslocal sns publish \
  --topic-arn arn:aws:sns:us-east-1:000000000000:local-topic \
  --message "This is a test message from LocalStack init script!"

# Check if the message arrived in SQS
echo "Checking SQS queue for test message"
awslocal sqs receive-message \
  --queue-url http://localhost:4566/000000000000/local-queue \
  --wait-time-seconds 5 \
  --max-number-of-messages 1

echo "✅ LocalStack SNS and SQS setup completed."