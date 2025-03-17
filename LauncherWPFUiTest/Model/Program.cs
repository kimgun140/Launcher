using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LauncherWPFUiTest.Model
{
    public class Program
    {
        public string ProgramName { get; set; }
        public string FolderPath { get; set; }
        public ObservableCollection<VersionInfo> Versions { get; set; } = new ObservableCollection<VersionInfo>(); // 버전 목록
        public string IconPath { get; set; }

        public BitmapImage IconSource
        {
            get
            {
                if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(IconPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                return null;
            }
        }



    }
    public class VersionInfo
    {
        public string Number { get; set; }
        public string Path { get; set; }
        public ObservableCollection<ImageFile> Images { get; set; } = new ObservableCollection<ImageFile>(); // 이미지 리스트

    }

    public class ImageFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public BitmapImage ImageSource
        {
            get
            {
                if (File.Exists(FilePath))
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
