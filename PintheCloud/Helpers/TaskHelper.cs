using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Helpers
{
    public static class TaskHelper
    {
        // Tasks
        private static IDictionary<string, Task<bool>> Tasks = new Dictionary<string, Task<bool>>();
        private static Dictionary<string ,Task<bool>> SignInTasks = new Dictionary<string, Task<bool>>();
        //private static Dictionary<string, Task> SignOutTasks = new Dictionary<string, Task>();


        public static void AddTask(string name, Task<bool> task)
        {
            Task<bool> existedTask = null;
            if (!Tasks.TryGetValue(name, out existedTask))
                Tasks.Add(name, task);
        }


        public static async Task<bool> WaitTask(string name)
        {
            Task<bool> task = null;
            if (Tasks.TryGetValue(name, out task))
            {
                await task;
                Tasks.Remove(name);
                return task.Result;
            }
            throw new Exception();
        }

        public static void AddSignInTask(string key, Task<bool> task)
        {
            if (!SignInTasks.ContainsKey(key))
                SignInTasks.Add(key, task);
        }


        public static async Task<bool> WaitSignInTask(string key)
        {
            if (SignInTasks.ContainsKey(key))
            {
                bool resut = await SignInTasks[key];
                SignInTasks.Remove(key);
                return resut;
            }
            else
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task.Result;
            }
        }

        public static async Task<bool> WaitForAllSignIn()
        {
            bool result = true;
            using (var itr = SignInTasks.GetEnumerator())
            {
                while (itr.MoveNext())
                {
                    result &= await itr.Current.Value;
                }
            }
            return result;
        }

        //public static void AddSignOutTask(string key, Task task)
        //{
        //    if (!SignOutTasks.ContainsKey(key))
        //        SignOutTasks.Add(key, task);
        //}


        //public static Task WaitSignOutTask(string key)
        //{
        //    if (SignOutTasks.ContainsKey(key))
        //    {
        //        Task task = this.SignOutTasks[key];
        //        SignOutTasks.Remove(key);
        //        return task;
        //    }
        //    else
        //    {
        //        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        //        tcs.SetResult(true);
        //        return tcs.Task;
        //    }
        //}
    }
}
