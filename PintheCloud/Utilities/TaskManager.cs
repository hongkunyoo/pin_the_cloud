using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public class TaskManager
    {
        // Tasks
        public IDictionary<string, Task> Tasks = new Dictionary<string, Task>();
        public Task[] SignInTasks = new Task[App.PLATFORMS.Length];

        public const string SIGN_OUT_TASK_KEY = "SIGN_OUT_TASK_KEY";


        public void AddTask(string name, Task task)
        {
            Task existedTask = null;
            if (!this.Tasks.TryGetValue(name, out existedTask))
                this.Tasks.Add(name, task);
        }


        public async Task WaitTask(string name)
        {
            Task task = null;
            if (this.Tasks.TryGetValue(name, out task))
            {
                await task;
                this.Tasks.Remove(name);
            }
        }

        public void AddSignInTask(Task task, int platform)
        {
            if(this.SignInTasks[platform] == null)
                this.SignInTasks[platform] = task;
        }


        public async Task WaitSignInTask(int platform)
        {
            if (this.SignInTasks[platform] != null)
            {
                await this.SignInTasks[platform];
                this.SignInTasks[platform] = null;
            }
        }
    }
}
