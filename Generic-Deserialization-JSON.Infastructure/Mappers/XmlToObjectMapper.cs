using Generic_Deserialization_JSON.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;

namespace Generic_Deserialization_JSON.Infastructure.Mappers
{
    public class XmlToObjectMapper<TObject> : IObjectMapper<TObject> where TObject : class, new()
    {
        public IList<TObject> MapToList(string inputString, string configString, string? rootElement = null)
        {
            throw new NotImplementedException();
        }

        public TObject MapToSingle(string inputString, string configString)
        {
            var mapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(configString);

            inputString = RemoveXmlnsDeclarations(inputString);

            if (mapping == null)
                throw new Exception($"Invalid mapping configuration");

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(inputString);

            var entity = Activator.CreateInstance<TObject>();

            foreach (var keyValue in mapping)
            {
                var propertyName = keyValue.Key;
                var xpath = keyValue.Value;

                var xmlNodeList = xmlDoc.SelectNodes(xpath);

                if (xmlNodeList != null && xmlNodeList.Count > 0)
                {
                    var property = entity.GetType().GetProperty(propertyName);
                    if (property != null)
                    {
                        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var listType = property.PropertyType.GetGenericArguments()[0];
                            var collection = Activator.CreateInstance(property.PropertyType) as IList;

                            if (collection == null)
                            {
                                // Handle the case where the created instance is not IList
                                throw new InvalidCastException($"Failed to create IList from type {property.PropertyType}");
                            }

                            foreach (XmlNode xmlNode in xmlNodeList)
                            {
                                var child = ConvertToEntity(xmlNode, mapping, listType);
                                collection.Add(child);
                            }

                            property.SetValue(entity, collection);
                        }
                        else
                        {
                            var xmlNode = xmlNodeList[0];
                            if (xmlNode != null)
                            {
                                var value = Convert.ChangeType(xmlNode.InnerText, property.PropertyType);
                                property.SetValue(entity, value);
                            }
                        }
                    }
                }
            }

            return entity;
        }

        private object? ConvertToEntity(XmlNode xmlNode, Dictionary<string, string> mapping, Type entityType)
        {
            var entity = Activator.CreateInstance(entityType);

            foreach (var propertyInfo in entityType.GetProperties())
            {
                if (mapping.TryGetValue($"{entityType.Name}.{propertyInfo.Name}", out var xpath))
                {
                    var nestedXmlNodeList = xmlNode.SelectNodes(xpath);

                    if (nestedXmlNodeList != null && nestedXmlNodeList.Count > 0)
                    {
                        if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var listType = propertyInfo.PropertyType.GetGenericArguments()[0];
                            var collection = Activator.CreateInstance(propertyInfo.PropertyType) as IList;

                            if (collection == null)
                            {
                                // Handle the case where the created instance is not IList
                                throw new InvalidCastException($"Failed to create IList from type {propertyInfo.PropertyType}");
                            }

                            foreach (XmlNode nestedNode in nestedXmlNodeList)
                            {
                                var nestedChild = ConvertToEntity(nestedNode, mapping, listType);
                                collection.Add(nestedChild);
                            }

                            propertyInfo.SetValue(entity, collection);
                        }
                        else
                        {
                            var value = Convert.ChangeType(nestedXmlNodeList[0]?.InnerText, propertyInfo.PropertyType);
                            propertyInfo.SetValue(entity, value);
                        }
                    }
                }
            }

            return entity;
        }

        private string RemoveXmlnsDeclarations(string inputString)
        {
            return Regex.Replace(inputString, @"(xmlns:?[^=]*=[""][^""]*[""])", "", RegexOptions.IgnoreCase);
        }
    }

}
