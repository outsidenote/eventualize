# Kafka Connect SQS Connector

kafka-connect-sqs is a [Kafka Connector](http://kafka.apache.org/documentation.html#connect)
for copying data between AWS SQS Queues and Kafka.

# Development

You can build kafka-connect-sqs with Maven using the standard lifecycle phases.

`mvn clean install` will run a clean build and run the unit tests. Integration tests are disabled by default.


# Running Integration Tests

1. Install [AWS CLI](https://aws.amazon.com/cli/) and configure it
1. Save the SQS credentials from Vault under `.aws/credentials`. 
    ```
    vault kv get v1/ci/kv/connect/sqs_it
    ```
    ```
    [default]
    aws_access_key_id = <key>
    aws_secret_access_key = <secret>
    ```

   OR set the environment variables

   ```
   AWS_ACCESS_KEY_ID = <key>
   AWS_SECRET_ACCESS_KEY = <secret>
   ```

1. If the credentials have been removed from Vault or are not working anymore follow these steps to setup new ones:
    1. Login to AWS through Okta. Ensure the AWS credentials have access to receive/delete messages from queues and create/delete queues (required for test setup and cleanup).
    1. Then, under IAM in the AWS console create a new user and API access keys. Save the keys in `.aws/credentials`.
`IAM -> Users -> Add User -> Enter name -> Add to/Create Group with SQS access -> Save Access Key ID and Secret Key`


Assuming you configured AWS CLI, the connector will pick up the AWS credentials from your CLI configuration, typically the file `~/.aws/credentials`


# Contribute

- Source Code: https://github.com/confluentinc/kafka-connect-sqs
- Issue Tracker: https://github.com/confluentinc/kafka-connect-sqs/issues
