using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpringAnalyzer.DataModels
{
    internal static class SpringConfigReader
    {
        public static IEnumerable<XElement> ReadSpringConfigFile( string configFilePath )
        {
            XDocument doc = XDocument.Load( configFilePath );
            var elements = doc.Descendants();
            return elements;
        }

        public static IEnumerable<string> GetImportFiles( IEnumerable<XElement> elements )
        {
            var importFiles = new List<string>();
            var imports = GetImportXElements( elements );
            foreach( var element in imports )
            {
                string file = element.Attribute( "resource" ).Value;
                var fullPath = Path.GetFullPath( file );
                importFiles.Add( fullPath );
            }
            return importFiles;
        }

        public static IEnumerable<XElement> GetObjectXElements( IEnumerable<XElement> elements )
        {
            var objects = from element in elements
                          where element.Name.LocalName.Equals( "object" )
                          select element;
            return objects;
        }

        public static IEnumerable<XElement> GetReferenceXElements( XElement obj )
        {
            var allElements = GetChildXElements( obj );

            return from element in allElements
                   where element.Attribute( "ref" ) != null || element.Name.LocalName.Equals( "ref" )
                   select element;
        }

        private static List<XElement> GetChildXElements( XElement element )
        {
            List<XElement> allElements = new List<XElement>();

            var childElements = element.Nodes().OfType<XElement>().ToList();

            foreach( var childElement in childElements )
            {
                var elements = GetChildXElements( childElement );
                allElements.AddRange( elements );
            }

            allElements.AddRange( childElements );
            return allElements;
        }

        private static IEnumerable<XElement> GetImportXElements(IEnumerable<XElement> elements)
        {
            var imports = from element in elements
                          where element.Name.LocalName.Equals("import")
                          select element;
            return imports;
        }
    }
}