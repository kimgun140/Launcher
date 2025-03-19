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
        //private string XmlFilePath = @"\\Gms-mcc-nas01\audio-file\test1\ProgramsPath.xml";

        public FileCopyManager fileCopyManager = new FileCopyManager();

        public ObservableCollection<Program> Programs { get; set; } = new ObservableCollection<Program>();

        //public IProgress<int> Progress { get; set; } = new Progress<int>();

        public bool _flag;
        public bool Flag
        {
            get => _flag;
            set
            {
                _flag = value;
                if (_flag)
                {
                    _buttonContent = "설치";
                }
                else
                {
                    _buttonContent = "실행";
                }
                OnPropertyChanged(nameof(Flag));
            }
        }
        public string _buttonContent;

        public string ButtonContent
        {
            get => _buttonContent;
            set
            {
                _buttonContent = value;
                OnPropertyChanged(nameof(ButtonContent));
            }
        }

        public string _deleteButtonContent;
        public string DeleteButtonContent
        {
            get => _deleteButtonContent;
            set
            {
                _deleteButtonContent = value;
                OnPropertyChanged(nameof(DeleteButtonContent));

            }
        }
        private Program _selectedProgram;
        public Program SelectedProgram
        //프로그램 선택 
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
                InstallOrRun();//버튼 콘텐트 변경 

            }
        }
        //프로그래스바 
        private int _progressBarValue;
        public int ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                _progressBarValue = value;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }

        //private decimal _loadDataMax;
        //public decimal LoadDataMax
        //{
        //    get => _loadDataMax;
        //    set
        //    {
        //        _loadDataMax = value;
        //        OnPropertyChanged(nameof(LoadDataMax));
        //    }
        //}




        public string PatchNote => _selectedVersion?.PatchNote ?? "";
        public ICommand LaunchProgramCommand { get; }
        public ICommand LoadFilesCommand { get; }
        public ICommand SelectProgramCommand { get; }

        public ICommand DeleteProgramCommand { get; }

        public ICommand ProgressBarCommand { get; }

        public MainViewModel()
        {
            // 프로그램 목록 불러오기
            LoadPrograms();
            // 선택된 버전 프로그램 실행하기 
            LaunchProgramCommand = new RelayCommand(_ => LaunchSelectedVersion(), _ => SelectedVersion != null);
            // 선택된 프로그램 버전
            SelectProgramCommand = new RelayCommand(param => SetSelectedProgram(param));

            DeleteProgramCommand = new RelayCommand(_ => DeleteProgram(), _ => SelectedProgram != null);

            ProgressBarCommand = new RelayCommand(_ => ProgressBar(), _ => SelectedProgram != null);
        }

        private ObservableCollection<string> ProgressBar()
        {
            ObservableCollection<string> result = new ObservableCollection<string>();



            return result;

        }

        public void DeleteProgram()
        // 삭제
        {
            if (SelectedProgram == null)
                return;

            string versionFolderName = Path.GetFileName(SelectedVersion.Path);
            string programFolderName = Path.GetFileName(SelectedProgram.FolderPath);
            string relativeInstallPath = Path.Combine(programFolderName, versionFolderName);
            string fullInstallPath = Path.Combine(@"C:\Program Files\launcherfolder", relativeInstallPath);

            fileCopyManager.deleteDirectory(fullInstallPath);

        }

        private void LoadPrograms()
        {

            Programs = fileCopyManager.LoadPrograms();
 
        }
        private void InstallOrRun()
        {
            if (SelectedVersion == null)
            {
                // SelectedVersion이 null인 경우 처리
                ButtonContent = "설치";
                return;
            }
            //// 설치 경로를 알고있어서 
            string versionFolderName = Path.GetFileName(SelectedVersion.Path);
            string programFolderName = Path.GetFileName(SelectedProgram.FolderPath);
            string relativeInstallPath = Path.Combine(programFolderName, versionFolderName);


            //버전이름으로 디렉토리
            string fullInstallPath = Path.Combine(@"C:\Program Files\launcherfolder", relativeInstallPath);
            //플래그 바꿔야지 
            Flag = Directory.Exists(fullInstallPath);
            ButtonContent = Flag ? "실행" : "설치";

        }
        private async void LaunchSelectedVersion()
        //실행설치버튼 
        {//프로그램 버전이 선택될 때  아래 경로를 확인해서 


            if (SelectedVersion == null)
                return;

            //// 설치 경로를 알고있어서 
            string lastFolder = Path.GetFileName(SelectedVersion.Path);
            //버전이름 
            string installPath = Path.Combine(@"C:\Program Files\launcherfolder", lastFolder);


            string versionFolderName = Path.GetFileName(SelectedVersion.Path);
            string programFolderName = Path.GetFileName(SelectedProgram.FolderPath);
            string relativeInstallPath = Path.Combine(programFolderName, versionFolderName);
            string fullInstallPath = Path.Combine(@"C:\Program Files\launcherfolder", relativeInstallPath);

            //(SelectedVersion.Path, SelectedProgram.FolderPath);

            string[] Paths = new string[] { SelectedVersion.Path, fullInstallPath };


            DirectoryInfo di = new DirectoryInfo(Paths[1]);
            // 설치확인 

            //경로에 이미 디렉토리가 있으면 
            if (di.Exists)
            {
                MessageBox.Show("이미 설치된 프로그램입니다. 실행");
                //Process.Start(new ProcessStartInfo
                //{
                //    //FileName = SelectedVersion.Path,
                //    //UseShellExecute = true
                //});

                return;


            }
            else
            {
                MessageBox.Show("설치합니다");
                //fileCopyManager.CopyProgramFiles(Paths);
                var progress = new Progress<int>(value => ProgressBarValue = value);
                await fileCopyManager.ProgressUpdate(progress, SelectedVersion.Path, fullInstallPath);
                //설치하기 

            }
        }

        private void SetSelectedProgram(object param)
        {
            //버전 선택하면 바꾸기 
            if (param is Program selected)
            {
                SelectedProgram = selected;


            }
        }


    }
}