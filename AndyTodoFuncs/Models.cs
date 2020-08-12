using System;
//using Newtonsoft.Json;

namespace AndyTodoFuncs
{
    public class Todo
    {
        public string id { get; set; } = Guid.NewGuid().ToString();

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public string CreatedYearMonth { get; set; }

        public string TaskDescription { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompleteBy { get; set; }
    }

    public class TodoCreateModel
    {
        public string TaskDescription { get; set; }

        public DateTime? CompleteBy { get; set; }
    }

    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}
