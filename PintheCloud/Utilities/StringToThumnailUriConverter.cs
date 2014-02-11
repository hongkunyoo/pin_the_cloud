using PintheCloud.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PintheCloud.Utilities
{
    public class StringToThumnailUriConverter : IValueConverter
    {
        // Instances
        private const string FOLDER_IMAGE_PATH = "/Assets/pajeon/png/icon_file_folder.png";
        private const string DOC_IMAGE_PATH = "/Assets/pajeon/png/icon_file_doc.png";
        private const string ETC_IMAGE_PATH = "/Assets/pajeon/png/icon_file_etc.png";
        private const string HWP_IMAGE_PATH = "/Assets/pajeon/png/icon_file_hwp.png";
        private const string IMG_IMAGE_PATH = "/Assets/pajeon/png/icon_file_image.png";
        private const string PDF_IMAGE_PATH = "/Assets/pajeon/png/icon_file_pdf.png";
        private const string PPT_IMAGE_PATH = "/Assets/pajeon/png/icon_file_ppt.png";
        private const string SOUND_IMAGE_PATH = "/Assets/pajeon/png/icon_file_sound.png";
        private const string VIDEO_IMAGE_PATH = "/Assets/pajeon/png/icon_file_video.png";
        private const string XLS_IMAGE_PATH = "/Assets/pajeon/png/icon_file_xls.png";
        private const string ZIP_IMAGE_PATH = "/Assets/pajeon/png/icon_file_zip.png";

        // Sound
        private const string MP3 = "mp3";
        private const string WMA = "wma";

        // Video
        private const string MP4 = "mp4";
        private const string MPEG = "mpeg";
        private const string WMV = "wmv";
        private const string AVI = "avi";

        // Image
        private const string JPG = "jpg";
        private const string JPEG = "jpeg";
        private const string PNG = "png";

        // Document
        private const string PPT = "ppt";
        private const string PDF = "pdf";
        private const string DOC = "doc";
        private const string DOCX = "docx";
        private const string XLS = "xls";
        private const string HWP = "hwp";

        // ETC
        private const string ZIP = "zip";


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string type = ((string)value).ToLower();

            if (type.Equals(AppResources.Folder))  // Folder
                return new Uri(FOLDER_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(MP3) || type.Equals(WMA))  // Sound
                return new Uri(SOUND_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(MP4) || type.Equals(WMV) || type.Equals(MPEG) || type.Equals(AVI))  // video
                return new Uri(VIDEO_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(JPG) || type.Equals(JPEG) || type.Equals(PNG))  // Image
                return new Uri(IMG_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(PPT))  // PPT
                return new Uri(PPT_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(DOC) || type.Equals(DOCX))  // Word
                return new Uri(DOC_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(XLS))  // XLS
                return new Uri(VIDEO_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(HWP))  // HWP
                return new Uri(VIDEO_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(PDF))  // PDF
                return new Uri(PDF_IMAGE_PATH, UriKind.Relative);

            else if (type.Equals(ZIP))  // ZIP
                return new Uri(ZIP_IMAGE_PATH, UriKind.Relative);

            else
                return new Uri(ETC_IMAGE_PATH, UriKind.Relative);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}