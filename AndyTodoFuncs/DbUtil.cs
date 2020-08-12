using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Linq;
using System.Collections.Generic;

namespace AndyTodoFuncs
{
    public static class DbUtil
    {
        private static string GetDbName()
        {
            var db = Environment.GetEnvironmentVariable("Database");
            return db;
        }

        private static string GetDbId()
        {
            return GetContainerId();
        }

        private static string GetContainerId()
        {
            var container = Environment.GetEnvironmentVariable("Container");
            return container;
        }

        private async static Task ViewContainer(ContainerProperties containerProperties, ILogger log)
        {
            var db = GetDbId();
            var container = Shared.cosmosClient.GetContainer(db, containerProperties.Id);
            var throughput = await container.ReadThroughputAsync();

            log.LogInformation($"Throughput: {throughput}");
        }

        private async static Task<Container> CreateContainerIfNotExist(
            ILogger log,
            string containerId,
            string partitionKey = "/CreatedYearMonth",
            int throughput = 400)
        {
            var db = GetDbId();
            var start = DateTime.Now;
            var database = (await Shared.cosmosClient.CreateDatabaseIfNotExistsAsync(db)).Database;
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, partitionKey, throughput);
            var duration = DateTime.Now - start;

            return container;
        }

        public async static Task AddTodo(Todo todo, ILogger log)
        {
            var container = await CreateContainerIfNotExist(log, GetContainerId());

            await container.CreateItemAsync(todo, new PartitionKey(todo.CreatedYearMonth));
            log.LogInformation($"Created new document {todo.id}");
        }

        public static async Task<Todo[]> QueryAllTodos(ILogger log)
        {
            var container = await CreateContainerIfNotExist(log, GetContainerId());

            var sql = "SELECT * FROM c";

            // Query for dynamic objects
            var iterator = container.GetItemQueryIterator<dynamic>(sql);
            var documents = await iterator.ReadNextAsync();
            List<Todo> todos = new List<Todo>();
            foreach (var document in documents)
            {
                var todo = JsonConvert.DeserializeObject<Todo>(document.ToString());
                todos.Add(todo);
            }

            return todos.ToArray();
        }

        public static async Task<Todo> QueryTodo(string id, ILogger log)
        {
            var container = await CreateContainerIfNotExist(log, GetContainerId());

            var sql = $"SELECT TOP 1 * FROM c WHERE c.id = '{id}'";

            // Query for dynamic objects
            var iterator = container.GetItemQueryIterator<dynamic>(sql);
            var document = (await iterator.ReadNextAsync()).First();
            if (document == null)
            {
                return null;
            }

            var todo = JsonConvert.DeserializeObject<Todo>(document.ToString());

            return todo;
        }

        public static async Task<Todo> UpdateTodo(string id, TodoUpdateModel todoUpdateModel, ILogger log)
        {
            var container = await CreateContainerIfNotExist(log, GetContainerId());

            var sql = $"SELECT TOP 1 * FROM c WHERE c.id = '{id}'";

            // Query for dynamic objects
            var iterator = container.GetItemQueryIterator<dynamic>(sql);
            var document = (await iterator.ReadNextAsync()).First();
            if (document == null)
            {
                return null;
            }
            document.IsCompleted = todoUpdateModel.IsCompleted;
            document.TaskDescription = todoUpdateModel.TaskDescription;
            var result = await container.ReplaceItemAsync<dynamic>(document, (string)document.id);
            var updatedDocument = result.Resource;
            var todo = JsonConvert.DeserializeObject<Todo>(updatedDocument.ToString());

            return todo;
        }

        public static async Task<bool> DeleteTodo(string id, ILogger log)
        {
            var container = await CreateContainerIfNotExist(log, GetContainerId());

            log.LogInformation("Querying for new customer documents (SQL)");
            var sql = $"SELECT TOP 1 * FROM c WHERE c.id = '{id}'";

            // Query for dynamic objects
            var iterator = container.GetItemQueryIterator<dynamic>(sql);
            var document = (await iterator.ReadNextAsync()).First();
            if (document == null)
            {
                return false;
            }

            Todo todo = JsonConvert.DeserializeObject<Todo>(document.ToString());
            await container.DeleteItemAsync<dynamic>(todo.id, new PartitionKey(todo.CreatedYearMonth));

            return true;
        }
    }
}
