using System.Collections;
using System.Windows;
using System.Windows.Controls;

using SpringAnalyzer.DataModels;
using SpringAnalyzer.ViewModels;

namespace SpringAnalyzer.FE
{
    /// <summary>
    /// Interaction logic for ObjectListView.xaml
    /// </summary>
    public partial class ObjectListControl : UserControl
    {
        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource",
                                                                                            typeof( IEnumerable ),
                                                                                            typeof( ObjectListControl ) );

        public IEnumerable ItemsSource
        {
            get
            {
                return GetValue( ItemsSourceProperty ) as IEnumerable;
            }
            set
            {
                SetValue( ItemsSourceProperty, value );
            }
        }

        public ObjectListControl()
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
                vm.CurrentObject = item;
            }
        }
    }
}