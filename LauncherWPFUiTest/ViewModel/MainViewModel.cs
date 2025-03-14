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

namespace LauncherWPFUiTest.ViewModel
{
    public class MainViewModel
    {
        private string XmlFilePath = @"C:\Users\kimgu\source\repos\LauncherWPFUiTest\LauncherWPFUiTest\XMLFile1.xml";
        public ObservableCollection<Program> Programs { get; set; } = new ObservableCollection<Program>();

        public ICommand LoadFilesCommand { get; }

        public MainViewModel()
        {
            LoadFilesCommand = new RelayCommand(_ => LoadPrograms());

        }

        private void LoadPrograms()
        {
            if (!File.Exists(XmlFilePath))
                return;

            XDocument xmlDoc = XDocument.Load(XmlFilePath);
            //파일 읽기
            var programs = xmlDoc.Descendants("Program").Select(p => new
            {
                Name = p.Element("Name")?.Value,
                Path = p.Element("Path")?.Value
            }).Where(p => !string.IsNullOrEmpty(p.Path) && Directory.Exists(p.Path));

            foreach (var program in programs)
            {
                var files = Directory.GetFiles(program.Path, "*.png", SearchOption.TopDirectoryOnly).Select(file => new Program
                {
                    ProgramName = program.Name,
                    FilePath = file,
                    FileName = file
                });

                foreach (var file in files)
                {
                    Programs.Add(file);

                }

            }
        }

    }
}
