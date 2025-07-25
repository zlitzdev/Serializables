using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

using UnityEngine;

namespace Zlitz.General.Serializables
{
    internal class JsonFormatContext
    {
        public bool prettyPrint { get; private set; }
        
        private string m_indent = "";
        
        public string ApplyIndent(string value)
        {
            return m_indent + value;
        }

        public string RemoveIndent(string value)
        {
            return value.TrimStart('\t');
        }

        public IDisposable IndentScope()
        {
            return new AddIndentScopeDisposable(this);
        }

        public JsonFormatContext(bool prettyPrint)
        {
            this.prettyPrint = prettyPrint;
        }

        private class AddIndentScopeDisposable : IDisposable
        {
            private JsonFormatContext m_context;

            internal AddIndentScopeDisposable(JsonFormatContext context)
            {
                m_context = context;
                m_context.m_indent += '\t';
            }

            void IDisposable.Dispose()
            {
                m_context.m_indent = m_context.m_indent.Remove(m_context.m_indent.Length - 1);
            }
        }
    }

    internal interface IJsonValue
    {
        internal IJsonValue Copy();

        public string ToJsonString(bool prettyPrint = false)
        {
            JsonFormatContext context = new JsonFormatContext(prettyPrint);
            return ToJsonString(context);
        }

        internal string ToJsonString(JsonFormatContext context);

        public bool FromJsonString(string jsonString);
        
    }

    [Serializable]
    public sealed class JsonValue : IJsonValue
    {
        [SerializeReference]
        private IJsonValue m_value;

        public bool IsNull()
        {
            return m_value == null;
        }

        public bool IsBool(out JsonBool value)
        {
            value = default;

            if (m_value is JsonBool jsonBool)
            {
                value = jsonBool;
                return true;
            }

            return false;
        }

        public bool IsNumber(out JsonNumber value)
        {
            value = default;

            if (m_value is JsonNumber jsonNumber)
            {
                value = jsonNumber;
                return true;
            }

            return false;
        }

        public bool IsString(out JsonString value)
        {
            value = default;

            if (m_value is JsonString jsonString)
            {
                value = jsonString;
                return true;
            }

            return false;
        }

        public bool IsArray(out JsonArray value)
        {
            value = default;

            if (m_value is JsonArray jsonArray)
            {
                value = jsonArray;
                return true;
            }

            return false;
        }

        public bool IsObject(out JsonObject value)
        {
            value = default;

            if (m_value is JsonObject jsonObject)
            {
                value = jsonObject;
                return true;
            }

            return false;
        }

        public void AsNull()
        {
            m_value = null;
        }

        public JsonBool AsBool()
        {
            if (m_value == null || m_value is not JsonBool jsonBool)
            {
                jsonBool = new JsonBool();
                m_value = jsonBool;
            }

            return jsonBool;
        }

        public JsonNumber AsNumber()
        {
            if (m_value == null || m_value is not JsonNumber jsonNumber)
            {
                jsonNumber = new JsonNumber();
                m_value = jsonNumber;
            }

            return jsonNumber;
        }

        public JsonString AsString()
        {
            if (m_value == null || m_value is not JsonString jsonString)
            {
                jsonString = new JsonString();
                m_value = jsonString;
            }

            return jsonString;
        }

        public JsonArray AsArray()
        {
            if (m_value == null || m_value is not JsonArray jsonArray)
            {
                jsonArray = new JsonArray();
                m_value = jsonArray;
            }

            return jsonArray;
        }

        public JsonObject AsObject()
        {
            if (m_value == null || m_value is not JsonObject jsonObject)
            {
                jsonObject = new JsonObject();
                m_value = jsonObject;
            }

            return jsonObject;
        }

        #region IJsonValue

        public JsonValue Copy()
        {
            JsonValue result = new JsonValue();
            result.m_value = m_value?.Copy();
            return result;
        }

        IJsonValue IJsonValue.Copy()
        {
            return Copy();
        }

        public string ToJsonString(bool prettyPrint = false)
        {
            JsonFormatContext context = new JsonFormatContext(prettyPrint);
            return (this as IJsonValue).ToJsonString(context);
        }

