using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Windows.Storage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using Windows.System;
using PintheCloud.Models;
using PintheCloud.Pages;
using Microsoft.Phone.Tasks;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.IO;
using PintheCloud.Managers;
using System.Collections;

namespace PintheCloud.Utilities
{
    public partial class TestDrive : PtcPage
    {
        public TestDrive()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //List<FileObject> list = (List<FileObject>)PhoneApplicationService.Current.State["SELECTED_FILE"];
            //await doit(list);
            //await downfromBlob();
            //await TestStorages();
            await TestAccount();
        }

        public async Task TestAccount()
        {
            List<Spot> list = await App.MobileService.GetTable<Spot>().ToListAsync();
        }

        private async Task<StorageFile> GetStreamGiveFile(Stream input, string fileName)
        {
            byte[] buffer = new byte[1024000];
            int count = 0;
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            Stream ouput = await file.OpenStreamForWriteAsync();
            while ((count = input.Read(buffer, 0, buffer.Length)) != 0)
            {
                ouput.Write(buffer, 0, count);
            }
            input.Close();
            ouput.Close();
            return file;
        }

        protected  override void OnNavigatedFrom(NavigationEventArgs e)
        {
            
        }
        


        public void Test()
        {
            try
            {
                //FileObject fo = await App.SkyDriveManager.Synchronize();
                //FileObject.PrintFileObject(fo);
                //FileObject.PrintFileObjectList(await App.SkyDriveManager.GetFilesFromFolderAsync("folder.b96a113f78eb1c6f.B96A113F78EB1C6F!348"));
                //await App.SkyDriveManager.DownloadFile("file.b96a113f78eb1c6f.B96A113F78EB1C6F!314", new Uri("/shared/transfers/mytest.pdf", UriKind.Relative));
                //StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/shared/transfers/mytest.pdf"));

                //await App.SkyDriveManager.DownloadFile("file.b96a113f78eb1c6f.B96A113F78EB1C6F!314", new Uri("/"+PintheCloud.Managers.LocalStorageManager.SKYDRIVE_FOLDER+"/mytest.pdf", UriKind.Relative));
                //StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/" + PintheCloud.Managers.LocalStorageManager.SKYDRIVE_FOLDER + "/mytest.pdf"));
                //await Launcher.LaunchFileAsync(file);

                //await App.BlobManager.UploadFileAsync("myspace","path1/name.pdf",file);
                //FileObject.PrintFileObject(await App.BlobManager.GetFile("myspace","path1/Test.pdf"));
                //FileObject.PrintFileObjectList(await App.BlobManager.GetFilesFromFolderAsync("myspace","path1"));
                //FileObject.PrintFileObject(await App.BlobManager.GetFile("myspace", "path1"));
                //List<FileObject> list = await App.BlobManager.GetRootFilesAsync("myspace");
                //FileObject f = list[1];
                //System.Diagnostics.Debug.WriteLine(f.Id);
                //FileObject.PrintFileObjectList(await App.BlobManager.GetFilesFromFolderAsync(f.Id));
                //await App.LocalStorageManager.CreateFileToLocalSkyDriveStorageAsync("/mypath/myfolder/myfile");
                //await App.LocalStorageManager.CreateFileToLocalSkyDriveStorageAsync("/mypath/myfolder/myfile2");
                //await App.LocalStorageManager.CreateFileToLocalSkyDriveStorageAsync("/mypath/myfolder2/ttmyfile");
                //await App.LocalStorageManager.CreateFileToLocalSkyDriveStorageAsync("/mypath3/fefe/ttmyfile");
                //await App.LocalStorageManager.PrintFolderAsync(await App.LocalStorageManager.GetSkyDriveStorageFolderAsync());

                //Uri uri = await App.LocalStorageManager.GetSkyDriveDownloadUriFromPath("/mypath/testpath/mytest.pdf");
                //App.HDebug.WriteLine(uri);
                //await App.LocalStorageManager.PrintFolderAsync(await App.LocalStorageManager.GetSkyDriveStorageFolderAsync());
                //Uri uri = new Uri("/shared/transfers/mytest.pdf", UriKind.Relative);
                //await Launcher.LaunchFileAsync(await App.SkyDriveManager.DownloadFile("file.b96a113f78eb1c6f.B96A113F78EB1C6F!314", uri));
                
                //StorageFolder myfolder = await App.LocalStorageManager.CreateFolderToSkyDriveStorage("myfoldertest");
                //StorageFolder folder = await App.SkyDriveManager.DownloadFolderAsync("folder.b96a113f78eb1c6f.B96A113F78EB1C6F!1856", myfolder);
                //await Launcher.LaunchFileAsync(await App.LocalStorageManager.GetSkyDriveStorageFileAsync("myfoldertest/pzl.docx"));
                //await Launcher.LaunchFileAsync(await App.SkyDriveManager.DownloadFileAsync("file.b96a113f78eb1c6f.B96A113F78EB1C6F!390", new Uri(("/Shared/Transfers/skydrive/f ee e ee.zip"), UriKind.Relative)));
                //await App.LocalStorageManager.PrintFolderAsync(folder);
                //Popup p = new Popup();
                //MyDebug.WriteLine("in TestDrive");
                //await p.showSkyDriveAsync();
                //MyDebug.WriteLine("start download");
                //await App.SkyDriveManager.DownloadFileAsync("file.b96a113f78eb1c6f.B96A113F78EB1C6F!314", new Uri("/shared/transfers/mytest.pdf", UriKind.Relative));
                //MyDebug.WriteLine("end download");
                //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/shared/transfers/mytest.pdf"));
                //await App.SkyDriveManager.UploadFileAsync("folder.b96a113f78eb1c6f.B96A113F78EB1C6F!1856", file);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            } 
        }

        private void button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //PhotoChooserTask photoChooserTask = new PhotoChooserTask();
            //photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
            //photoChooserTask.Show();
            //Debug.WriteLine(GoogleDriveManager.ExtensionMapper.Keys);
            foreach (string key in GoogleDriveManager.ExtensionMapper.Keys)
            {
                Debug.WriteLine(key + " : " + GoogleDriveManager.ExtensionMapper[key]);
            }
        }

        private void button_Click2(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(EventHelper.SETTINGS_PAGE, UriKind.Relative));
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            Debug.WriteLine("Chooser Done");
        }
    }
}