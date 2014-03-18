using PintheCloud.Helpers;
using PintheCloud.Managers;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public static class StorageExplorer
    {
        private static Dictionary<string,FileObject> DictionaryRoot = new Dictionary<string,FileObject>();
        private static Dictionary<string, Stack<FileObject>> DictionaryTree = new Dictionary<string, Stack<FileObject>>();
        //private static PintheCloud.Utilities.ConvertTemplate.ConvertFirstToSecond<FileObject, T> ReverseFileObject;
        //private static PintheCloud.Utilities.ConvertTemplate.ConvertFirstToSecond<T, FileObject> ConvertToFileObject;
        //private static string SQL_DATABASE_SET = "SQL_DATABASE_SET";

        public async static Task<bool> Synchronize()
        {
            if (App.ApplicationSettings.Contains("0"))
            {
                ////////////////////////////////////////////
                // TODO : Retrieve Data from DATABASE;
                ////////////////////////////////////////////

                
                return true;
            }
            else
            {
                try
                {
                    //await TaskHelper.WaitSignInTask(Switcher.GetCurrentStorage().GetStorageName());
                    //bool result = await TaskHelper.WaitTask(App.AccountManager.GetPtcId());
                    //bool result = await TaskHelper.WaitForAllSignIn();
                    //if (!result) return false;
                    System.Diagnostics.Debug.WriteLine("Sychronizing!");
                    using (var itr = StorageHelper.GetStorageEnumerator())
                    {
                        while (itr.MoveNext())
                        {
                            if (itr.Current.IsSignIn())
                            {
                                if (await TaskHelper.WaitSignInTask(itr.Current.GetStorageName()))
                                {
                                    FileObject rootFolder = await itr.Current.Synchronize();
                                    DictionaryRoot.Add(itr.Current.GetStorageName(), rootFolder);
                                    Stack<FileObject> stack = new Stack<FileObject>();
                                    stack.Push(rootFolder);
                                    DictionaryTree.Add(itr.Current.GetStorageName(),stack);
                                }
                            }
                        }
                    }

                    ////////////////////////////////////////////
                    // TODO : SAVE Data to DATABASE;
                    ////////////////////////////////////////////


                    //App.ApplicationSettings[SQL_DATABASE_SET] = true;
                    System.Diagnostics.Debug.WriteLine("Sychronizing Finished!!");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            
        }
        public async static Task Refresh()
        {
            //App.ApplicationSettings.Remove(SQL_DATABASE_SET);
            await Synchronize();
        }
        public static FileObject GetRootFile()
        {
            return GetCurrentRoot();
        }
        public static List<FileObject> GetFilesFromRootFolder()
        {
            if (GetRootFile().FileList == null) System.Diagnostics.Debugger.Break();
            return GetRootFile().FileList;
        }

        public static List<FileObject> GetTreeForFolder(FileObject folder)
        {
            List<FileObject> list = folder.FileList;


            if (!GetCurrentTree().Contains(folder))
                GetCurrentTree().Push(folder);
            if (list == null) System.Diagnostics.Debugger.Break();
            return list;
        }
        public static List<FileObject> TreeUp()
        {
            if (GetCurrentTree().Count > 1)
            {
                GetCurrentTree().Pop();
                return GetTreeForFolder(GetCurrentTree().First());
            }
            return null;
        }

        public static string GetCurrentPath()
        {
            return null;
        }

        private static FileObject GetCurrentRoot()
        {
            return DictionaryRoot[Switcher.GetCurrentStorage().GetStorageName()];
        }

        private static Stack<FileObject> GetCurrentTree()
        {
            return DictionaryTree[Switcher.GetCurrentStorage().GetStorageName()];
        }
    }
}
