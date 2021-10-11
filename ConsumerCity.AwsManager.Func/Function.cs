using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ConsumerCity.AwsManager.Func
{
    public class Functions
    {

        private DynamoDBContext DbContext { get; set; }
        private AmazonLambdaClient LambdaClient { get; set; }

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            DotNetEnv.Env.Load();

            var awsCredentials = new BasicAWSCredentials(Environment.GetEnvironmentVariable("DynamoUsername"), Environment.GetEnvironmentVariable("DynamoPassword"));

            DbContext = new DynamoDBContext(
                new AmazonDynamoDBClient(
                    awsCredentials, 
                    new AmazonDynamoDBConfig() {
                        RegionEndpoint = RegionEndpoint.SAEast1
                    }
                )
            );

            AmazonLambdaConfig lambdaConfig = new AmazonLambdaConfig() { RegionEndpoint = RegionEndpoint.SAEast1 };

            LambdaClient = new AmazonLambdaClient(awsCredentials, lambdaConfig);
        }


        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse MyFirstFunc(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Request\n");


            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = $"Env - [{Environment.GetEnvironmentVariable("DynamoUsername")}]",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            return response;
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task<APIGatewayProxyResponse> MySecondFunc(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try {
                

                //var result = await context.LoadAsync<Person>("1");

                //await context.SaveAsync(new Person() { Id = "1", Name = "Pedro"});

                var result = await DbContext.ScanAsync<Person>(new List<ScanCondition>()).GetRemainingAsync();

                return new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = result.First().Name,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }
            catch (Exception e) {

                return new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "error",
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }
        }

        public class Person
        {
            [DynamoDBHashKey("id")]
            public string Id { get; set; }

            [DynamoDBProperty("name")]
            public string Name { get; set; }

            [DynamoDBProperty("Car")]
            public Car Car { get; set; }
        }

        public class Car
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public async Task<APIGatewayProxyResponse> AwsStatus(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Request\n");
            var res = await LambdaClient.ListFunctionsAsync();

            var funcs = res.Functions;


            var response = new APIGatewayProxyResponse {
                StatusCode = (int)HttpStatusCode.OK,
                Body = $"AWS status",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            return response;
        }
    }
}
