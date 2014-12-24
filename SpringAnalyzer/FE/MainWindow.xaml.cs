using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SpringAnalyzer.DataModels;
using SpringAnalyzer.ViewModels;

namespace SpringAnalyzer.FE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataModel DataModel { get; private set; }
        public SingleFileViewModel SingleFileVM { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DataModel = new DataModel();

            var detailsView = new ObjectDetailsView();

            SingleFileVM = new SingleFileViewModel( DataModel );
            DataContext = SingleFileVM;
            detailsView.DataContext = SingleFileVM;
            detailsView.Show();

            LoadFile( @"D:\SSME_INT\deploy\config\H\Exam\ProtocolDesigner\SpringConfig.xml" );
        }

        private void LoadFile( string path )
        {
            DataModel.Clear();
            try
            {
                DataModel.LoadConfigFile( path, true );
            }
            catch( FileNotFoundException ) {}
            catch( DirectoryNotFoundException ) {}
            SingleFileVM.UpdateDefinitions( DataModel );
        }

        private void BrowseForFile( object sender, RoutedEventArgs e )
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files| (*.xml)";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if( result == true )
            {
                // Open document 
                string filename = dlg.FileName;
                filePathTextBox.Text = filename;
            }
        }

        private void Load_Clicked( object sender, RoutedEventArgs e )
        {
            string sprdFilePath = filePathTextBox.Text;
            LoadFile( sprdFilePath );
        }

        private void FileImports_OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            ListView listView = sender as ListView;
            var filePaths = listView.SelectedItems as IList<object>;

            var files = new List<string>();
            files.AddRange( filePaths.Select( x => x as string ) );

            SingleFileVM.FilterObjectDefinitions( files );
        }
    }
}