#region --References--
using Generic_Deserialization_JSON.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;
#endregion

namespace Generic_Deserialization_JSON.Infrastructure.Mappers
{
    public class JsonToObjectMapper<TObject> : IJasonObjectMapper<TObject> where TObject : class, new()
    {
        // Method to map JSON input to a list of objects
        public IList<TObject> MapToList(string inputString, string configString, string? rootElement = null)
        {
            throw new NotImplementedException();
        }

        // Method to map JSON input to a single object
        public TObject MapToSingle(string inputString, string configString)
        {
            // Parse mapping configuration from string to dictionary
            var mapping = ParseMappingConfig(configString);

            // Parse JSON input string to JObject
            var obj = ParseJsonInput(inputString);

            // Create instance of TObject
            TObject entity = CreateInstance<TObject>();

            // Map properties from JSON to entity object
            MapProperties(obj, mapping, entity);

            return entity;
        }

        // Method to parse mapping configuration string to dictionary
        private Dictionary<string, string> ParseMappingConfig(string configString)
        {
            // Deserialize JSON configuration string to dictionary
            var mapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(configString);

            // Check if mapping is null
            if (mapping == null)
                throw new Exception($"Invalid mapping configuration");

            return mapping;
        }

        // Method to parse JSON input string to JObject
        private JObject ParseJsonInput(string inputString)
        {
            // Parse input string to JObject
            var obj = JObject.Parse(inputString);

            // Check if JObject is null
            if (obj == null)
                throw new Exception("Unable to parse JSON input");

            return obj;
        }

        // Method to create instance of generic type TObject
        private TObject CreateInstance<T>()
        {
            return Activator.CreateInstance<TObject>();
        }

        // Method to map properties from JObject to entity object
        private void MapProperties(JObject obj, Dictionary<string, string> mapping, TObject entity)
        {
            foreach (var keyValuePair in mapping)
            {
                var propertyName = keyValuePair.Key;
                var path = keyValuePair.Value;

                var token = obj.SelectToken(path);

                var property = entity.GetType().GetProperty(propertyName);

                if (property == null)
                    continue;

                if (token is JArray && token.Count() != 0)
                {
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        MapListProperty(obj, mapping, property, token, entity);
                    }
                    else
                    {
                        MapSingleProperty(obj, property, token, entity);
                    }
                }
                else
                {
                    if (token == null)
                    {
                        Console.WriteLine($"Token is null");
                        throw new Exception("Token is null");
                    }
                    else
                        MapSingleProperty(obj, property, token, entity);
                }
            }
        }

        // Method to map list property from JObject to entity object
        private void MapListProperty(JObject obj, Dictionary<string, string> mapping, PropertyInfo property, JToken token, TObject entity)
        {
            var listType = property.PropertyType.GetGenericArguments()[0];
            var collection = Activator.CreateInstance(property.PropertyType) as IList;

            if (collection == null)
            {
                // Handle the case where the created instance is not IList
                throw new InvalidCastException($"Failed to create IList from type {property.PropertyType}");
            }

            if (collection == null)
                return;

            foreach (JObject jobj in token)
            {
                var child = ConvertToEntity(jobj, mapping, listType);
                collection.Add(child);
            }

            property.SetValue(entity, collection);
        }

        // Method to map single property from JObject to entity object
        private void MapSingleProperty(JObject obj, PropertyInfo property, JToken token, TObject entity)
        {
            var jsonNode = obj[token.Path];
            if (jsonNode == null)
            {
                return;
            }
            var value = Convert.ChangeType(jsonNode.ToString(), property.PropertyType);
            property.SetValue(entity, value);
        }

        // Method to convert JObject to entity object
        private object ConvertToEntity(JObject jsonObject, Dictionary<string, string> mapping, Type entityType)
        {
            var entity = Activator.CreateInstance(entityType);

            if (entity == null)
                throw new Exception("Invalid mapping configuration");

            foreach (var propertyInfo in entityType.GetProperties())
            {
                if (mapping.TryGetValue($"{entityType.Name}.{propertyInfo.Name}", out var path))
                {
                    var token = jsonObject.SelectToken(path);

                    if (token is JArray && token.Count() != 0)
                    {
                        if (propertyInfo == null || propertyInfo.PropertyType == null)
                            continue;

                        var listType = propertyInfo.PropertyType.GetGenericArguments()[0];

                        var collection = Activator.CreateInstance(propertyInfo.PropertyType) as IList;

                        if (collection == null)
                        {
                            // Handle the case where the created instance is not IList
                            throw new InvalidCastException($"Failed to create IList from type {propertyInfo.PropertyType}");
                        }

                        if (collection == null)
                            continue;

                        foreach (JObject jobj in token)
                        {
                            var child = ConvertToEntity(jobj, mapping, listType);
                            collection.Add(child);
                        }

                        propertyInfo.SetValue(entity, collection);
                    }
                    else
                    {
                        var rs = jsonObject[propertyInfo.Name];
                        if (rs != null)
                        {
                            var val = rs.ToString();
                            if (propertyInfo.PropertyType != null)
                            {
                                var value = Convert.ChangeType(val, propertyInfo.PropertyType);
                                propertyInfo.SetValue(entity, value);
                            }
                            else
                            {
                                Console.WriteLine($"Mapping issue identified. PropertyType : {propertyInfo.PropertyType}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Mapping issue identified. PropertyName : {propertyInfo.Name}");
                        }
                    }
                }
            }

            return entity;
        }
    }

}
