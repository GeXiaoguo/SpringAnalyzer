using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace SpringAnalyzer.DataModels
{
    public class ObjectDefinition
    {
        public string id { get; set; }
        public string error_msg { get; set; }

        public bool IsValid
        {
            get
            {
                error_msg = "";

                if( Duplicates.Count > 0 )
                {
                    error_msg = "Duplicated with other objects. ";
                }
                if( missing_references.Count > 0 )
                {
                    error_msg += "Contains missing references. ";
                }
                if( direct_references.Count == 0 && matching_referenced_by.Count == 0 )
                {
                    error_msg += "orphan Object. ";
                }
                if( string.Empty.Equals( error_msg ) )
                {
                    return true;
                }
                return false;
            }
        }

        public string type { get; set; }
        public string lazy_init { get; set; }
        public string factory { get; set; }
        public string factory_method { get; set; }

        public string containing_file { get; set; }
        public IList<string> direct_references { get; set; }
        public IList<string> missing_references { get; set; }
        public IList<ObjectDefinition> matching_references { get; set; }
        public IList<ObjectDefinition> matching_referenced_by { get; set; }
        public IList<ObjectDefinition> Duplicates { get; set; }

        public ObjectDefinition( XElement element, string containingFile )
        {
            containing_file = containingFile;
            if( element.Attribute( "id" ) != null )
            {
                id = element.Attribute( "id" ).Value;
            }
            else
            {
                id = "ananymous@" + containing_file;
            }

            if( element.Attribute( "factory-object" ) != null )
            {
                factory = element.Attribute( "factory-object" ).Value;
                factory_method = element.Attribute( "factory-method" ).Value;
            }
            if( element.Attribute( "type" ) != null )
            {
                type = element.Attribute( "type" ).Value;
            }
            else
            {
                type = factory_method + "@" + factory;
            }

            if( element.Attribute( "lazy-init" ) != null )
            {
                lazy_init = element.Attribute( "lazy-init" ).Value;
            }
            direct_references = new List<string>();
            missing_references = new List<string>();

            matching_references = new List<ObjectDefinition>();
            matching_referenced_by = new List<ObjectDefinition>();
            Duplicates = new List<ObjectDefinition>();
            BuidReferences( element );
        }

        private static void ExtractReferences( ObjectDefinition obj, IList<ObjectDefinition> references )
        {
            foreach( var objectDefinition in obj.matching_references )
            {
                if( !references.Contains( objectDefinition ) )
                {
                    references.Add( objectDefinition );
                    ExtractReferences( objectDefinition, references );
                }
                else
                {
                    Trace.TraceError( "Circular references found in references: {0}@{1} : {2}@{3}",
                                      obj.id,
                                      obj.containing_file,
                                      objectDefinition.id,
                                      objectDefinition.containing_file );
                }
            }
        }

        private static void ExtractReferenceBys( ObjectDefinition obj, IList<ObjectDefinition> references )
        {
            foreach( var objectDefinition in obj.matching_referenced_by )
            {
                if( !references.Contains( objectDefinition ) )
                {
                    references.Add( objectDefinition );
                    ExtractReferenceBys( objectDefinition, references );
                }
                else
                {
                    Trace.TraceError( "Circular references found in ReferenceBys: {0}@{1} : {2}@{3}",
                                      obj.id,
                                      obj.containing_file,
                                      objectDefinition.id,
                                      objectDefinition.containing_file );
                }
            }
        }

        public IList<ObjectDefinition> indirect_references
        {
            get
            {
                List<ObjectDefinition> refs = new List<ObjectDefinition>();
                ExtractReferences( this, refs );
                return refs;
            }
        }

        public IList<ObjectDefinition> indirect_referenced_by
        {
            get
            {
                List<ObjectDefinition> refs = new List<ObjectDefinition>();
                ExtractReferenceBys( this, refs );
                return refs;
            }
        }

        public IList<ObjectDefinition> inderect_matching_references { get; set; }

        private void BuidReferences( XElement element )
        {
            direct_references.Clear();
            var refs = SpringConfigReader.GetReferenceXElements( element );
            foreach( var refElement in refs )
            {
                if( refElement.Attribute( "ref" ) != null )
                {
                    var refId = refElement.Attribute( "ref" ).Value;
                    direct_references.Add( refId );
                }
                else if( refElement.Name.LocalName.Equals( "ref" ) )
                {
                    var refID = refElement.Attribute( "object" ).Value;
                    direct_references.Add( refID );
                }
                else
                {
                    Trace.TraceError( "error when extracting reference object IDs" );
                }
            }
            if( !string.IsNullOrEmpty( factory ) )
            {
                direct_references.Add( factory );
            }
        }

        public bool IsDuplicate( ObjectDefinition obj )
        {
            if( obj == this )
            {
                return false;
            }

            if( !string.Equals( obj.type, type ) )
            {
                return false;
            }

            foreach( var reference in matching_references )
            {
                if( !obj.matching_references.Contains( reference ) )
                {
                    return false;
                }
            }
            foreach( var reference in obj.matching_references )
            {
                if( !matching_references.Contains( reference ) )
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals( object obj )
        {
            if( obj == null || obj.GetType() != GetType() )
            {
                return false;
            }
            var that = obj as ObjectDefinition;

            return string.Equals( id, that.id )
                   && string.Equals( type, that.type )
                   && string.Equals( containing_file, that.containing_file );
        }

        public override int GetHashCode()
        {
            return id.GetHashCode() ^ containing_file.GetHashCode();
        }
    }
}