using System;
using System.Net;
using Microsoft.Azure.Cosmos;
//using Microsoft.Extensions.Configuration;

namespace AndyTodoFuncs
{
	public static class Shared
	{
		public static CosmosClient cosmosClient { get; private set; }

		static Shared()
		{
			var endpoint = Environment.GetEnvironmentVariable("CosmosEndpoint");
			var masterKey = Environment.GetEnvironmentVariable("CosmosMasterKey");

			cosmosClient = new CosmosClient(endpoint, masterKey);
			// WebProxy proxy = new WebProxy("http://one.proxy.att.com:8080", true);
			// cosmosClient = new CosmosClient(endpoint, masterKey, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway, WebProxy = proxy });
		}
	}
}