        string IJsonValue.ToJsonString(JsonFormatContext context)
        {
            if (m_value != null)
            {
                return m_value.ToJsonString(context);
            }

            if (context.prettyPrint)
            {
                return context.ApplyIndent("null");
            }

            return "null";
        }

        public bool FromJsonString(string jsonString)
        {
            jsonString = jsonString?.Trim();

            if (jsonString == "null")
            {
                m_value = null;
                return true;
            }

            JsonBool jsonBool = new JsonBool();
            if (jsonBool.FromJsonString(jsonString))
            {
                m_value = jsonBool;
                return true;
            }

            JsonNumber jsonNumber = new JsonNumber();
            if (jsonNumber.FromJsonString(jsonString))
            {
                m_value = jsonNumber;
                return true;
            }

            JsonString jsonStr = new JsonString();
            if (jsonStr.FromJsonString(jsonString))
            {
                m_value = jsonStr;
                return true;
            }

            JsonArray jsonArray = new JsonArray();
            if (jsonArray.FromJsonString(jsonString))
            {
                m_value = jsonArray;
                return true;
            }

            JsonObject jsonObject = new JsonObject();
            if (jsonObject.FromJsonString(jsonString))
            {
                m_value = jsonObject;
                return true;
            }

            return false;
        }

        #endregion
    }

    [Serializable]
    public sealed class JsonBool : IJsonValue
    {
        [SerializeField]
        private bool m_value;

        public bool value
        {
            get => m_value;
            set => m_value = value;
        }

        #region IJsonValue

        IJsonValue IJsonValue.Copy()
        {
            return new JsonBool()
            {
                m_value = m_value
            };
        }

        string IJsonValue.ToJsonString(JsonFormatContext context)
        {
            string result = m_value ? "true" : "false";
            if (context.prettyPrint)
            {
                result = context.ApplyIndent(result);
            }
            return result;
        }

        public bool FromJsonString(string jsonString)
        {
            jsonString = jsonString?.Trim();
            switch (jsonString)
            {
                case "true":
                    {
                        m_value = true;
                        return true;
                    }
                case "false":
                    {
                        m_value = false;
                        return true;
                    }
            }
            return false;
        }

        #endregion
    }

    [Serializable]
    public sealed class JsonNumber : IJsonValue
    {
        [SerializeField]
        private double m_value;

        #region IJsonValue

        IJsonValue IJsonValue.Copy()
        {
            return new JsonNumber()
            {
                m_value = m_value
            };
        }

        string IJsonValue.ToJsonString(JsonFormatContext context)
        {
            string result = m_value.ToString(CultureInfo.InvariantCulture);
            if (context.prettyPrint)
            {
                result = context.ApplyIndent(result);
            }
            return result;
        }

        public bool FromJsonString(string jsonString)
        {
            jsonString = jsonString?.Trim();
            if (double.TryParse(jsonString, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                m_value = value;
                return true;
            }

            return false;
        }

        #endregion

        public double value
        {
            get => m_value;
            set => m_value = value;
        }
    }

    [Serializable]
    public sealed class JsonString : IJsonValue
    {
        [SerializeField]
        private string m_value;

        #region IJsonValue

        IJsonValue IJsonValue.Copy()
        {
            return new JsonString()
            {
                m_value = m_value
            };
        }

        string IJsonValue.ToJsonString(JsonFormatContext context)
        {
            string result = JsonUtility.ToJson((0, m_value))
                .Replace("{\"Item1\":0,\"Item2\":", "")
                .TrimEnd('}'); ;
            if (context.prettyPrint)
            {
                result = context.ApplyIndent(result);
            }
            return result;
        }

