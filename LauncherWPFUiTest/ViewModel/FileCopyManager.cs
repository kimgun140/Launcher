using LauncherWPFUiTest.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
//using FileSystem.Directory;

namespace LauncherWPFUiTest.ViewModel
{
    public class FileCopyManager
    {
        [DllImport("mpr.dll")]
        // Windows API 함수 WNetAddConnection2를 호출하기 위한 선언입니다. 
        // Multiple Provider Router DLL 에서 제공하는 함수, smb 네트워크 드라이브 연결을 설정하는 역할을 한다. netresource 구조체를 전달하여 네트워크 경로를 지정하고, username과 password로 인증을 수행한다. 
        // 스토리지 접근권한 주기
        private static extern int WNetAddConnection2(ref NETRESOURCE netResource, string password, string username, int flags);

        [DllImport("mpr.dll")]
        // smb 네트워크 드라이브 연결을 해재하는 기능을 한다.
        private static extern int WNetCancelConnection2(string name, int flags, bool force);
        [StructLayout(LayoutKind.Sequential)]// 메모리에 순차적으로 배치하다
        private struct NETRESOURCE
        // 네트워크 리소스 나타내는 구조체 
        {
            public int dwScope;
            public int dwType;//리소스 유형 1 이면 디스크 드라이브 
            public int dwDisplayType;
            public int dwUsage;
            public string lpLocalName;// 로컬드라이브 문자, 매핑할 경우 사용 
            public string lpRemoteName;// 원격 경로
            public string lpComment;
            public string lpProvider;
        }

        //string uncPath = @"\\gms-mcc-nas01\AUDIO-FILE\test1\test121.txt";
        ////string content = "이것은 공유 스토리지에 저장되는 테스트 파일입니다.";
        //public string username = "develop";
        //public string password = "Akds0ft3!48";

        public const string XmlFilePath = @"C:\Program Files\launcherfolder\ProgramsPath.xml";
        //런처 폴더 경로

        // 프로그램 설치 폴더 경로 
        public const string installedPath = @"C:\Program Files\LauncherPrograms";

        //public string XmlFilePaths = Path.Combine(XmlFilePath, "ProgramsPath.xml");

        LauncherConfig launcherConfig;




        public ObservableCollection<Program> LoadPrograms()
        {

            ObservableCollection<Program> programs = new ObservableCollection<Program>();

            if (!File.Exists(XmlFilePath))
                return new ObservableCollection<Program>();

            programs.Clear();

            //  런처 XML을 역직렬화하여 프로그램 폴더 경로 가져오기
            XmlSerializer launcherSerializer = new XmlSerializer(typeof(LauncherConfig));
            //if (launcherConfig == null)

                using (StreamReader reader = new StreamReader(XmlFilePath))
                {
                    launcherConfig = (LauncherConfig)launcherSerializer.Deserialize(reader);
                }

            if (string.IsNullOrEmpty(launcherConfig.ProgramsFolder) || !Directory.Exists(launcherConfig.ProgramsFolder))
                return new ObservableCollection<Program>();
            // 예외 처리가 되어있네 재시도를 자동으로 하게 하는게 좋을 까 아니면 사용자에게 알림을 주고 재시작하게 하는게 좋을까 

            // 
            var programFolders = Directory.GetDirectories(launcherConfig.ProgramsFolder);
            // unc 경로에 위치한 폴더 목록  가져오기

            foreach (var programFolder in programFolders)
            //등록된 프로그램 마다 
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
                programs.Add(programData);

            }
            // 프로그램 데이터 
            return programs;

        }



        public void Connection()
        {

            //unc경로, 아이디 , 비밀번호 읽기 
            XmlSerializer launcherSerializer = new XmlSerializer(typeof(LauncherConfig));

            using (StreamReader reader = new StreamReader(XmlFilePath))
            {
                launcherConfig = (LauncherConfig)launcherSerializer.Deserialize(reader);
            }
            //if (string.IsNullOrEmpty(launcherConfig.ProgramsFolder) || !Directory.Exists(launcherConfig.ProgramsFolder))
            //    return;
            string path = launcherConfig.UncPath;
            string username = launcherConfig.UserName;
            string password = launcherConfig.UserPassword;


            NETRESOURCE netResource = new NETRESOURCE
            {
                dwType = 1,// 파일드라이버처럼 처리
                lpRemoteName = path
            };
            // 구조체 초기화해 경로 설정 
            int result = WNetAddConnection2(ref netResource, password, username, 0);


            //연결 끊기 
            //WNetCancelConnection2(path, 0, true);

            if (result == 0)// 0이면 연결 
            {
                //Task.Delay(10000000);
                MessageBox.Show("연결");


            }
            else
            {

                MessageBox.Show(result.ToString());
            }
        }

