using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud
{
    class GlobalKeys
    {
        // Azure
        public static string AZURE_CLIENT_ID = "0000000044110129";
        public static string AZURE_MOBILE_SERVICE_ID = "MicrosoftAccount:2914cb486d0f9106050de9ad70564d53";
        public static string AZURE_MOBILE_SERVICE_TOKEN 
            = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IjAifQ.eyJleHAiOjEzOTQ5Nzg4NTcsImlzcyI6InVybjptaWNyb3NvZnQ6d2luZG93cy1henVyZTp6dW1vIiwidmVyIjoyLCJhdWQiOiJNaWNyb3NvZnRBY2NvdW50IiwidWlkIjoiTWljcm9zb2Z0QWNjb3VudDoyOTE0Y2I0ODZkMGY5MTA2MDUwZGU5YWQ3MDU2NGQ1MyIsInVybjptaWNyb3NvZnQ6Y3JlZGVudGlhbHMiOiJrc2VzY21WaXA1b2ZrZDhUenBQQ1h3PT0ifQ.cUrvBbXsHQOiz0ZRu8FxA5HxqpQbPRSQQb8_N0-6eAo";


        // Platform Id
        public static string[] PLATFORMS = { "SkyDrive", "Dropbox" };


        // Location
        public static int SKY_DRIVE_LOCATION_KEY = 0;
        public static int DROPBOX_LOCATION_KEY = 1;
         

        // Current Usrer
        public static string USER = "";
    }
}
