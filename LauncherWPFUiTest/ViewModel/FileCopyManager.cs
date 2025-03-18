using LauncherWPFUiTest.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        private string XmlFilePath = @"C:\Users\kimgu\source\repos\LauncherWPFUiTest\LauncherWPFUiTest\ProgramsPath.xml";
        //런처 설치위치 

        //ProgramsPath 런처 설정도 포함

        //public void 


        public void Connection()
        {

            //unc경로, 아이디 , 비밀번호 읽기 
            XmlSerializer launcherSerializer = new XmlSerializer(typeof(LauncherConfig));
            LauncherConfig launcherConfig;

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



        public void CopyProgramFiles(string[] Paths)
        // 원본 폴더 경로 
        {

            string sourcePath = Paths[0];
            string destinationDir = Paths[1];

            if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
            {
                //원본이 존재하지않을때 

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
            //폴더 만들기 폴더가 있어서 이게 대체됐네 

            foreach (string file in Directory.GetFiles(sourceDir))
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
    }
}