        public bool FromJsonString(string jsonString)
        {
            if (jsonString == null)
            {
                return false;
            }

            jsonString = jsonString.Trim();
            if (!jsonString.StartsWith('"') || !jsonString.EndsWith('"'))
            {
                return false;
            }

            string json = "{\"Item1\":0,\"Item2\":" + jsonString + "}";
            try
            {
                (int _, string value) = JsonUtility.FromJson<(int, string)>(json);
                m_value = value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public string value
        {
            get => m_value;
            set => m_value = value;
        }
    }

    [Serializable]
    public sealed class JsonArray : IJsonValue, IReadOnlyList<JsonValue>
    {
        [SerializeField]
        private List<JsonValue> m_values = new List<JsonValue>();

        #region IReadOnlyList<JsonValue>

        public JsonValue this[int index]
        {
            get => m_values[index];
            set => m_values[index] = value;
        }

        public int Count => m_values.Count;

        public bool IsReadOnly => ((ICollection<JsonValue>)m_values).IsReadOnly;

        IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator()
        {
            return m_values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_values.GetEnumerator();
        }

        #endregion

        #region IJsonValue

        IJsonValue IJsonValue.Copy()
        {
            return new JsonArray()
            {
                m_values = m_values.Select(v => v.Copy()).ToList()
            };
        }

        string IJsonValue.ToJsonString(JsonFormatContext context)
        {
            StringBuilder result = new StringBuilder();

            if (context.prettyPrint)
            {
                result.AppendLine(context.ApplyIndent("["));

                using (IDisposable scope = context.IndentScope())
                {
                    bool start = true;
                    foreach (JsonValue v in m_values)
                    {
                        if (!start)
                        {
                            result.AppendLine(", ");
                        }
                        else
                        {
                            start = false;
                        }
                        result.Append((v as IJsonValue).ToJsonString(context));
                    }

                    if (!start)
                    {
                        result.AppendLine();
                    }
                }

                result.Append(context.ApplyIndent("]"));
            }
            else
            {
                result.Append("[");

                bool start = true;
                foreach (JsonValue v in m_values)
                {
                    if (!start)
                    {
                        result.Append(", ");
                    }
                    else
                    {
                        start = false;
                    }
                    result.Append((v as IJsonValue).ToJsonString(context));
                }

                result.Append("]");
            }

            return result.ToString();
        }

        public bool FromJsonString(string jsonString)
        {
            if (jsonString == null)
            {
                return false;
            }

            jsonString = jsonString.Trim();

            if (!jsonString.StartsWith('['))
            {
                return false;
            }
            jsonString = jsonString.Substring(1);

            if (!jsonString.EndsWith(']'))
            {
                return false;
            }
            jsonString = jsonString.Substring(0, jsonString.Length - 1);

            IEnumerable<string> splits = JsonFormatHelper.JsonSafeSplit(jsonString, ',');

            List<JsonValue> values = new List<JsonValue>();
            foreach (string element in splits)
            {
                JsonValue jsonValue = new JsonValue();
                if (!jsonValue.FromJsonString(element))
                {
                    return false;
                }

                values.Add(jsonValue);
            }

            m_values = values;
            return true;
        }

        #endregion

        public JsonValue Insert(int index = -1)
        {
            if (index < 0)
            {
                index = Count;
            }

            if (index > Count)
            {
                return null;
            }

            JsonValue value = new JsonValue();
            m_values.Insert(index, value);
            return value;
        }

        public bool Remove(int index)
        {
            if (index < 0 || index >= Count)
            {
                return false;
            }

            m_values.RemoveAt(index);
            return true;
        }
    
        public void Clear()
        {
            m_values.Clear();
        }

        public void CopyFrom(IEnumerable<JsonValue> jsonArray)
        {
            m_values = jsonArray?.Select(v =>
            {
                JsonValue newValue = v?.Copy();
                if (newValue == null)
                {
                    newValue = new JsonValue();
                }
                return newValue;
            }).ToList() ?? new List<JsonValue>();
        }
    }

    [Serializable]
    public sealed class JsonObject : IJsonValue, ISerializationCallbackReceiver, IEnumerable<KeyValuePair<string, JsonValue>>
    {
        [SerializeField]
        private List<Pair<string, JsonValue>> m_list = new List<Pair<string, JsonValue>>();

        private Dictionary<string, JsonValue> m_values = new Dictionary<string, JsonValue>();

        #region ISerializationCallbackReceiver

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (m_list == null)
            {
                m_list = new List<Pair<string, JsonValue>>();
            }

            HashSet<string> handledKeys = new HashSet<string>(EqualityComparer<string>.Default);

            for (int i = m_list.Count - 1; i >= 0; i--)
            {
                string key = m_list[i].key;
                if (handledKeys.Add(key))
                {
                    if (!m_values.TryGetValue(key, out JsonValue value))
                    {
                        m_list.RemoveAt(i);
                        i--;
                        continue;
                    }
                    m_list[i] = new Pair<string, JsonValue>(key, value);
                }
            }

            HashSet<KeyValuePair<string, JsonValue>> missing = new HashSet<KeyValuePair<string, JsonValue>>(m_values, new PairComparer());
            missing.ExceptWith(m_list.Select(i => new KeyValuePair<string, JsonValue>(i.key, i.value)));
            m_list.AddRange(missing.Select(i => new Pair<string, JsonValue>(i.Key, i.Value)));
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_values.Clear();
            foreach (Pair<string, JsonValue> item in m_list)
            {
                if (item.key == null)
                {
                    continue;
                }
                m_values[item.key] = item.value;
            }
        }

        #endregion

        #region IEnumerable<string, JsonValue>

        IEnumerator<KeyValuePair<string, JsonValue>> IEnumerable<KeyValuePair<string, JsonValue>>.GetEnumerator()
        {
            return m_values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_values.GetEnumerator();
        }

        #endregion

        #region IJsonValue

        IJsonValue IJsonValue.Copy()
        {
            return new JsonObject()
            {
                m_list = m_list.Select(v => new Pair<string, JsonValue>(v.key, v.value?.Copy())).ToList(),
                m_values = m_values.ToDictionary(p => p.Key, p => p.Value?.Copy())
            };
        }

        string IJsonValue.ToJsonString(JsonFormatContext context)
        {
            StringBuilder result = new StringBuilder();

            if (context.prettyPrint)
            {
                result.AppendLine(context.ApplyIndent("{"));

                using (IDisposable scope = context.IndentScope())
                {
                    bool start = true;
                    foreach (KeyValuePair<string, JsonValue> keyValue in m_values)
                    {
                        if (!start)
                        {
                            result.AppendLine(", ");
                        }
                        else
                        {
                            start = false;
                        }

                        string element = (keyValue.Value as IJsonValue).ToJsonString(context);
                        element = context.ApplyIndent($"\"{keyValue.Key}\": {context.RemoveIndent(element)}");
                        result.Append(element);
                    }

                    if (!start)
                    {
                        result.AppendLine();
                    }
                }

                result.Append(context.ApplyIndent("}"));
            }
            else
            {
                result.Append("{");

                bool start = true;
                foreach (KeyValuePair<string, JsonValue> keyValue in m_values)
                {
                    if (!start)
                    {
                        result.Append(", ");
                    }
                    else
                    {
                        start = false;
                    }
                    result.Append($"\"{keyValue.Key}\": {context.RemoveIndent((keyValue.Value as IJsonValue).ToJsonString(context))}");
                }

                result.Append("}");
            }

            return result.ToString();
        }

        public bool FromJsonString(string jsonString)
        {
            if (jsonString == null)
            {
                return false;
            }

            jsonString = jsonString.Trim();

            if (!jsonString.StartsWith('{'))
            {
                return false;
            }
            jsonString = jsonString.Substring(1);

            if (!jsonString.EndsWith('}'))
            {
                return false;
            }
            jsonString = jsonString.Substring(0, jsonString.Length - 1);

            IEnumerable<string> splits = JsonFormatHelper.JsonSafeSplit(jsonString, ',');

            JsonString jsonKey = new JsonString();

            Dictionary<string, JsonValue> values = new Dictionary<string, JsonValue>();
            foreach (string element in splits)
            {
                string[] keyAndValueStr = JsonFormatHelper.JsonSafeSplit(element, ':').ToArray();
                if (keyAndValueStr.Length != 2)
                {
                    return false;
                }

                if (!jsonKey.FromJsonString(keyAndValueStr[0]))
                {
                    return false;
                }
                string key = jsonKey.value;
                
                JsonValue jsonValue = new JsonValue();
                if (!jsonValue.FromJsonString(keyAndValueStr[1]))
                {
                    return false;
                }

                values[key] = jsonValue;
            }

            m_values = values;
            (this as ISerializationCallbackReceiver).OnBeforeSerialize();

            return true;
        }

        #endregion

        public JsonValue Get(string key)
        {
            if (!TryGet(key, out JsonValue value))
            {
                return null;
            }
            return value;
        }

        public JsonValue GetOrCreate(string key)
        {
            if (!m_values.TryGetValue(key, out JsonValue currentValue))
            {
                currentValue = new JsonValue();
                m_values.Add(key, currentValue);
                m_list.Add(new Pair<string, JsonValue>(key, currentValue));
            }

            return currentValue;
        }

        public bool TryGet(string key, out JsonValue value)
        {
            value = null;

            if (m_values.TryGetValue(key, out JsonValue currentValue))
            {
                value = currentValue;
            }

            return value != null;
        }

        public bool Remove(string key)
        {
            if (m_values.Remove(key))
            {
                if (m_list != null)
                {
                    for (int i = 0; i < m_list.Count; i++)
                    {
                        if (EqualityComparer<string>.Default.Equals(m_list[i].key, key))
                        {
                            m_list.RemoveAt(i);
                            break;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public void Clear()
        {
            m_values.Clear();
            m_list.Clear();
        }

        public void CopyFrom(IEnumerable<KeyValuePair<string, JsonValue>> jsonObject)
        {
            m_values = jsonObject?.Select(kv =>
            {
                return new KeyValuePair<string, JsonValue>
                (
                    kv.Key,
                    kv.Value?.Copy() ?? new JsonValue()
                );
            }).ToDictionary
            (
                kv => kv.Key,
                kv => kv.Value
            ) ?? new Dictionary<string, JsonValue>();
        }

        private class PairComparer : IEqualityComparer<KeyValuePair<string, JsonValue>>
        {
            private EqualityComparer<string> m_keyComparer = EqualityComparer<string>.Default;

            public bool Equals(KeyValuePair<string, JsonValue> x, KeyValuePair<string, JsonValue> y)
            {
                return m_keyComparer.Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<string, JsonValue> obj)
            {
                return m_keyComparer.GetHashCode(obj.Key);
            }
        }
    }

    internal static class JsonFormatHelper
    {
        public static IEnumerable<string> JsonSafeSplit(string value, char separator)
        {
            int sIndex = 0;
            int eIndex = 0;

            bool quotations = false;

            int squareBrackets = 0;
            int curlyBrackets  = 0;

            List<string> values = new List<string>();

            while (eIndex < value.Length)
            {
                char c = value[eIndex];
                if (c == '"')
                {
                    quotations = !quotations;
                    eIndex++;
                    continue;
                }

                if (c == '[')
                {
                    squareBrackets++;
                    eIndex++;
                    continue;
                }
                if (c == ']')
                {
                    squareBrackets--;
                    eIndex++;
                    continue;
                }

                if (c == '{')
                {
                    curlyBrackets++;
                    eIndex++;
                    continue;
                }
                if (c == '}')
                {
                    curlyBrackets--;
                    eIndex++;
                    continue;
                }

                if (c == '\\')
                {
                    eIndex++;
                    if (eIndex < value.Length && value[eIndex] == '"')
                    {
                        eIndex++;
                    }
                    continue;
                }

                if (!quotations && curlyBrackets == 0 && squareBrackets == 0 && c == separator)
                {
                    string jsonValueStr = value.Substring(sIndex, eIndex - sIndex);
                    values.Add(jsonValueStr);
                    sIndex = eIndex + 1;
                }

                eIndex++;
            }

            string lastJsonValueStr = value.Substring(sIndex);
            values.Add(lastJsonValueStr);

            values = values.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return values;
        }
    }
}