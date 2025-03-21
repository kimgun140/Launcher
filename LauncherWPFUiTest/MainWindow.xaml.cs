using LauncherWPFUiTest.Model;
using LauncherWPFUiTest.ViewModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LauncherWPFUiTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileCopyManager fileCopyManager = new FileCopyManager();

        public MainWindow()
        {
            InitializeComponent();
            //DataContext = new ItemViewModel();
            DataContext = new MainViewModel();

            fileCopyManager.Connection();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("준비중");
        }
    }
}