using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
#pragma warning disable S101 // Types should be named in PascalCase

namespace EvDb.DemoWebApi;

public static class AWSProviderFactory
{
    private readonly static BasicAWSCredentials CREDENTIALS = new BasicAWSCredentials("test", "test");
    private const string AWS_ENDPOINT = "http://localhost:4566";

    #region CreateSNSClient

    public static AmazonSimpleNotificationServiceClient CreateSNSClient()
    {
        var config = new AmazonSimpleNotificationServiceConfig
        {
            ServiceURL = AWS_ENDPOINT,
            //AuthenticationRegion = REGION
            UseHttp = true // UseHttp is required for LocalStack 
        };
        return new AmazonSimpleNotificationServiceClient(CREDENTIALS, config);
    }

    #endregion //  CreateSNSClient

    #region CreateSqsCreateSQSClientClient

    public static AmazonSQSClient CreateSQSClient()
    {
        var config = new AmazonSQSConfig
        {
            ServiceURL = AWS_ENDPOINT,
            // AuthenticationRegion = REGION
            UseHttp = true // UseHttp is required for LocalStack 
        };
        return new AmazonSQSClient(CREDENTIALS, config);
    }

    #endregion //  CreateSQSClient
}
