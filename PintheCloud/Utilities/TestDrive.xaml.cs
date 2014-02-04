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

namespace PintheCloud.Utilities
{
    public partial class TestDrive : PhoneApplicationPage
    {
        public TestDrive()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await Test();
        }

        public async Task Test()
        {
            try
            {
                //FileObject fo = await App.SkyDriveManager.Synchronize();
                //FileObject.PrintFileObject(fo);
                //await App.SkyDriveManager.DownloadFile("file.b96a113f78eb1c6f.B96A113F78EB1C6F!314", new Uri("/shared/transfers/mytest.pdf", UriKind.Relative));
                //StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/shared/transfers/mytest.pdf"));
                //await Launcher.LaunchFileAsync(file);
                //await App.BlobManager.UploadFileAsync("myspace","path1/name.pdf",file);
                //FileObject.PrintFileObject(await App.BlobManager.GetFile("myspace","path1/Test.pdf"));
                //List<FileObject> list = await App.BlobManager.GetRootFilesAsync("myspace");
                //FileObject f = list[1];
                //System.Diagnostics.Debug.WriteLine(f.Id);
                //FileObject.PrintFileObjectList(await App.BlobManager.GetFilesFromFolderAsync(f.Id));
                await App.CurrentLocalStorageManager.CreateFileToLocalSkyDriveStorageAsync("/mypath/myfolder/myfile");
                await App.CurrentLocalStorageManager.CreateFileToLocalSkyDriveStorageAsync("/mypath/myfolder/myfile2");
                await App.CurrentLocalStorageManager.CreateFileToLocalSkyDriveStorageAsync("/mypath/myfolder2/ttmyfile");
                await App.CurrentLocalStorageManager.CreateFileToLocalSkyDriveStorageAsync("/mypath3/fefe/ttmyfile");
                await App.CurrentLocalStorageManager.PrintFolderAsync(await App.CurrentLocalStorageManager.GetSkyDriveStorageFolderAsync());


                /*
                 *  skydrive(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive)
                      mypath(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath)
                       myfolder(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath\myfolder)
                        myfile(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath\myfolder\myfile)
                        myfile2(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath\myfolder\myfile2)
                       myfolder2(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath\myfolder2)
                        ttmyfile(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath\myfolder2\ttmyfile)
                      mypath3(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath3)
                       fefe(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath3\fefe)
                        ttmyfile(C:\Data\Users\DefApps\AppData\{76C7756F-1A04-4893-9B4F-378BE731EBDF}\Local\skydrive\mypath3\fefe\ttmyfile)
                 * 
                 */
            }
            catch(Exception e)
            {
                App.HDebug.WriteLine(e.ToString());
            }
            
            
        }
    }
}