using LauncherWPFUiTest.Model;
using LauncherWPFUiTest.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using System.Xml.Linq;
using System.IO.Enumeration;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace LauncherWPFUiTest.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        //private string XmlFilePath = @"C:\Users\kimgu\source\repos\LauncherWPFUiTest\LauncherWPFUiTest\ProgramsPath.xml";
        private string XmlFilePath = @"\\Gms-mcc-nas01\audio-file\test1\ProgramsPath.xml";

        public FileCopyManager fileCopyManager = new FileCopyManager();

        public ObservableCollection<Program> Programs { get; set; } = new ObservableCollection<Program>();

        public ObservableCollection<bool> flags { get; set; } = new ObservableCollection<bool>();
        public bool _flag;
        public bool Flag
        {
            get => _flag;
            set
            {
                _flag = value;
                OnPropertyChanged(nameof(Flag));
            }
        }


        private Program _selectedProgram;
        public Program SelectedProgram
        {
            get => _selectedProgram;
            set
            {
                if (_selectedProgram != value)
                {
                    _selectedProgram = value;
                    OnPropertyChanged(nameof(SelectedProgram));
                    SelectedVersion = null;
                }
            }
        }

        private VersionInfo _selectedVersion;
        public VersionInfo SelectedVersion
        {
            get => _selectedVersion;
            set
            {
                _selectedVersion = value;
                OnPropertyChanged(nameof(SelectedVersion));

                //  버전 선택 시 패치 노트 로드
                _selectedVersion?.LoadMetaData();
                OnPropertyChanged(nameof(PatchNote));
            }
        }


        public string PatchNote => _selectedVersion?.PatchNote ?? "";
        public ICommand LaunchProgramCommand { get; }
        public ICommand LoadFilesCommand { get; }
        public ICommand SelectProgramCommand { get; }


        //public ICommand FileCopyCommand { get; }

        public MainViewModel()
        {
            // 프로그램 아이콘 불러오기
            LoadFilesCommand = new RelayCommand(_ => LoadPrograms());
            // 선택된 버전 프로그램 실행하기 
            LaunchProgramCommand = new RelayCommand(_ => LaunchSelectedVersion(), _ => SelectedVersion != null);
            // 선택된 프로그램 버전
            SelectProgramCommand = new RelayCommand(param => SetSelectedProgram(param));

            //FileCopyCommand = new RelayCommand(param => fileCopyManager.CopyProgramFiles(param));

        }




        private void LoadPrograms()
        //서비스로 옮겨줘야함 
        {



            if (!File.Exists(XmlFilePath))
                return;

            Programs.Clear();

            //  런처 XML을 역직렬화하여 프로그램 폴더 경로 가져오기
            XmlSerializer launcherSerializer = new XmlSerializer(typeof(LauncherConfig));
            LauncherConfig launcherConfig;

            using (StreamReader reader = new StreamReader(XmlFilePath))
            {
                launcherConfig = (LauncherConfig)launcherSerializer.Deserialize(reader);
            }

            if (string.IsNullOrEmpty(launcherConfig.ProgramsFolder) || !Directory.Exists(launcherConfig.ProgramsFolder))
                return;

            // 
            var programFolders = Directory.GetDirectories(launcherConfig.ProgramsFolder);
            // unc 경로에 위치한 폴더 목록  가져오기

            foreach (var programFolder in programFolders)

            {
                string programXmlPath = Path.Combine(programFolder, "program.xml");
                // 각 프로그램 폴더의 program.xml 

                if (!File.Exists(programXmlPath))
                    continue;

                // program.xml을 Program 객체로 변환
                XmlSerializer programSerializer = new XmlSerializer(typeof(Program));
                Program programData;
                //여기서 접근 

                using (StreamReader reader = new StreamReader(programXmlPath))
                {
                    programData = (Program)programSerializer.Deserialize(reader);
                }

                // 프로그램 폴더 경로 & 아이콘 경로 업데이트
                programData.FolderPath = programFolder;
                programData.IconPath = Path.Combine(programFolder, programData.IconPath);



                // : UI 업데이트를 위해 `Programs` 리스트에 추가
                Programs.Add(programData);
            }
        }
        private void LaunchSelectedVersion()
        {
         
            
            if (SelectedVersion == null)
                return;

            // 설치 경로를 알고있어서 
            Microsoft.Win32.OpenFolderDialog dialog = new();
            string exePath;
            bool? result = dialog.ShowDialog();
            string lastFolder = Path.GetFileName(SelectedVersion.Path);
            
            if (result == true) {

                 exePath = dialog.FolderName;

                string[] Paths = new string[] { SelectedVersion.Path, exePath+ $"\\{lastFolder}" };
                DirectoryInfo di = new DirectoryInfo(Paths[1]);
                //경로에 이미 디렉토리가 있으면 
                if (di.Exists)
                {
                    MessageBox.Show("이미 설치된 프로그램입니다.");
                    return;
                }
                fileCopyManager.CopyProgramFiles(Paths);
            }




            //Process.Start(new ProcessStartInfo
            //{
            //    //FileName = SelectedVersion.Path,
            //    //UseShellExecute = true
            //});
        }

        private void SetSelectedProgram(object param)
        {
            if (param is Program selected)
            {
                SelectedProgram = selected;

            }
        }


    }
}