using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace LauncherWPFUiTest.Model
{
    //[XmlRoot("Program")]
    public class Program
    {
        [XmlElement("Name")]
        public string ProgramName { get; set; }

        [XmlIgnore] // 폴더 경로는 직렬화하지 않음
        public string FolderPath { get; set; }

        [XmlArray("Versions")]
        [XmlArrayItem("Version")]
        public ObservableCollection<VersionInfo> Versions { get; set; } = new ObservableCollection<VersionInfo>(); // 버전 목록

        [XmlElement("Icon")]
        public string IconPath { get; set; }

        [XmlIgnore] // BitmapImage는 직렬화할 수 없으므로 제외
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
        //programmetadata.xml
    {
        [XmlElement("Number")]
        public string Number { get; set; }

        [XmlElement("Path")]
        public string Path { get; set; }

        [XmlIgnore] // PatchNote는 직렬화에서 제외
        public string PatchNote { get; set; }

        [XmlIgnore] // PatchNote는 직렬화에서 제외

        public ObservableCollection<string> PatchNotes { get; set; }

        public void LoadMetaData()
        {
            string metadataPath = System.IO.Path.Combine(Path, "ProgramMetaData.xml");
            if (!File.Exists(metadataPath))
            {
                //PatchNote = "패치 노트 없음";
                return;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(ProgramMetaData));
            using (StreamReader reader = new StreamReader(metadataPath))
            {
                ProgramMetaData metaData = (ProgramMetaData)serializer.Deserialize(reader);
                PatchNotes = new ObservableCollection<string>();
                PatchNote = metaData.PatchNote; // 

                PatchNotes.Add(metaData.PatchNote);
            }
        }
    }
    // 버전별 데이터 
    [XmlRoot("ProgramMetaData")]
    public class ProgramMetaData
    {
        [XmlElement("Version")]
        public string Version { get; set; }

        [XmlElement("PatchNote")]
        public string PatchNote { get; set; }
    }

    // 런처가 가지고 있는 런처에 등록된 프로그램들의 경로
    [XmlRoot("Launcher")]
    public class LauncherConfig
    {
        [XmlElement("ProgramsFolder")]
        public string ProgramsFolder { get; set; }

        [XmlElement("UncPath")]
        public string UncPath { get; set; }

        [XmlElement("UserName")]
        public string UserName { get; set; }

        [XmlElement("UserPassword")]
        public string UserPassword { get; set; }


    }
}
