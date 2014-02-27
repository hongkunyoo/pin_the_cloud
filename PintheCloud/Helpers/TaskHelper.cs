using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Helpers
{
    public class TaskHelper
    {
        // Tasks
        public IDictionary<string, Task> Tasks = new Dictionary<string, Task>();
        public Task<bool>[] SignInTasks = new Task<bool>[App.IStorageManagers.Length];
        public Task[] SignOutTasks = new Task[App.IStorageManagers.Length];


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

        public void AddSignInTask(Task<bool> task, int platform)
        {
            if(this.SignInTasks[platform] == null)
                this.SignInTasks[platform] = task;
        }


        public async Task<bool> WaitSignInTask(int platform)
        {
            bool result = false;
            if (this.SignInTasks[platform] != null)
            {
                result = await this.SignInTasks[platform];
                this.SignInTasks[platform] = null;
            }
            return result;
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
