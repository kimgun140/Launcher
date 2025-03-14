using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LauncherWPFUiTest.Model
{
    public class Program
    {
       public string ProgramName { get; set; } //
        public string FileName { get; set; } // 파일명 
        public string FilePath {  get; set; }// 폴더 경로? xml파일 거기에는 

        public BitmapImage ImageSource
        {
            get
            {
                if (System.IO.File.Exists(FilePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(FilePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                return null;
            }
        }

    }
}
