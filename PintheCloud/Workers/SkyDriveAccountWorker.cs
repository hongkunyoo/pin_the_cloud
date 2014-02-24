//using Microsoft.Live;
//using Microsoft.WindowsAzure.MobileServices;
//using PintheCloud.Managers;
//using PintheCloud.Models;
//using PintheCloud.Resources;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace PintheCloud.Workers
//{
//    public class SkyDriveAccountWorker
//    {
//        /*** Public ***/

       


//        // Register Live Connect Session for Live Profile
        


//        // Get User Profile information result using registered live connection session
//        public async Task<dynamic> GetProfileResultAsync(LiveConnectClient liveClient)
//        {
//            LiveOperationResult operationResult = null;
//            try
//            {
//                operationResult = await liveClient.GetAsync("me");
//            }
//            catch (LiveConnectException)
//            {
//                return null;
//            }

//            if (operationResult.Result == null)
//                return null;
//            else 
//                return operationResult.Result;
//        }



//        /*** private ***/

//        // Check whether it exists in DB
        
//    }
//}
