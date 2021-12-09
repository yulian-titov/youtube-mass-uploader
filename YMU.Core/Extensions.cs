using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Contains variety of helper classes and methods.
/// </summary>
namespace YMU.Core {
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions {
        #region Collections

        /// <summary>
        /// Iterates through the provided list and executes action on each element.
        /// </summary>
        /// <typeparam name="T">Type of element.</typeparam>
        /// <param name="c">Input collection of elements.</param>
        /// <param name="action">Action which must be perfromed on each element.</param>
        /// <returns>Reference to provided list for future processing.</returns>
        public static IList<T> For<T>(this IList<T> c, Action<T, int> action) {
            for(int i = 0; i < c.Count; i++) {
                action(c[i], i);
            }
            return c;
        }

        #endregion

        #region Reflection

        /// <summary>
        /// Extracts values of all properties and fields from provided object.
        /// </summary>
        /// <param name="value">Provided instance of an object.</param>
        /// <returns>Dictionary, which contains values from all properties and fields and their names as a dictionary's keys.</returns>
        public static Dictionary<string, object> ToDictionary(this object value) {
            var dictionary = new Dictionary<string, object>();

            if(value == null) {
                return dictionary;
            }

            var fieldInfos = value.GetType().GetFields();
            foreach(var fieldInfo in fieldInfos) {
                dictionary.Add(fieldInfo.Name, fieldInfo.GetValue(value));
            }

            //To get value of property on mobile builds you should set the [Preserve] attribute on it
            //https://docs.unity3d.com/ScriptReference/Scripting.PreserveAttribute.html
            var propertiesInfos = value.GetType().GetProperties();
            foreach(var propertyInfo in propertiesInfos) {
                dictionary.Add(propertyInfo.Name, propertyInfo.GetValue(value));
            }

            return dictionary;
        }

        /// <summary>
        /// Set Properties and Fields available in 'members' Dictionary to 'obj' of type 'T'.
        /// </summary>
        /// <typeparam name="T">Type of the object which properties and fields we want to set.</typeparam>
        /// <typeparam name="V">Type of the values which we want to set.</typeparam>
        /// <param name="obj">Instance of an object which properties and fields we want to set.</param>
        /// <param name="members">List of member names and their values which we want to set.</param>
        /// <returns>Instance of an object with properties and fields updated with a new values.</returns>
        public static T Bind<T, V>(this T obj, Dictionary<string, V> members) {
            var bindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var properties = obj.GetType().GetProperties(bindingFlags);
            foreach(var p in properties) {
                if(members.TryGetValue(p.Name, out V value)) {
                    var convertedValue = value.TryConvert(p.PropertyType);
                    p.SetValue(obj, convertedValue);
                }
            }
            var fields = obj.GetType().GetFields(bindingFlags);
            foreach(var f in fields) {
                if(members.TryGetValue(f.Name, out V value)) {
                    var convertedValue = value.TryConvert(f.FieldType);
                    f.SetValue(obj, convertedValue);
                }
            }
            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static object[] ToArray(this string array, Type valueType) {
            var result = new List<object>();
            var values = array.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var v in values) {
                result.Add(v.TryConvert(valueType));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Converts string with comma separated values to the array of values with given type.
        /// </summary>
        /// <typeparam name="T">Type of values which must be parsed from provided string.</typeparam>
        /// <param name="array">Input string with comma separated values.</param>
        /// <returns>The array of values with given type.</returns>
        public static T[] ToArray<T>(this string array) {
            var result = new List<T>();
            var values = array.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var v in values) {
                result.Add(v.TryConvert<T>());
            }
            return result.ToArray();
        }

        /// <summary>
        /// Method tries to perform conversion of the given object to the object with given type. If conversion is not possible, method returns 'null' or default value.
        /// </summary>
        /// <param name="value">Instance of the object which should be converted.</param>
        /// <param name="convertType">Target converting type.</param>
        /// <returns>Instance of the target converting type or if conversion is not possible: 'null' or default value.</returns>
        public static object TryConvert(this object value, Type convertType) {
            try {
                if(convertType.IsValueType) {
                    if(value == null) {
                        return Convert.ChangeType(Activator.CreateInstance(convertType), convertType, CultureInfo.InvariantCulture);
                    }
                    if(value is string v) {
                        var indexer = convertType.GetProperty("Item");
                        if(indexer != null) { // We have type with Indexer like an Array or Vector2, Vector3, etc:
                            var propertyType = indexer.PropertyType;
                            var values = v.ToArray(propertyType);
                            var instance = Activator.CreateInstance(convertType);
                            object[] index = new object[1];
                            values.For((v, i) => {
                                index[0] = i;
                                indexer.SetValue(instance, v, index);
                            });
                            return instance;
                        } else if(convertType.IsEnum) {
                            return Enum.Parse(convertType, v);
                        } else if(string.IsNullOrEmpty(v)) {
                            return Convert.ChangeType(Activator.CreateInstance(convertType), convertType, CultureInfo.InvariantCulture);
                        } else {
                            return Convert.ChangeType(value, convertType, CultureInfo.InvariantCulture);
                        }
                    } else {
                        return Convert.ChangeType(value, convertType, CultureInfo.InvariantCulture);
                    }
                } else if(convertType.IsEnum && value is string str) {
                    return Enum.Parse(convertType, str, true);
                } else {
                    return Convert.ChangeType(value, convertType, CultureInfo.InvariantCulture);
                }
            } catch {
                Console.WriteLine($"[Extensions.TryConvert] Not possible to convert '{value}' to type '{convertType.Name}'");
                return null;
            }
        }

        /// <summary>
        /// Method tries to perform conversion of the given object to the object with given type. If conversion is not possible, method returns 'null' or default value.
        /// </summary>
        /// <typeparam name="T">Target converting type.</typeparam>
        /// <param name="value">Instance of the object which should be converted.</param>
        /// <returns>Instance of the target converting type or if conversion is not possible: 'null' or default value.</returns>
        public static T TryConvert<T>(this object value) {
            var result = value.TryConvert(typeof(T));
            if(result != null)
                return (T)result;
            return default;
        }

        #endregion
    }
}