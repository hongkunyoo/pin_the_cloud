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
        private static Dictionary<string, FileObject> DictionaryRoot = new Dictionary<string, FileObject>();
        private static Dictionary<string, Stack<FileObject>> DictionaryTree = new Dictionary<string, Stack<FileObject>>();

        private static string SYNC_KEYS = "SYNC_KEYS";
        private static string ROOT_ID = "ROOT_ID";

        /*
        public async static bool SynchronizeAll()
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
        */


        public async static Task<bool> Synchronize(string key)
        {
            IStorageManager Storage = StorageHelper.GetStorageManager(key);
            if (!await TaskHelper.WaitSignInTask(Storage.GetStorageName()))
                return false;

            // Fetching from SQL
            if (App.ApplicationSettings.Contains(SYNC_KEYS + key))
            {
                System.Diagnostics.Debug.WriteLine("Fetching From SQL");
                try
                {
                    using (FileObjectDataContext db = new FileObjectDataContext("isostore:/" + key + "_db.sdf"))
                    {
                        if (!db.DatabaseExists())
                        {
                            App.ApplicationSettings.Remove(SYNC_KEYS + key);
                            return await Synchronize(key);
                        }
                        var rootDB = from FileObjectSQL fos in db.FileItems where fos.ParentId.Equals(ROOT_ID) select fos;

                        List<FileObjectSQL> getsqlList = rootDB.ToList<FileObjectSQL>();

                        if (getsqlList.Count != 1) System.Diagnostics.Debugger.Break();
                        FileObjectSQL rootFos = getsqlList.First<FileObjectSQL>();
                        FileObject rootFolder = FileObject.ConvertToFileObject(db, rootFos);

                        if (DictionaryRoot.ContainsKey(key))
                            DictionaryRoot.Remove(key);
                        DictionaryRoot.Add(key, rootFolder);

                        Stack<FileObject> stack = new Stack<FileObject>();
                        stack.Push(rootFolder);
                        if (DictionaryTree.ContainsKey(key))
                            DictionaryTree.Remove(key);
                        DictionaryTree.Add(key, stack);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                    return false;
                }
            }

            // Fetching from Server
            else
            {
                System.Diagnostics.Debug.WriteLine("Fetching From Server");
                if (Storage.IsSignIn())
                {
                    FileObject rootFolder = await Storage.Synchronize();
                    if (DictionaryRoot.ContainsKey(key))
                        DictionaryRoot.Remove(key);
                    DictionaryRoot.Add(key, rootFolder);

                    Stack<FileObject> stack = new Stack<FileObject>();
                    stack.Push(rootFolder);
                    if (DictionaryTree.ContainsKey(key))
                        DictionaryTree.Remove(key);
                    DictionaryTree.Add(key, stack);


                    ////////////////////////////////////////////
                    // Saving to SQL job
                    ////////////////////////////////////////////
                    try
                    {
                        using (FileObjectDataContext db = new FileObjectDataContext("isostore:/" + key + "_db.sdf"))
                        {
                            if (db.DatabaseExists())
                                db.DeleteDatabase();
                            db.CreateDatabase();

                            List<FileObjectSQL> sqlList = new List<FileObjectSQL>();
                            FileObject.ConvertToFileObjectSQL(sqlList, rootFolder, ROOT_ID);
                            for (var i = 0; i < sqlList.Count; i++)
                                db.FileItems.InsertOnSubmit(sqlList[i]);
                            db.SubmitChanges();
                        }

                        App.ApplicationSettings.Add(SYNC_KEYS + key, true);
                        App.ApplicationSettings.Save();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                        return false;
                    }
                }
            }

            return true;
        }


        public async static Task<bool> Refresh()
        {
            string key = Switcher.GetCurrentStorage().GetStorageName();
            App.ApplicationSettings.Remove(SYNC_KEYS + key);
            return await Synchronize(key);
        }


        public static void RemoveKey(string key)
        {
            if (App.ApplicationSettings.Contains(SYNC_KEYS + key))
                App.ApplicationSettings.Remove(SYNC_KEYS + key);
        }


        public static void RemoveAllKeys()
        {
            using (var itr = StorageHelper.GetStorageEnumerator())
            {
                while (itr.MoveNext())
                {
                    string key = itr.Current.GetStorageName();
                    if (App.ApplicationSettings.Contains(SYNC_KEYS + key))
                        App.ApplicationSettings.Remove(SYNC_KEYS + key);
                }
            }

        }


        public static List<FileObject> GetFilesFromRootFolder()
        {
            if (GetCurrentRoot() == null) return null;
            if (GetCurrentRoot().FileList == null) return null;

            GetCurrentTree().Clear();
            GetCurrentTree().Push(GetCurrentRoot());
            return GetCurrentRoot().FileList;
        }


        public static List<FileObject> GetTreeForFolder(FileObject folder)
        {
            if (folder == null) return null;
            if (folder.FileList == null) return null;
            if (!GetCurrentTree().Contains(folder)) GetCurrentTree().Push(folder);
            return folder.FileList;
        }


        public static List<FileObject> TreeUp()
        {
            if (GetCurrentTree().Count > 1) GetCurrentTree().Pop();
            return GetTreeForFolder(GetCurrentTree().First());
        }


        public static string GetCurrentPath()
        {
            FileObject[] array = GetCurrentTree().Reverse<FileObject>().ToArray<FileObject>();
            string str = String.Empty;
            foreach (FileObject f in array)
                str = str + f.Name + "/";
            return str;
        }


        public static FileObject GetCurrentRoot()
        {
            if (!DictionaryRoot.ContainsKey(Switcher.GetCurrentStorage().GetStorageName())) return null;
            return DictionaryRoot[Switcher.GetCurrentStorage().GetStorageName()];
        }


        public static Stack<FileObject> GetCurrentTree()
        {
            if (!DictionaryTree.ContainsKey(Switcher.GetCurrentStorage().GetStorageName())) return null;
            return DictionaryTree[Switcher.GetCurrentStorage().GetStorageName()];
        }
    }
}
