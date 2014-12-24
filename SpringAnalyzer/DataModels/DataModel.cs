using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace SpringAnalyzer.DataModels
{
    public class DataModel
    {
        public DataModel()
        {
            ObjectDefinitions = new Dictionary<string, ObjectDefinition>();
            ObjectInstances = new Dictionary<string, ObjectDefinition>();
            file_imports = new List<string>();
        }

        public void Clear()
        {
            ObjectDefinitions.Clear();
            ObjectInstances.Clear();
            file_imports.Clear();
        }

        public IDictionary<string, ObjectDefinition> ObjectDefinitions { get; private set; }
        public IDictionary<string, ObjectDefinition> ObjectInstances { get; private set; }
        public IList<string> file_imports { get; private set; }

        //parse the content of a spring configuration file
        //the imports are also parsed in their inclusion order
        //object definitions are recorded in dictionary ObjectDefinitions
        public void LoadConfigFile( string filePath, bool isRootConfigFile )
        {
            if( file_imports.Contains( filePath ) )
            {
                return;
            }
            file_imports.Add( filePath );

            string currentDir = Directory.GetCurrentDirectory();
            string fileContainingDir = Path.GetDirectoryName( filePath );

            Directory.SetCurrentDirectory( fileContainingDir );
            var elements = SpringConfigReader.ReadSpringConfigFile( filePath );
            var imports = SpringConfigReader.GetImportFiles( elements );
            Directory.SetCurrentDirectory( currentDir );

            foreach( var import in imports )
            {
                LoadConfigFile( import, false );
            }

            BuildObjectDefinitions( elements, filePath );
            if( isRootConfigFile ) //only do the matching after all the files finishes loading
            {
                foreach( var objectDefinition in ObjectDefinitions.Values )
                {
                    MatchReferences( objectDefinition );
                    FindDuplicates( objectDefinition );
                }
            }
        }

        public void BuildObjectDefinitions( IEnumerable<XElement> elements, string containingFile )
        {
            var objects = SpringConfigReader.GetObjectXElements( elements );
            foreach( var xElement in objects )
            {
                if( xElement is XElement )
                {
                    var objDef = new ObjectDefinition( xElement, containingFile );
                    objDef.containing_file = containingFile;

                    string key = ExtractKey( objDef, containingFile );
                    if( !ObjectDefinitions.ContainsKey( key ) )
                    {
                        ObjectDefinitions.Add( key, objDef );
                        Trace.TraceInformation( "object definition {0} added", key );
                    }
                    else
                    {
                        Trace.TraceInformation( "object definition {0} duplicated", key );
                    }
                }
            }
        }

        public void MatchReferences( ObjectDefinition objectDefinition )
        {
            objectDefinition.matching_references.Clear();
            objectDefinition.missing_references.Clear();
            foreach( var referenceId in objectDefinition.direct_references )
            {
                var referencedObj = TryGetObjectDefinition( referenceId, objectDefinition.containing_file );
                if( referencedObj != null )
                {
                    objectDefinition.matching_references.Add( referencedObj );
                    referencedObj.matching_referenced_by.Add( objectDefinition );
                }
                else
                {
                    objectDefinition.missing_references.Add( referenceId );
                }
            }
        }

        public void FindDuplicates( ObjectDefinition objectDefinition )
        {
            objectDefinition.Duplicates.Clear();
            foreach( var obj in ObjectDefinitions.Values )
            {
                if( objectDefinition.IsDuplicate( obj ) && !objectDefinition.Duplicates.Contains( obj ) )
                {
                    objectDefinition.Duplicates.Add( obj );
                    obj.Duplicates.Add( objectDefinition );
                }
            }
        }

        public ObjectDefinition TryGetObjectDefinition( string id, string contain_file )
        {
            string key = MakeKey( id, contain_file );
            if( ObjectDefinitions.ContainsKey( key ) )
            {
                return ObjectDefinitions[ key ];
            }
            foreach( var objectDefinition in ObjectDefinitions )
            {
                if( objectDefinition.Value.id.Equals( id ) )
                {
                    return objectDefinition.Value;
                }
            }

            return null;
        }

        private string ExtractKey( ObjectDefinition definition, string containingFile )
        {
            return MakeKey( definition.id, containingFile );
        }

        private string MakeKey( string id, string containing_file )
        {
            return id + containing_file;
        }
    }
}