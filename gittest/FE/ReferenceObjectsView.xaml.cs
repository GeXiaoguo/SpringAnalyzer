using System.Windows;
using System.Windows.Controls;

using XmlDataTree.DataModels;

namespace XmlDataTree
{
    /// <summary>
    /// Interaction logic for ReferenceObjectsView.xaml
    /// </summary>
    public partial class ReferenceObjectsView : Window
    {
        public ReferenceObjectsView()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            ListView lstView = sender as ListView;
            var item = lstView.SelectedItem as ObjectDefinition;
            if( item != null )
            {
                var vm = DataContext as SingleFileViewModel;
                vm.UpdateReferences( item );
            }
        }
    }
}