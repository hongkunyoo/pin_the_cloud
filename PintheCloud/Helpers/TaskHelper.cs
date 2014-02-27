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
        //public Task<bool>[] SignInTasks = new Task<bool>[App.IStorageManagers.Length];
        public Dictionary<string ,Task<bool>> SignInTasks = new Dictionary<string, Task<bool>>();
        //public Task[] SignOutTasks = new Task[App.IStorageManagers.Length];
        public Dictionary<string, Task> SignOutTasks = new Dictionary<string, Task>();


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

        public void AddSignInTask(string key, Task<bool> task)
        {
            if(!this.SignInTasks.ContainsKey(key))
                this.SignInTasks[key] = task;
        }


        public Task<bool> WaitSignInTask(string key)
        {
            if (this.SignInTasks.ContainsKey(key))
            {
                Task<bool> t = this.SignInTasks[key];
                this.SignInTasks.Remove(key);
                return t;
            }
            else
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            }
            
        }


        public void AddSignOutTask(string key, Task task)
        {
            if (!this.SignOutTasks.ContainsKey(key))
                this.SignOutTasks[key] = task;
        }


        public Task WaitSignOutTask(string key)
        {
            if (this.SignOutTasks.ContainsKey(key))
            {
                Task t = this.SignOutTasks[key];
                this.SignOutTasks.Remove(key);
                return t;
            }
            else
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            }
        }
    }
}
