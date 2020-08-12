using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AndyTodoFuncs
{
    public static class TodoApi
    {

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            ILogger log)
        {
            //log.LogInformation("Creating a new todo item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            var todo = new Todo() { TaskDescription = input.TaskDescription };
            if (input.CompleteBy != null)
            {
                todo.CompleteBy = input.CompleteBy;
            }
            todo.CreatedYearMonth = todo.CreatedTime.Year.ToString() + todo.CreatedTime.Month.ToString();

            await DbUtil.AddTodo(todo, log);

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            ILogger log)
        {
            //log.LogInformation("Getting todo list items");
            Todo[] todos = await DbUtil.QueryAllTodos(log);

            return new OkObjectResult(todos);
        }

        [FunctionName("GetTodoById")]
        public static async Task<IActionResult> GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            //log.LogInformation("Getting todo item by id");
            Todo todo = await DbUtil.QueryTodo(id, log);
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(todo);
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TodoUpdateModel updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);
            Todo updatedTodo = await DbUtil.UpdateTodo(id, updated, log);
            if (updatedTodo == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(updatedTodo);
        }

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            bool ret = await DbUtil.DeleteTodo(id, log);
            if (!ret)
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }
    }
}
