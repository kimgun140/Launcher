using LauncherWPFUiTest.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LauncherWPFUiTest
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            fileCopyManager.Connection();
        }
        FileCopyManager fileCopyManager = new FileCopyManager();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 이버튼에 바인딩하면 되는거고 

            //FileCopyManager.CopyProgramFiles(@"C:\Users\kimgu\OneDrive\바탕 화면\프로그램들\ProgramA\V1.0");
            string uncpath = @"\\gms-mcc-nas01\AUDIO-FILE\test1";
            string[] strings = new string[2];
            strings[0] = uncpath;
            strings[1] = "설치할 위치";
            //어디서 받지 
            fileCopyManager.CopyProgramFiles(strings);


            // CopyProgramFiles(원본경로, 복사할경로 ) 


        }
    }
}
