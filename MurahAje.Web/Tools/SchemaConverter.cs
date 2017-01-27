using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using MurahAje.Web.Entities;

namespace MurahAje.Web.Tools
{
    public class SchemaConverter
    {
        
        public static DataTable ExpandoToDataTable(List<dynamic> data)
        {
        
            int counter = 0;
            DataTable dt = new DataTable();
            //generate columns
            foreach (dynamic item in data)
            {
                if (item is ExpandoObject)
                {
                    if (counter <= 0)
                    {
                        foreach (var dataitem in (IDictionary<string, object>)item)
                        {
                            dt.Columns.Add(new DataColumn() { ColumnName = dataitem.Key, DataType = typeof(string) });
                        }
                    }

                    {
                        var NewItem = dt.NewRow();
                        foreach (var dataitem in (IDictionary<string, object>)item)
                        {
                            NewItem[dataitem.Key] = dataitem.Value;
                        }
                        dt.Rows.Add(NewItem);
                    }
                    counter++;
                }

            }
            //dt.AcceptChanges();
            return dt;
        }
        public static dynamic JsonToExpando(string JsonStr)
        {
            return JsonConvert.DeserializeObject<ExpandoObject>(JsonStr, new ExpandoObjectConverter());
        }
        public static SchemaEntity ExpandoToSchema(dynamic obj, string SchemaName)
        {
            var node = new SchemaEntity();
            node.SchemaName = SchemaName;
            node.JsonStructure = JsonConvert.SerializeObject(obj);
            XElement el = ExpandoToXML(obj, SchemaName);
            node.XmlStructure = el.ToString();
            node.Fields = GenerateFieldsFromExpando(obj);
            return node;
        }
        public static SchemaEntity JsonToSchema(string JsonStr, string SchemaName)
        {
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(JsonStr, new ExpandoObjectConverter());
            var node = new SchemaEntity();
            node.SchemaName = SchemaName;
            node.JsonStructure = JsonStr;
            XElement el = ExpandoToXML(obj, SchemaName);
            node.XmlStructure = el.ToString();
            node.Fields = GenerateFieldsFromExpando(obj);
            return node;
        }
        /*
        public static SchemaEntity CsvToSchema(string CSVPath, string SchemaName)
        {
            DataTable dt = CSVReader.OpenCSV(CSVPath);
           
            dynamic newObj = new ExpandoObject();
            foreach (DataColumn dc in dt.Columns)
            {
                (newObj as IDictionary<string, object>).Add(dc.ColumnName.Replace(" ", "_"), dt.Rows[0][dc.ColumnName]);
            }
            return ExpandoToSchema(newObj, SchemaName);
        }
        
        public static dynamic CsvToExpando(string CSVPath)
        {

            DataTable dt = CSVReader.OpenCSV(CSVPath);
          
            var datas = new List<dynamic>();
            foreach (DataRow dr in dt.Rows)
            {
                dynamic newObj = new ExpandoObject();
                foreach (DataColumn dc in dt.Columns)
                {

                    (newObj as IDictionary<string, object>).Add(dc.ColumnName.Replace(" ", "_"), dt.Rows[0][dc.ColumnName]);
                }
                datas.Add(newObj);
            }
            return datas;

        }
        */
        public static object GetSample(Type sample)
        {
            var dict = new Dictionary<Type, object> {
            { typeof(String), "sample" },
            { typeof(int), 123 },
            { typeof(double), 12.3 },
            { typeof(DateTime), DateTime.MinValue },
            { typeof(float), 12.3f },
            { typeof(decimal), 12.3 },
            { typeof(Boolean), true },
            { typeof(char), 'a' },
            { typeof(Int64), 0 },
        };

