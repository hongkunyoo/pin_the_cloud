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
    public static class StorageExplorer<T>
    {
        private static FileObject Root;
        private static Stack<FileObject> FolderTree = new Stack<FileObject>();
        private static PintheCloud.Utilities.ConvertTemplate.ConvertFirstToSecond<FileObject, T> ReverseFileObject;
        private static PintheCloud.Utilities.ConvertTemplate.ConvertFirstToSecond<T, FileObject> ConvertToFileObject;
        private static string SQL_DATABASE_SET = "SQL_DATABASE_SET";

        public async static Task<bool> Synchronize()
        {
            if (App.ApplicationSettings.Contains(SQL_DATABASE_SET))
            {
                ////////////////////////////////////////////
                // TODO : Retrieve Data from DATABASE;
                ////////////////////////////////////////////

                App.ApplicationSettings[SQL_DATABASE_SET] = true;
                return true;
            }
            else
            {
                try
                {
                    await TaskHelper.WaitSignInTask(Switcher.GetCurrentStorage().GetStorageName());
                    IStorageManager storageManager = Switcher.GetCurrentStorage();
                    Root = await storageManager.Synchronize();

                    ////////////////////////////////////////////
                    // TODO : SAVE Data to DATABASE;
                    ////////////////////////////////////////////

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
            App.ApplicationSettings.Remove(SQL_DATABASE_SET);
            await Synchronize();
        }
        public static FileObject GetRootFile()
        {
            return Root;
        }

        public static List<FileObject> GetTreeForFolder(FileObject folder, Action<FileObject> action, ObservableCollection<T> viewList)
        {
            List<FileObject> list = folder.FileList;
            action(folder);

            for (var i = 0; i < list.Count; i++)
            {
                viewList.Add(ConvertTo(list[i]));
            }

            if (!FolderTree.Contains(folder))
                FolderTree.Push(folder);

            return list;
        }
        public static List<FileObject> TreeUp(Action<FileObject> action, ObservableCollection<T> viewList)
        {
            if (FolderTree.Count > 1)
            {
                FolderTree.Pop();
                return GetTreeForFolder(FolderTree.First(), action, viewList);
            }
            return null;
        }


        public static void SetConvertToFileObject(PintheCloud.Utilities.ConvertTemplate.ConvertFirstToSecond<T, FileObject> convert)
        {
            ConvertToFileObject = convert;
        }
        public static void SetReverseFileObject(PintheCloud.Utilities.ConvertTemplate.ConvertFirstToSecond<FileObject, T> reverse)
        {
            ReverseFileObject = reverse;
        }
        public static T ConvertTo(FileObject file)
        {
            return ReverseFileObject(file);
        }
        public static FileObject ConvertTo(T tt)
        {
            return ConvertToFileObject(tt);
        }
    }
}