        public void RunProgram(string programFolder, string versionPath)
        {
            const string exefile = "asdfsadf.exe";
            // xml에서 읽어오거나 정해놓거나 
            string installPath = GetInstallPath(programFolder, versionPath);

            string exePath = Path.Combine(installPath, exefile);
            if (!File.Exists(exePath))
            {
                MessageBox.Show("실행할 파일이 없습니다.");
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


            //ProcessStartInfo startInfo = new ProcessStartInfo
            //{
            //    FileName = exePath,
            //    WorkingDirectory = installPath,
            //    Arguments = " ",
            //    ArgumentList = { " ", "" },
            //    Verb = "runas"
            //};
            //Process.Start(exePath);

        }
        public void OptionBackup(string programFolder, string versionPath)
        {
            // 설정 파일 이름
            const string file = "ProgramSettings.xml";

            // 설치된 프로그램 경로 가져오기
            string installPath = GetInstallPath(programFolder, versionPath);

            // 원본 설정 파일 경로
            string sourceFilePath = Path.Combine(installPath, file);

            //  백업 폴더는 ProgramA 폴더 안에 위치해야 함
            string programRootPath = Path.GetDirectoryName(installPath); // ProgramA 폴더 경로
            string backupFolder = Path.Combine(programRootPath, "백업폴더");
            Directory.CreateDirectory(backupFolder); // 폴더가 없으면 생성

            // 버전명을 포함한 백업 파일명 생성
            string versionFolderName = Path.GetFileName(versionPath); // 예: V1.0
            string backupFileName = $"ProgramSettings_{versionFolderName}.xml";
            string backupFilePath = Path.Combine(backupFolder, backupFileName);
            if (!File.Exists(sourceFilePath))
            {
                MessageBox.Show("백업할 설정 파일이 없습니다.");
                return;
            }
            // 파일 복사
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream destinationStream = new FileStream(backupFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = new byte[32768];
                int bytesRead;
                while ((bytesRead = sourceStream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    destinationStream.Write(bytes, 0, bytesRead);
                }
            }

            MessageBox.Show($"백업 완료: {backupFilePath}");
        }


        public void Log(string str)
        {
            //// 현재 위치 경로 
            //string currentDirectoryPath = Environment.CurrentDirectory.ToString();

            //string DirPath = System.IO.Path.Combine(currentDirectoryPath, "Logs");

            //string FilePath = DirPath + @"\Log_" + DateTime.Today.ToString("yyyy-MM-dd") + ".txt";

            //if (!Directory.Exists(DirPath))
            //{
            //    Directory.CreateDirectory(DirPath);
            //}
            //string error = DateTime.Now.ToString() + " : " + str + Environment.NewLine;
            //File.AppendAllText(FilePath, error);


        }
        public string GetInstallPath(string programFolder, string versionPath)
        // 경로만들기 
        {
            string versionFolderName = Path.GetFileName(versionPath);
            string programFolderName = Path.GetFileName(programFolder);

            return Path.Combine(installedPath, programFolderName, versionFolderName);
        }

        public bool IsProgramInstalled(string programFolder, string versionPath)
        //설치확인
        {
            string installPath = GetInstallPath(programFolder, versionPath);

            return Directory.Exists(installPath);
        }


        public async void deleteDirectory(string InstalledDir)
        {

            //Task task = new Task(() => deleteDirectory(InstalledDir));
            try
            {
                if (string.IsNullOrEmpty(InstalledDir) || !Directory.Exists(InstalledDir))
                {
                    MessageBox.Show("삭제할 폴더가 존재하지 않습니다.");
                    return;
                }
                foreach (string file in Directory.GetFiles(InstalledDir))
                {
                    File.Delete(file);
                }
                foreach (string dir in Directory.GetDirectories(InstalledDir))
                {
                    deleteDirectory(dir);
                }
                Directory.Delete(InstalledDir);
            }
            catch (Exception e)
            {
                MessageBox.Show("삭제 실패");
                //Log(e.Message);
            }
        }
        public async Task ProgramSetup(IProgress<int> progress, string sourceDir, string destinationDir, Action<bool> SetProgressBarVisibility)
        // 설치
        {
            SetProgressBarVisibility(true);

            //  모든 하위 폴더 포함하여 파일 개수 계산
            var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            int total = files.Length; // 전체 파일 개수
            int current = 0; // 현재 복사된 파일 개수

            //  대상 폴더 생성 (없으면 생성)
            Directory.CreateDirectory(destinationDir);

            //  모든 파일 복사 + 진행률 업데이트
            foreach (string file in files)
            {
                string relativePath = file.Substring(sourceDir.Length + 1); // 상대 경로 가져오기
                string destFile = Path.Combine(destinationDir, relativePath);
                string destFolder = Path.GetDirectoryName(destFile)!;

                Directory.CreateDirectory(destFolder); // 파일이 속한 폴더가 없으면 생성

                using (FileStream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream destinationStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await Task.Delay(1000);
                    try
                    {
                        byte[] bytes = new byte[32768];
                        int bytesRead;

                        while ((bytesRead = await sourceStream.ReadAsync(bytes, 0, bytes.Length)) > 0)
                        {
                            await destinationStream.WriteAsync(bytes, 0, bytesRead);
                        }

                        //  진행률 업데이트
                        progress.Report((int)(++current / (double)total * 100));

                    }
                    catch (IOException)
                    {
                        MessageBox.Show($"복사 실패: {file}");
                    }


                }
            }
            SetProgressBarVisibility(false);
            MessageBox.Show("설치완료");
            //progress.Report(100);
        }


        public async Task InstallProgram(IProgress<int> progress, string sourcePath, string programFolder, Action<bool> updateVisibility)
        {
            string fullInstallPath = GetInstallPath(programFolder, sourcePath);
            await ProgramSetup(progress, sourcePath, fullInstallPath, updateVisibility);
        }



    }
}