            return dict[sample];
        }
        public static SchemaEntity DesignToSchema(List<IDField> fields, string SchemaName)
        {
            var node = new SchemaEntity();
            var NewFields = new Dictionary<string, IDField>();
            dynamic newObj = new ExpandoObject();
            foreach (var item in fields)
            {
                (newObj as IDictionary<string, object>).Add(item.Name.Replace(" ", "_"), GetSample(item.NativeType));
                NewFields.Add(item.Name.Replace(" ", "_"), item);
            }

            node.SchemaName = SchemaName;
            node.JsonStructure = JsonConvert.SerializeObject(newObj);
            XElement el = ExpandoToXML(newObj, SchemaName);
            node.XmlStructure = el.ToString();
            node.Fields = NewFields;
            return node;
        }

        public static string SchemaToJson(SchemaEntity entity)
        {
            return entity.JsonStructure;
        }

        public static string SchemaToXml(SchemaEntity entity)
        {
            return entity.XmlStructure;
        }

        public static SchemaEntity XmlToSchema(string XmlStr, string SchemaName)
        {
            dynamic obj = XMLtoExpando(null, XElement.Parse(XmlStr));
            var node = new SchemaEntity();
            node.SchemaName = SchemaName;
            node.JsonStructure = JsonConvert.SerializeObject(obj);
            node.XmlStructure = XmlStr;
            node.Fields = GenerateFieldsFromExpando(obj);
            return node;
        }
        private static Dictionary<string, IDField> GenerateFieldsFromExpando(dynamic node)
        {
            Dictionary<string, IDField> root = new Dictionary<string, IDField>();

            foreach (var property in (IDictionary<String, Object>)node)
            {
                if (property.Value.GetType() == typeof(List<dynamic>))
                    foreach (var element in (List<dynamic>)property.Value)
                        root.Add(property.Key, new IDField() { Name = property.Key, NativeType = property.Value.GetType(), Children = GenerateChild(element), Desc = "", FieldType = FieldTypes.MultiField, IsMandatory = true, RegexValidation = "" });
                else
                    root.Add(property.Key, new IDField() { Name = property.Key, NativeType = property.Value.GetType(), Children = null, Desc = "", FieldType = FieldTypes.SingleField, IsMandatory = true, RegexValidation = "" });
            }

            return root;
        }
        private static List<IDField> GenerateChild(dynamic node)
        {
            List<IDField> root = new List<IDField>();

            foreach (var property in (IDictionary<String, Object>)node)
            {

                if (property.Value.GetType() == typeof(List<dynamic>))
                    foreach (var element in (List<dynamic>)property.Value)
                        root.Add(new IDField() { Name = property.Key, NativeType = property.Value.GetType(), Children = GenerateChild(element), Desc = "", FieldType = FieldTypes.MultiField, IsMandatory = true, RegexValidation = "" });
                else
                    root.Add(new IDField() { Name = property.Key, NativeType = property.Value.GetType(), Children = null, Desc = "", FieldType = FieldTypes.SingleField, IsMandatory = true, RegexValidation = "" });
            }

            return root;
        }
        public static string ExpandoToCsv(dynamic node)
        {
            bool isColumnGenerated = false;
            StringBuilder sb = new StringBuilder();
            string ColNames = string.Empty;
            if (node is List<dynamic>)
            {
                foreach (var element in (List<dynamic>)node)
                {
                    if (element is ExpandoObject)
                    {
                        var counter = 0;
                        foreach (var property in (IDictionary<String, Object>)element)
                        {
                            if (!isColumnGenerated)
                            {
                                if (counter > 0) ColNames += ";";
                                ColNames += property.Key.Replace(" ", "_");
                            }
                            if (counter > 0)
                                sb.Append(";");
                            sb.Append(property.Value);
                            counter++;
                        }

                        sb.Append(Environment.NewLine);
                        isColumnGenerated = true;
                    }
                }
                if (!string.IsNullOrEmpty(ColNames))
                {
                    sb.Insert(0, ColNames + Environment.NewLine);
                }
            }
            else
            {
                if (node is ExpandoObject)
                {
                    var counter = 0;
                    foreach (var property in (IDictionary<String, Object>)node)
                    {
                        if (!isColumnGenerated)
                        {
                            if (counter > 0) ColNames += ";";
                            ColNames += property.Key.Replace(" ", "_");
                        }
                        if (counter > 0)
                            sb.Append(";");
                        sb.Append(property.Value);
                        counter++;
                    }

                    sb.Append(Environment.NewLine);
                    isColumnGenerated = true;
                    if (!string.IsNullOrEmpty(ColNames))
                    {
                        sb.Insert(0, ColNames + Environment.NewLine);
                    }
                }
            }

            return sb.ToString();
        }
        public static XElement ExpandoToXML(dynamic node, String nodeName)
        {
            XElement xmlNode = new XElement(nodeName);
            if (node is List<dynamic> || node is List<Dictionary<string, object>>)
            {
                if (node is List<dynamic>)
                {
                    foreach (var element in (List<dynamic>)node)
                        xmlNode.Add(ExpandoToXML(element as ExpandoObject, "row"));
                }
                else
                {
                    foreach (var element in (List<Dictionary<string, object>>)node)
                        xmlNode.Add(ExpandoToXML(element, "row"));
                }
            }
            else
            {
                foreach (var property in (IDictionary<String, Object>)node)
                {

                    if (property.Value.GetType() == typeof(ExpandoObject))
                        xmlNode.Add(ExpandoToXML(property.Value, property.Key));

                    else
                        if (property.Value.GetType() == typeof(List<dynamic>))
                        foreach (var element in (List<dynamic>)property.Value)
                            xmlNode.Add(ExpandoToXML(element, property.Key));
                    else
                        xmlNode.Add(new XElement(property.Key, property.Value));
                }
            }
            return xmlNode;
        }
        public static dynamic XMLtoExpando(String file, XElement node = null)
        {
            if (String.IsNullOrWhiteSpace(file) && node == null) return null;

            // If a file is not empty then load the xml and overwrite node with the
            // root element of the loaded document
            node = !String.IsNullOrWhiteSpace(file) ? XDocument.Load(file).Root : node;

            IDictionary<String, dynamic> result = new ExpandoObject();

          

            // use parallel as we dont really care of the order of our properties
            node.Elements().AsParallel().ForAll(gn =>
            {
                // Determine if node is a collection container
                var isCollection = gn.HasElements &&
                    (
                        // if multiple child elements and all the node names are the same
                        gn.Elements().Count() > 1 &&
                        gn.Elements().All(
                            e => e.Name.LocalName.ToLower() == gn.Elements().First().Name.LocalName) ||

                        // if there's only one child element then determine using the PluralizationService if
                        // the pluralization of the child elements name matches the parent node. 
                        gn.Name.LocalName.ToLower() ==  gn.Elements().First().Name.LocalName.ToLower()
                    );

                // If the current node is a container node then we want to skip adding
                // the container node itself, but instead we load the children elements
                // of the current node. If the current node has child elements then load
                // those child elements recursively
                var items = isCollection ? gn.Elements().ToList() : new List<XElement>() { gn };

                var values = new List<dynamic>();

                // use parallel as we dont really care of the order of our properties
                // and it will help processing larger XMLs
                items.AsParallel().ForAll(i => values.Add((i.HasElements) ?
                   XMLtoExpando(null, i) : i.Value.Trim()));

                // Add the object name + value or value collection to the dictionary
                result[gn.Name.LocalName] = isCollection ? values : values.FirstOrDefault();
            });
            return result;
        }
        public static bool AreExpandoStructureEquals(ExpandoObject obj1, ExpandoObject obj2)
        {
            var obj1AsColl = (ICollection<KeyValuePair<string, object>>)obj1;
            var obj2AsDict = (IDictionary<string, object>)obj2;

            // Make sure they have the same number of properties
            if (obj1AsColl.Count != obj2AsDict.Count)
                return false;

            foreach (var pair in obj1AsColl)
            {
                if (!obj2AsDict.ContainsKey(pair.Key))
                {
                    return false;
                }
                else
                {
                    //if (obj2AsDict[pair.Key].GetType() != pair.Value.GetType())
                    //    return false;
                }

            }

            // Everything matches
            return true;
        }
    }
}
