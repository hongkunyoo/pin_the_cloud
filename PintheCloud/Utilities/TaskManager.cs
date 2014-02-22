using PintheCloud.Models;
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
        public Task[] SignInTasks = new Task[App.IStorageManagers.Length];
        public Task[] SignOutTasks = new Task[App.IStorageManagers.Length];


        // Task keys
        public const string INITIAL_PIN_SPOT_AND_UPLOAD_FILE_TASK = "INITIAL_PIN_SPOT_AND_UPLOAD_FILE_TASK";


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


        public void AddSignOutTask(Task task, int platform)
        {
            if (this.SignOutTasks[platform] == null)
                this.SignOutTasks[platform] = task;
        }


        public async Task WaitSignOutTask(int platform)
        {
            if (this.SignOutTasks[platform] != null)
            {
                await this.SignOutTasks[platform];
                this.SignOutTasks[platform] = null;
            }
        }
    }
}
