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
        //설치할때만 프로그래스박 보이기 
        private bool _progressBarVisibility;
        public bool ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                _progressBarVisibility = value;
                OnPropertyChanged(nameof(ProgressBarVisibility));
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
                    //SelectedVersion = Programs[0].Versions[0].Number;
                    // 프로그램목록이 로드될 때 첫번째 프로그램, 그 프로그램의 첫 버전이 선택되게 하고 싶음 아니면기본화면이 있어야하겠지 

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





        public string PatchNote => _selectedVersion?.PatchNote ?? "";
        public ICommand LaunchProgramCommand { get; }
        public ICommand LoadFilesCommand { get; }
        public ICommand SelectProgramCommand { get; }

        public ICommand DeleteProgramCommand { get; }

        public ICommand RepairProgramCommand { get; }
        public ICommand backupCommand { get; }

        public MainViewModel()
        {
            // 프로그램 목록 불러오기
            LoadPrograms();
            // 선택된 버전의 프로그램 실행 or 설치 하기 
            LaunchProgramCommand = new RelayCommand(_ => LaunchSelectedVersion(), _ => SelectedVersion != null);
            // 선택된 프로그램 버전
            SelectProgramCommand = new RelayCommand(param => SetSelectedProgram(param));
            // 이게 선택을 했을때 전달되는건데 흠 그러면 선택을 해서 가져오는게 아니라 
            // 리스트에서 선택을 하면 그 내용이 전달이 되는건데 선택을 안한 상태에서 전달이 되어야 초기값이 나오는 설정이 가능하겠는데 
            // 그러면 로드됐을 떄 나오게 하면 되겠다. 로드 되었을때 선택이 되는 식으로 
            // 삭제 하기
            DeleteProgramCommand = new RelayCommand(_ => DeleteProgram(), _ => SelectedProgram != null);
            RepairProgramCommand = new RelayCommand(_ => RepairProgram(), _ => SelectedProgram != null);
            backupCommand = new RelayCommand(_ => settingsbackup(), _ => SelectedProgram != null);

        }


        public void DeleteProgram()
        // 삭제
        {
            if (SelectedProgram == null)
                return;
            MessageBox.Show("프로그램 삭제");
            //string versionFolderName = Path.GetFileName(SelectedVersion.Path);
            //string programFolderName = Path.GetFileName(SelectedProgram.FolderPath);
            //string relativeInstallPath = Path.Combine(programFolderName, versionFolderName);
            //string fullInstallPath = Path.Combine(@"C:\Program Files\launcherfolder", relativeInstallPath);

            string fullInstallPath = fileCopyManager.GetInstallPath(SelectedProgram.FolderPath, SelectedVersion.Path);

            MessageBox.Show("삭제");
            fileCopyManager.deleteDirectory(fullInstallPath);
            InstallOrRun();

        }

        private void LoadPrograms()
        {

            Programs = fileCopyManager.LoadPrograms();
            InstallOrRun();


        }
        private void InstallOrRun()
        {
            if (SelectedVersion == null)
            {
                // SelectedVersion이 null인 경우 처리
                ButtonContent = "설치";
                return;
            }
            bool isInstalled = fileCopyManager.IsProgramInstalled(SelectedProgram.FolderPath, SelectedVersion.Path);
            Flag = isInstalled;
            ButtonContent = Flag ? "실행" : "설치";

        }
        //private async void LaunchSelectedVersion()
        ////실행설치버튼 
        //{//프로그램 버전이 선택될 때  아래 경로를 확인해서 
        //    // 처음 로드가 되면 프로그램이랑 버전으 선택한걸로 되서 보여줄 수 있게는 해야겠다. 

        //    try
        //    {

        //        if (SelectedVersion == null)
        //            return;

        //        //// 설치 경로를 알고있어서 
        //        string lastFolder = Path.GetFileName(SelectedVersion.Path);
        //        //버전이름 
        //        string installPath = Path.Combine(@"C:\Program Files\launcherfolder", lastFolder);


        //        string versionFolderName = Path.GetFileName(SelectedVersion.Path);
        //        string programFolderName = Path.GetFileName(SelectedProgram.FolderPath);
        //        string relativeInstallPath = Path.Combine(programFolderName, versionFolderName);
        //        string fullInstallPath = Path.Combine(@"C:\Program Files\launcherfolder", relativeInstallPath);

        //        //(SelectedVersion.Path, SelectedProgram.FolderPath);

        //        string[] Paths = new string[] { SelectedVersion.Path, fullInstallPath };


        //        DirectoryInfo di = new DirectoryInfo(Paths[1]);
        //        // 설치확인 

        //        //경로에 이미 디렉토리가 있으면 
        //        if (di.Exists)
        //        {
        //            MessageBox.Show("이미 설치된 프로그램입니다. 실행");
        //            //Process.Start(new ProcessStartInfo
        //            //{
        //            //    //FileName = SelectedVersion.Path,
        //            //    //UseShellExecute = true
        //            //});
        //            return;

        //        }
        //        else
        //        {
        //            MessageBox.Show("설치합니다");
        //            var progress = new Progress<int>(value => ProgressBarValue = value);
        //            await fileCopyManager.ProgressUpdate(progress, SelectedVersion.Path, fullInstallPath, SetProgressBarVisibility);
        //            InstallOrRun();
        //            //알

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message);
        //    }
        //}


        private async void LaunchSelectedVersion()
        {
            try
            {
                if (SelectedVersion == null)
                    return;

                // 설치 여부 확인 (서비스에 위임)
                bool isInstalled = fileCopyManager.IsProgramInstalled(SelectedProgram.FolderPath, SelectedVersion.Path);

                if (isInstalled)
                {
                    MessageBox.Show("이미 설치된 프로그램입니다. 실행");
                    //fileCopyManager.RunProgram(SelectedVersion.Path);
                    return;
                }
                else
                {
                    MessageBox.Show("설치합니다");

                    var progress = new Progress<int>(value => ProgressBarValue = value);
                    await fileCopyManager.InstallProgram(progress, SelectedVersion.Path, SelectedProgram.FolderPath, SetProgressBarVisibility);

                    InstallOrRun(); // UI 상태 업데이트
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void SetProgressBarVisibility(bool isVisible)
        {
            ProgressBarVisibility = isVisible;
        }

        private void SetSelectedProgram(object param)
        {
            //버전 선택하면 바꾸기 
            if (param is Program selected)
            {
                SelectedProgram = selected;


            }
        }
        private void RepairProgram()
        {
            //선택된 프로그램 버전을 삭제하고 다시 설치
            DeleteProgram();
            //
            LaunchSelectedVersion();
        }
        private void settingsbackup()
        {
            //
            MessageBox.Show("설정 백업");
            fileCopyManager.OptionBackup(SelectedProgram.FolderPath, SelectedVersion.Path);
        }

    }
}