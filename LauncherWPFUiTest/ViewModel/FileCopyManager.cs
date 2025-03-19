using LauncherWPFUiTest.Model;
using System;
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

        private string XmlFilePath = @"\\Gms-mcc-nas01\audio-file\test1\ProgramsPath.xml";
        //프로그램들 보관경로 

        //설치 루트폴더 경로 
        private string installedPath = @"C:\Program Files\launcherfolder";
        LauncherConfig launcherConfig;



        public ObservableCollection<Program> LoadPrograms()
        {

            ObservableCollection<Program> programs = new ObservableCollection<Program>();

            if (!File.Exists(XmlFilePath))
                return new ObservableCollection<Program>();

            programs.Clear();

            //  런처 XML을 역직렬화하여 프로그램 폴더 경로 가져오기
            XmlSerializer launcherSerializer = new XmlSerializer(typeof(LauncherConfig));
            //LauncherConfig launcherConfig;

            using (StreamReader reader = new StreamReader(XmlFilePath))
            {
                launcherConfig = (LauncherConfig)launcherSerializer.Deserialize(reader);
            }

            if (string.IsNullOrEmpty(launcherConfig.ProgramsFolder) || !Directory.Exists(launcherConfig.ProgramsFolder))
                return new ObservableCollection<Program>();

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
                //이걸 다 옮겨야한다는거지 서비스로 



            }
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

            if (result == 0)// 0이면 연결 
            {

                MessageBox.Show("연결");

                //WNetCancelConnection2(@"\\gms-mcc-nas01\AUDIO-FILE\test1", 0, true);
                //연결해제 lpRemoteName 을 넣어주기  
                //windows 네트워크 드라이브는 네트워크 공유 폴더 단위로 관리되므로, 연결을 해제할때 해당 공유폴더의 경로를 지정한다.
            }
            else
            {

                MessageBox.Show(result.ToString());
            }
        }
        public void OptionBackup()
        {


        }
        public void Log(string str)
        {
            // 현재 위치 경로 
            string currentDirectoryPath = Environment.CurrentDirectory.ToString();

            string DirPath = System.IO.Path.Combine(currentDirectoryPath, "Logs");

            string FilePath = DirPath + @"\Log_" + DateTime.Today.ToString("yyyy-MM-dd") + ".txt";

            if (!Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }
            string error = DateTime.Now.ToString() + " : " + str + Environment.NewLine;
            File.AppendAllText(FilePath, error);


        }


        public void CopyProgramFiles(string[] Paths)
        // 원본 폴더 경로 
        {

            string sourcePath = Paths[0];
            string destinationDir = Paths[1];

            if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
            {
                //원본이 존재하지않을때 
                MessageBox.Show("원본이 존재하지 않습니다.");
                return;

            }
            //sourcePath 폴더의 파일들을 복사해서 basicPath에 붙여넣기
            // 폴더 경로 


            //string basicPath = @"C:\Users\kimgu\OneDrive\바탕 화면\LauncherPrograms\v1";

            //설치할경로에 폴더이름 
            //string destinationDir = @"C:\Users\kimgu\OneDrive\바탕 화면\LauncherPrograms\v1";


            CopyDirectory(sourcePath, destinationDir);


        }


        private static async void CopyDirectory(string sourceDir, string destinationDir)
        {

            Directory.CreateDirectory(destinationDir);
            //폴더 만들기 폴더가 있어서

            foreach (string file in Directory.GetFiles(sourceDir))
            // files가 몇개인지를 전달하면 되겠다. 
            //플더안 파일 이름 가져오기 
            {
                string dest = Path.Combine(destinationDir, Path.GetFileName(file));

                using (FileStream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream destinationStream = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    try
                    {
                        byte[] bytes = new byte[32768];
                        int bytesRead;

                        while ((bytesRead = await sourceStream.ReadAsync(bytes, 0, bytes.Length)) > 0)
                        {
                            await destinationStream.WriteAsync(bytes, 0, bytesRead);
                        }
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("복사 실패");
                    }
                }
            }

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }
        public async void deleteDirectory(string InstalledDir)
        {

            //Task task = new Task(() => deleteDirectory(InstalledDir));

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
        //public async Task ProgressUpdate(IProgress<int> progress, string sourceDir, string destinationDir)
        //{
        //    // 1️⃣ 모든 하위 폴더 포함하여 파일 개수 계산
        //    var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
        //    int total = files.Length; // 전체 파일 개수
        //    int current = 0; // 현재 복사된 파일 개수

        //    // 2️⃣ 대상 폴더 생성 (없으면 생성)
        //    Directory.CreateDirectory(destinationDir);

        //    // 3️⃣ 모든 파일 복사 + 진행률 업데이트
        //    foreach (string file in files)
        //    {
        //        string relativePath = file.Substring(sourceDir.Length + 1); // 상대 경로 가져오기
        //        string destFile = Path.Combine(destinationDir, relativePath);
        //        string destFolder = Path.GetDirectoryName(destFile)!;

        //        Directory.CreateDirectory(destFolder); // 파일이 속한 폴더가 없으면 생성

        //        using (FileStream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        using (FileStream destinationStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None))
        //        {
        //            await Task.Delay(1000);
        //            try
        //            {
        //                byte[] bytes = new byte[32768];
        //                int bytesRead;

        //                while ((bytesRead = await sourceStream.ReadAsync(bytes, 0, bytes.Length)) > 0)
        //                {
        //                    await destinationStream.WriteAsync(bytes, 0, bytesRead);
        //                }

        //                // 4️⃣ 진행률 업데이트
        //                progress.Report((int)(++current / (double)total * 100));
        //            }
        //            catch (IOException)
        //            {
        //                MessageBox.Show($"복사 실패: {file}");
        //            }
        //        }
        //    }
        //}

        public async Task ProgressUpdate(IProgress<int> progress, string sourceDir, string destinationDir)
        {

            //progress.Report(File.count() / 100);
            var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            // dl
            int total = files.Length;
            int current = 0;
            //원본 파일 갯수

            Directory.CreateDirectory(destinationDir);
            //폴더 만들기 폴더가 있어서

            foreach (string file in Directory.GetFiles(sourceDir))
            //플더안 파일 이름 가져오기 
            {
                string dest = Path.Combine(destinationDir, Path.GetFileName(file));

                using (FileStream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream destinationStream = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None))
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
                        progress.Report((int)(++current / (double)total * 100));

                    }
                    catch (IOException)
                    {
                        MessageBox.Show("복사 실패");
                    }
                }
            }

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
                await ProgressUpdate(progress, dir, destDir);
            }


        }

    }
}
