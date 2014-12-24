using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using SpringAnalyzer.DataModels;

namespace SpringAnalyzer.ViewModels
{
    public class SingleFileViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ObjectDefinition> Definitions { get; set; }

        private ObjectDefinition _currObjectDefinition;

        public ObjectDefinition CurrentObject
        {
            get
            {
                return _currObjectDefinition;
            }
            set
            {
                _currObjectDefinition = value;
                RaisePropertyChanged( "CurrentObject" );
            }
        }

        public ObservableCollection<string> FileImports { get; set; }

        public ObservableCollection<ObjectDefinition> RootObjects { get; set; }

        public ObservableCollection<ObjectDefinition> LeafObjects { get; set; }

        private DataModel myDataModel;

        public SingleFileViewModel( DataModel dataModel )
        {
            myDataModel = dataModel;

            //            InitDataTable();
            Definitions = new ObservableCollection<ObjectDefinition>();
            Definitions.CollectionChanged += Definitions_CollectionChanged;
            FileImports = new ObservableCollection<string>();
            RootObjects = new ObservableCollection<ObjectDefinition>();
            LeafObjects = new ObservableCollection<ObjectDefinition>();
            UpdateDefinitions( myDataModel );
        }

        private void Definitions_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            RootObjects.Clear();
            var rootObjs = Definitions.Where( x => x.matching_referenced_by.Count == 0 ).Select( x => x );
            Copy2Collection( rootObjs, RootObjects );

            LeafObjects.Clear();
            var leafObjs = Definitions.Where( x => x.direct_references.Count == 0 ).Select( x => x );
            Copy2Collection( leafObjs, LeafObjects );
        }

        public void UpdateDefinitions( DataModel model )
        {
            Definitions.Clear();
            Copy2Collection( model.ObjectDefinitions.Values, Definitions );
            Copy2Collection( model.file_imports, FileImports );
        }

        public void FilterObjectDefinitions( IList<string> filePaths )
        {
            if( filePaths != null )
            {
                Definitions.Clear();
                var objs = myDataModel.ObjectDefinitions.Where( x => filePaths.Contains( x.Value.containing_file ) ).Select( x => x.Value );
                Copy2Collection( objs, Definitions );
            }
        }

        private static void Copy2Collection<T>( IEnumerable<T> source, ICollection<T> target )
        {
            target.Clear();
            foreach( var obj in source )
            {
                target.Add( obj );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged( string propertyName )
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
    }
}