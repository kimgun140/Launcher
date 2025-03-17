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

namespace LauncherWPFUiTest.ViewModel
{
    public class MainViewModel
    {
        private string XmlFilePath = @"C:\Users\kimgu\source\repos\LauncherWPFUiTest\LauncherWPFUiTest\ProgramsPath.xml";
        public ObservableCollection<Program> Programs { get; set; } = new ObservableCollection<Program>();

        private Program _selectedProgram;
        public Program SelectedProgram
        {
            get => _selectedProgram;
            set
            {
                _selectedProgram = value;
                SelectedVersion = null;
            }
        }
        private VersionInfo _selectedVersion;
        public VersionInfo SelectedVersion
        {
            get => _selectedVersion;
            set => _selectedVersion = value;
        }
        public ICommand LaunchProgramCommand { get; }


        public ICommand LoadFilesCommand { get; }
        public ICommand SelectProgramCommand { get; }

        public MainViewModel()
        {
            LoadFilesCommand = new RelayCommand(_ => LoadPrograms());
            LaunchProgramCommand = new RelayCommand(_ => LaunchSelectedVersion(), _ => SelectedVersion != null);
            SelectProgramCommand = new RelayCommand(param => SetSelectedProgram(param));

        }

        private void LoadPrograms()
        {

            if (!File.Exists(XmlFilePath))
                return;

            Programs.Clear();

            XDocument xmlDoc = XDocument.Load(XmlFilePath);
            var programNodes = xmlDoc.Descendants("Program");




            // 1단계: 런처의 XML 파일을 읽어 프로그램 목록 폴더 경로를 가져오기
            XDocument launcherXml = XDocument.Load(XmlFilePath);
            string programsFolder = launcherXml.Element("Launcher")?.Element("ProgramsFolder")?.Value;

            if (string.IsNullOrEmpty(programsFolder) || !Directory.Exists(programsFolder))
                return;

            //  2단계: 프로그램 폴더 목록 가져오기 폴더바로읽기 이거는 
            var programFolders = Directory.GetDirectories(programsFolder);

            foreach (var programFolder in programFolders)
            {
                string programXmlPath = Path.Combine(programFolder, "program.xml");

                if (!File.Exists(programXmlPath))
                    continue;

                // 3단계: 프로그램 폴더 안의 XML 파일 읽기
                XDocument programXml = XDocument.Load(programXmlPath);
                string programName = programXml.Element("Program")?.Element("Name")?.Value;
                string iconFileName = programXml.Element("Program")?.Element("Icon")?.Value;

                if (string.IsNullOrEmpty(programName) || string.IsNullOrEmpty(iconFileName))
                    continue;

                string iconPath = Path.Combine(programFolder, iconFileName);

                var program = new Program
                {
                    ProgramName = programName,
                    FolderPath = programFolder,
                    IconPath = File.Exists(iconPath) ? iconPath : null
                };
                var versionNodes = programXml.Descendants("Version");
                foreach (var  versionNode in versionNodes)
                {
                    string versionNumber = versionNode.Element("Number")?.Value;
                    string versionPath = versionNode.Element("Path")?.Value;

                    if (string.IsNullOrEmpty(versionNumber) || string.IsNullOrEmpty(versionPath))
                        continue;

                    program.Versions.Add(new VersionInfo
                    {
                        Number = versionNumber,
                       Path = versionPath
                    });
                }

                Programs.Add(program);

            }
        }

        private void LaunchSelectedVersion()
        {

            if (SelectedVersion == null)
                return;
            Process.Start(new ProcessStartInfo
            {
                FileName = SelectedVersion.Path,
                UseShellExecute = true
            });

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
