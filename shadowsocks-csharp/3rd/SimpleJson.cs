//-----------------------------------------------------------------------
// <copyright file="SimpleJson.cs" company="The Outercurve Foundation">
//    Copyright (c) 2011, The Outercurve Foundation.
//
//    Licensed under the MIT License (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.opensource.org/licenses/mit-license.php
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// <author>Nathan Totten (ntotten.com), Jim Zimmerman (jimzimmerman.com) and Prabir Shrestha (prabir.me)</author>
// <website>https://github.com/facebook-csharp-sdk/simple-json</website>
//-----------------------------------------------------------------------

// VERSION:

// NOTE: uncomment the following line to make SimpleJson class internal.
//#define SIMPLE_JSON_INTERNAL

// NOTE: uncomment the following line to make JsonArray and JsonObject class internal.
//#define SIMPLE_JSON_OBJARRAYINTERNAL

// NOTE: uncomment the following line to enable dynamic support.
//#define SIMPLE_JSON_DYNAMIC

// NOTE: uncomment the following line to enable DataContract support.
#define SIMPLE_JSON_DATACONTRACT

// NOTE: uncomment the following line to use Reflection.Emit (better performance) instead of method.invoke().
// don't enable ReflectionEmit for WinRT, Silverlight and WP7.
//#define SIMPLE_JSON_REFLECTIONEMIT

// NOTE: uncomment the following line if you are compiling under Window Metro style application/library.
// usually already defined in properties
//#define NETFX_CORE;

// original json parsing code from http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
#if SIMPLE_JSON_DYNAMIC
using System.Dynamic;
#endif
using System.Globalization;
using System.Reflection;
#if SIMPLE_JSON_REFLECTIONEMIT
using System.Reflection.Emit;
#endif
#if SIMPLE_JSON_DATACONTRACT
using System.Runtime.Serialization;
#endif
using System.Text;
using System.Threading;
using SimpleJson.Reflection;

namespace SimpleJson
{
    #region JsonArray

    /// <summary>
    /// Represents the json array.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
#if SIMPLE_JSON_OBJARRAYINTERNAL
    internal
#else
    public
#endif
 class JsonArray : List<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonArray"/> class. 
        /// </summary>
        public JsonArray() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonArray"/> class. 
        /// </summary>
        /// <param name="capacity">The capacity of the json array.</param>
        public JsonArray(int capacity) : base(capacity) { }

        /// <summary>
        /// The json representation of the array.
        /// </summary>
        /// <returns>The json representation of the array.</returns>
        public override string ToString()
        {
            return SimpleJson.SerializeObject(this) ?? string.Empty;
        }
    }

    #endregion

    #region JsonObject

    /// <summary>
    /// Represents the json object.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
#if SIMPLE_JSON_OBJARRAYINTERNAL
    internal
#else
    public
#endif
 class JsonObject :
#if SIMPLE_JSON_DYNAMIC
 DynamicObject,
#endif
 IDictionary<string, object>, IDictionary
    {
        /// <summary>
        /// The internal member dictionary.
        /// </summary>
        private readonly Dictionary<string, object> _members = new Dictionary<string, object>();

        /// <summary>
        /// Gets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        /// <value></value>
        public object this[int index]
        {
            get { return GetAtIndex(_members, index); }
        }

        internal static object GetAtIndex(IDictionary<string, object> obj, int index)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (index >= obj.Count)
                throw new ArgumentOutOfRangeException("index");

            int i = 0;
            lock(obj)
            foreach (KeyValuePair<string, object> o in obj)
                if (i++ == index) return o.Value;

            return null;
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, object value)
        {
            _members.Add(key, value);
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string key)
        {
            return _members.ContainsKey(key);
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<string> Keys
        {
            get { return _members.Keys; }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return _members.Remove(key);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out object value)
        {
            return _members.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<object> Values
        {
            get { return _members.Values; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value></value>
        public object this[string key]
        {
            get { return _members[key]; }
            set { _members[key] = value; }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<string, object> item)
        {
            _members.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _members.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return _members.ContainsKey(item.Key) && _members[item.Key] == item.Value;
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            int num = Count;
            foreach (KeyValuePair<string, object> kvp in this)
            {
                array[arrayIndex++] = kvp;

                if (--num <= 0)
                    return;
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return _members.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return false; }
        }

		ICollection IDictionary.Keys
		{
			get
			{
				return _members.Keys;
			}
		}

		ICollection IDictionary.Values
		{
			get
			{
				return _members.Values;
			}
		}

		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		private object _syncRoot;

		public object SyncRoot
		{
			get
			{
				if (this._syncRoot == null)
				{
					Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
				}
				return this._syncRoot;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public object this[object key]
		{
			get { return _members[(string)key]; }
			set { _members[(string)key] = value; }
		}

		/// <summary>
		/// Removes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public bool Remove(KeyValuePair<string, object> item)
        {
            return _members.Remove(item.Key);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        /// <summary>
        /// Returns a json <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// null if failed.
		/// </summary>
        /// <returns>
        /// A json <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return SimpleJson.SerializeObject(this);
        }

		public bool Contains(object key)
		{			
			return _members.ContainsKey((string)key);
		}

		public void Add(object key, object value)
		{
			_members.Add((string)key, value); 
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return (IDictionaryEnumerator) _members.GetEnumerator();
		}

		public void Remove(object key)
		{
			_members.Remove((string)key);
		}

		public void CopyTo(Array array, int index)
		{
			int num = Count;
			foreach (KeyValuePair<string, object> kvp in this)
			{				
				array.SetValue(kvp, index++);

				if (--num <= 0)
					return;
			}
		}

#if SIMPLE_JSON_DYNAMIC
        /// <summary>
        /// Provides implementation for type conversion operations. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations that convert an object from one type to another.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation. The binder.Type property provides the type to which the object must be converted. For example, for the statement (String)sampleObject in C# (CType(sampleObject, Type) in Visual Basic), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Type returns the <see cref="T:System.String"/> type. The binder.Explicit property provides information about the kind of conversion that occurs. It returns true for explicit conversion and false for implicit conversion.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns>
        /// Alwasy returns true.
        /// </returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            // <pex>
            if (binder == (ConvertBinder)null)
                throw new ArgumentNullException("binder");
            // </pex>
            Type targetType = binder.Type;

            if ((targetType == typeof(IEnumerable)) ||
                (targetType == typeof(IEnumerable<KeyValuePair<string, object>>)) ||
                (targetType == typeof(IDictionary<string, object>)) ||
#if NETFX_CORE
 (targetType == typeof(IDictionary<,>))
#else
 (targetType == typeof(IDictionary))
#endif
)
            {
                result = this;
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        /// <summary>
        /// Provides the implementation for operations that delete an object member. This method is not intended for use in C# or Visual Basic.
        /// </summary>
        /// <param name="binder">Provides information about the deletion.</param>
        /// <returns>
        /// Alwasy returns true.
        /// </returns>
        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            // <pex>
            if (binder == (DeleteMemberBinder)null)
                throw new ArgumentNullException("binder");
            // </pex>
            return _members.Remove(binder.Name);
        }

        /// <summary>
        /// Provides the implementation for operations that get a value by index. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for indexing operations.
        /// </summary>
        /// <param name="binder">Provides information about the operation.</param>
        /// <param name="indexes">The indexes that are used in the operation. For example, for the sampleObject[3] operation in C# (sampleObject(3) in Visual Basic), where sampleObject is derived from the DynamicObject class, <paramref name="indexes"/> is equal to 3.</param>
        /// <param name="result">The result of the index operation.</param>
        /// <returns>
        /// Alwasy returns true.
        /// </returns>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1)
            {
                result = ((IDictionary<string, object>)this)[(string)indexes[0]];
                return true;
            }
            result = (object)null;
            return true;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        /// <returns>
        /// Alwasy returns true.
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            object value;
            if (_members.TryGetValue(binder.Name, out value))
            {
                result = value;
                return true;
            }
            result = (object)null;
            return true;
        }

        /// <summary>
        /// Provides the implementation for operations that set a value by index. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations that access objects by a specified index.
        /// </summary>
        /// <param name="binder">Provides information about the operation.</param>
        /// <param name="indexes">The indexes that are used in the operation. For example, for the sampleObject[3] = 10 operation in C# (sampleObject(3) = 10 in Visual Basic), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="indexes"/> is equal to 3.</param>
        /// <param name="value">The value to set to the object that has the specified index. For example, for the sampleObject[3] = 10 operation in C# (sampleObject(3) = 10 in Visual Basic), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="value"/> is equal to 10.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.
        /// </returns>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length == 1)
            {
                ((IDictionary<string, object>)this)[(string)indexes[0]] = value;
                return true;
            }

            return base.TrySetIndex(binder, indexes, value);
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // <pex>
            if (binder == (SetMemberBinder)null)
                throw new ArgumentNullException("binder");
            // </pex>
            _members[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (var key in Keys)
                yield return key;
        }
#endif
	}

    #endregion
}

namespace SimpleJson
{
    #region JsonParser

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    /// 
    /// JSON uses Arrays and Objects. These correspond here to the datatypes JsonArray(IList&lt;object>) and JsonObject(IDictionary&lt;string,object>).
    /// All numbers are parsed to doubles.
    /// </summary>
#if SIMPLE_JSON_INTERNAL
    internal
#else
    public
#endif
 class SimpleJson
    {
        private const int TOKEN_NONE = 0;
        private const int TOKEN_CURLY_OPEN = 1;
        private const int TOKEN_CURLY_CLOSE = 2;
        private const int TOKEN_SQUARED_OPEN = 3;
        private const int TOKEN_SQUARED_CLOSE = 4;
        private const int TOKEN_COLON = 5;
        private const int TOKEN_COMMA = 6;
        private const int TOKEN_STRING = 7;
        private const int TOKEN_NUMBER = 8;
        private const int TOKEN_TRUE = 9;
        private const int TOKEN_FALSE = 10;
        private const int TOKEN_NULL = 11;

        private const int BUILDER_CAPACITY = 2000;

		/// <summary>
		/// Parses the string json into a value of a simple type, or an array, or a dictionary.
		/// Throw if fail.
		/// </summary>
		/// <param name="json">A JSON string.</param>
		/// <returns>An IList&lt;object>, a IDictionary&lt;string,object>, a double, a string, null, true, or false</returns>
		public static object DeserializeJsonObj(string json)
        {
            object @object;
            if (TryDeserializeJsonObj(json, out @object))
                return @object;
            throw new SerializationException("Invalid JSON string");
        }

		/// <summary>
		/// Try parsing the json string into a value of a simple type, or an array, or a dictionary
		/// </summary>
		/// <param name="json">
		/// A JSON string.
		/// </param>
		/// <param name="object">
		/// The object: a simple type, or an array, or a dictionary
		/// </param>
		/// <returns>
		/// Returns true if successfull otherwise false.
		/// </returns>
		public static bool TryDeserializeJsonObj(string json, out object @object)
        {
            bool success = true;
            if (json != null)
            {
                char[] charArray = json.ToCharArray();
                int index = 0;
                @object = ParseJsonObj(charArray, ref index, ref success);
            }
            else
                @object = null;

            return success;
        }

        public static object DeserializeObject(string json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
        {
            object jsonObject = DeserializeJsonObj(json);

            return type == null || jsonObject != null &&
#if NETFX_CORE
	jsonObject.GetType().GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())
#else
	jsonObject.GetType().IsAssignableFrom(type)
#endif
			? jsonObject
			: (jsonSerializerStrategy ?? CurrentJsonSerializerStrategy).DeserializeObject(jsonObject, type);
        }

        public static object DeserializeObject(string json, Type type)
        {
            return DeserializeObject(json, type, null);
        }

        public static T DeserializeObject<T>(string json, IJsonSerializerStrategy jsonSerializerStrategy)
        {
            return (T)DeserializeObject(json, typeof(T), jsonSerializerStrategy);
        }

        public static T DeserializeObject<T>(string json)
        {
            return (T)DeserializeObject(json, typeof(T), null);
        }

        /// <summary>
        /// Converts an object into a JSON string, null if failed.
        /// </summary>
        /// <param name="obj">A IDictionary&lt;string,object> / IList&lt;object></param>
        /// <param name="jsonSerializerStrategy">Serializer strategy to use</param>
        /// <returns>A JSON encoded string, or null if object 'obj' is not serializable</returns>
        public static string SerializeObject(object obj, IJsonSerializerStrategy jsonSerializerStrategy)
        {
            StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
            bool success = SerializeObject(jsonSerializerStrategy, obj, builder);
            return (success ? builder.ToString() : null);
        }

		/// <summary>
		/// Converts an object into a JSON string, use global current strategy. null if failed.
		/// </summary>
		public static string SerializeObject(object obj)
        {
            return SerializeObject(obj, CurrentJsonSerializerStrategy);
        }

        protected static IDictionary<string, object> ParseDict(char[] json, ref int index, ref bool success)
        {
            IDictionary<string, object> table = new JsonObject();
            int token;

            // {
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                token = LookAhead(json, index);
                if (token == TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == TOKEN_COMMA)
                    NextToken(json, ref index);
                else if (token == TOKEN_CURLY_CLOSE)
                {
                    NextToken(json, ref index);
                    return table;
                }
                else
                {
                    // name
                    string name = ParseString(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    // :
                    token = NextToken(json, ref index);
                    if (token != TOKEN_COLON)
                    {
                        success = false;
                        return null;
                    }

                    // value
                    object value = ParseJsonObj(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    table[name] = value;
                }
            }

            return table;
        }

        protected static JsonArray ParseArray(char[] json, ref int index, ref bool success)
        {
            JsonArray array = new JsonArray();

            // [
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                int token = LookAhead(json, index);
                if (token == TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == TOKEN_COMMA)
                    NextToken(json, ref index);
                else if (token == TOKEN_SQUARED_CLOSE)
                {
                    NextToken(json, ref index);
                    break;
                }
                else
                {
                    object value = ParseJsonObj(json, ref index, ref success);
                    if (!success)
                        return null;
                    array.Add(value);
                }
            }

            return array;
        }

		/// <summary>
		/// return a simple type, or an dictionary, or an array.
		/// note the entire hirachy is parsed to json.
		/// </summary>
		/// <returns></returns>
        protected static object ParseJsonObj(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case TOKEN_STRING:
                    return ParseString(json, ref index, ref success);
                case TOKEN_NUMBER:
                    return ParseNumber(json, ref index, ref success);
                case TOKEN_CURLY_OPEN:
                    return ParseDict(json, ref index, ref success);
                case TOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index, ref success);
                case TOKEN_TRUE:
                    NextToken(json, ref index);
                    return true;
                case TOKEN_FALSE:
                    NextToken(json, ref index);
                    return false;
                case TOKEN_NULL:
                    NextToken(json, ref index);
                    return null;
                case TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

        protected static string ParseString(char[] json, ref int index, ref bool success)
        {
            StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
            char c;

            EatWhitespace(json, ref index);

            // "
            c = json[index++];

            bool complete = false;
            while (!complete)
            {
                if (index == json.Length)
                {
                    break;
                }

                c = json[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if (c == '\\')
                {
                    if (index == json.Length)
                        break;
                    c = json[index++];
                    if (c == '"')
                        s.Append('"');
                    else if (c == '\\')
                        s.Append('\\');
                    else if (c == '/')
                        s.Append('/');
                    else if (c == 'b')
                        s.Append('\b');
                    else if (c == 'f')
                        s.Append('\f');
                    else if (c == 'n')
                        s.Append('\n');
                    else if (c == 'r')
                        s.Append('\r');
                    else if (c == 't')
                        s.Append('\t');
                    else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint;
                            if (
                                !(success =
                                  UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber,
                                                  CultureInfo.InvariantCulture, out codePoint)))
                                return "";

                            // convert the integer codepoint to a unicode char and add to string

                            if (0xD800 <= codePoint && codePoint <= 0xDBFF)  // if high surrogate
                            {
                                index += 4; // skip 4 chars
                                remainingLength = json.Length - index;
                                if (remainingLength >= 6)
                                {
                                    uint lowCodePoint;
                                    if (new string(json, index, 2) == "\\u" &&
                                        UInt32.TryParse(new string(json, index + 2, 4), NumberStyles.HexNumber,
                                                        CultureInfo.InvariantCulture, out lowCodePoint))
                                    {
                                        if (0xDC00 <= lowCodePoint && lowCodePoint <= 0xDFFF)    // if low surrogate
                                        {
                                            s.Append((char)codePoint);
                                            s.Append((char)lowCodePoint);
                                            index += 6; // skip 6 chars
                                            continue;
                                        }
                                    }
                                }
                                success = false;    // invalid surrogate pair
                                return "";
                            }
#if SILVERLIGHT
                            s.Append(ConvertFromUtf32((int)codePoint));
#else
                            s.Append(Char.ConvertFromUtf32((int)codePoint));
#endif
                            // skip 4 chars
                            index += 4;
                        }
                        else
                            break;
                    }
                }
                else
                    s.Append(c);
            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return s.ToString();
        }

#if SILVERLIGHT
        private static string ConvertFromUtf32(int utf32)
        {
            // http://www.java2s.com/Open-Source/CSharp/2.6.4-mono-.net-core/System/System/Char.cs.htm
            if (utf32 < 0 || utf32 > 0x10FFFF)
                throw new ArgumentOutOfRangeException("utf32", "The argument must be from 0 to 0x10FFFF.");
            if (0xD800 <= utf32 && utf32 <= 0xDFFF)
                throw new ArgumentOutOfRangeException("utf32", "The argument must not be in surrogate pair range.");
            if (utf32 < 0x10000)
                return new string((char)utf32, 1);
            utf32 -= 0x10000;
            return new string(new char[] {(char) ((utf32 >> 10) + 0xD800),(char) (utf32 % 0x0400 + 0xDC00)});
        }
#endif

        protected static object ParseNumber(char[] json, ref int index, ref bool success)
        {
            EatWhitespace(json, ref index);

            int lastIndex = GetLastIndexOfNumber(json, index);
            int charLength = (lastIndex - index) + 1;

            object returnNumber;
            string str = new string(json, index, charLength);
            if (str.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || str.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1)
            {
                double number;
                success = double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);
                returnNumber = number;
            }
            else
            {
                long number;
                success = long.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);
                returnNumber = number;
            }

            index = lastIndex + 1;
            return returnNumber;
        }

        protected static int GetLastIndexOfNumber(char[] json, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++)
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1) break;
            return lastIndex - 1;
        }

        protected static void EatWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index++)
                if (" \t\n\r\b\f".IndexOf(json[index]) == -1) break;
        }

        protected static int LookAhead(char[] json, int index)
        {
            int saveIndex = index;
            return NextToken(json, ref saveIndex);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected static int NextToken(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);

            if (index == json.Length)
                return TOKEN_NONE;

            char c = json[index];
            index++;
            switch (c)
            {
                case '{':
                    return TOKEN_CURLY_OPEN;
                case '}':
                    return TOKEN_CURLY_CLOSE;
                case '[':
                    return TOKEN_SQUARED_OPEN;
                case ']':
                    return TOKEN_SQUARED_CLOSE;
                case ',':
                    return TOKEN_COMMA;
                case '"':
                    return TOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return TOKEN_NUMBER;
                case ':':
                    return TOKEN_COLON;
            }
            index--;

            int remainingLength = json.Length - index;

            // false
            if (remainingLength >= 5)
            {
                if (json[index] == 'f' &&
                    json[index + 1] == 'a' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 's' &&
                    json[index + 4] == 'e')
                {
                    index += 5;
                    return TOKEN_FALSE;
                }
            }

            // true
            if (remainingLength >= 4)
            {
                if (json[index] == 't' &&
                    json[index + 1] == 'r' &&
                    json[index + 2] == 'u' &&
                    json[index + 3] == 'e')
                {
                    index += 4;
                    return TOKEN_TRUE;
                }
            }

            // null
            if (remainingLength >= 4)
            {
                if (json[index] == 'n' &&
                    json[index + 1] == 'u' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 'l')
                {
                    index += 4;
                    return TOKEN_NULL;
                }
            }

            return TOKEN_NONE;
        }

		static public bool GetGenericIDicKV(object obj, out IEnumerable keys, out IEnumerable values)
		{
			if (obj == null)
				goto setnullandreturn;
			Type tt = obj.GetType();
			if (!tt.IsGenericType)
				goto setnullandreturn;

			Type[] its = obj.GetType().GetInterfaces();
			Type dicinterface = typeof(IDictionary<,>);
			foreach (Type intf in its)
			{
				if (intf.IsGenericType && intf.GetGenericTypeDefinition() == dicinterface)
				{
					keys = (IEnumerable)intf.GetProperty("Keys").GetValue(obj);
					values = (IEnumerable)intf.GetProperty("Values").GetValue(obj);
					return true;
				}
			}
	setnullandreturn:
			keys = null;
			values = null;
			return false;
		}		

		protected static bool SerializeObject(IJsonSerializerStrategy jsonSerializerStrategy, object value, StringBuilder builder)
        {
            bool success = true;

            if (value is string)
                return SerializeString((string)value, builder);

			IEnumerable keys;
			IEnumerable values;
			bool isGenericIDic = GetGenericIDicKV(value, out keys, out values );
			if (isGenericIDic)
			{
				success = SerializeMap(jsonSerializerStrategy, keys, values, builder);
			}
			else if (value is IDictionary)
			{
				IDictionary dict = (IDictionary)value;
				success = SerializeMap(jsonSerializerStrategy, dict.Keys, dict.Values, builder);
			}
			// else if (value is IDictionary<string, object>)
   //         {
   //             IDictionary<string, object> dict = (IDictionary<string, object>)value;
   //             success = SerializeMap(jsonSerializerStrategy, dict.Keys, dict.Values, builder);
   //         }
   //         else if (value is IDictionary<string, string>)
   //         {
   //             IDictionary<string, string> dict = (IDictionary<string, string>)value;
   //             success = SerializeMap(jsonSerializerStrategy, dict.Keys, dict.Values, builder);
   //         }
            else if (value is IEnumerable)
                success = SerializeArray(jsonSerializerStrategy, (IEnumerable)value, builder);
            else if (IsNumeric(value))
                success = SerializeNumber(value, builder);
            else if (value is Boolean)
                builder.Append((bool)value ? "true" : "false");
            else if (value == null)
                builder.Append("null");
            else
            {
                object json;
				// first turn it into an json object
                success = jsonSerializerStrategy.SerializeNonPrimitiveObject(value, out json);
                if (success)
					success = SerializeObject(jsonSerializerStrategy, json, builder);
            }

            return success;
        }

        protected static bool SerializeMap(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable keys, IEnumerable values, StringBuilder builder)
        {
            builder.Append("{\r\n");

            IEnumerator ke = keys.GetEnumerator();
            IEnumerator ve = values.GetEnumerator();

            bool first = true;
            while (ke.MoveNext() && ve.MoveNext())
            {
                object key = ke.Current;
                object value = ve.Current;

                if (!first)
                    builder.Append(",\r\n");

                if (key is string)
                    SerializeString((string)key, builder);
                else
                    if (!SerializeObject(jsonSerializerStrategy, value, builder)) return false;

                builder.Append(" : ");
                if (!SerializeObject(jsonSerializerStrategy, value, builder))
                    return false;

                first = false;
            }

            builder.Append("}\r\n");
            return true;
        }

        protected static bool SerializeArray(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable anArray, StringBuilder builder)
        {
            builder.Append("[\r\n  ");

            bool first = true;
            lock(anArray)
            foreach (object value in anArray)
            {
                if (!first)
                    builder.Append(",\r\n  ");

                if (!SerializeObject(jsonSerializerStrategy, value, builder))
                    return false;

                first = false;
            }

            builder.Append("\r\n]");
            return true;
        }

        protected static bool SerializeString(string aString, StringBuilder builder)
        {
            builder.Append("\"");

            char[] charArray = aString.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"')
                    builder.Append("\\\"");
                else if (c == '\\')
                    builder.Append("\\\\");
                else if (c == '\b')
                    builder.Append("\\b");
                else if (c == '\f')
                    builder.Append("\\f");
                else if (c == '\n')
                    builder.Append("\\n");
                else if (c == '\r')
                    builder.Append("\\r");
                else if (c == '\t')
                    builder.Append("\\t");
                else
                    builder.Append(c);
            }

            builder.Append("\"");
            return true;
        }

        protected static bool SerializeNumber(object number, StringBuilder builder)
        {
            if (number is long)
            {
                builder.Append(((long)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is ulong)
            {
                builder.Append(((ulong)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is int)
            {
                builder.Append(((int)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is uint)
            {
                builder.Append(((uint)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is decimal)
            {
                builder.Append(((decimal)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is float)
            {
                builder.Append(((float)number).ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture));
            }

            return true;
        }

        /// <summary>
        /// Determines if a given object is numeric in any way
        /// (can be integer, double, null, etc).
        /// </summary>
        protected static bool IsNumeric(object value)
        {
            if (value is sbyte) return true;
            if (value is byte) return true;
            if (value is short) return true;
            if (value is ushort) return true;
            if (value is int) return true;
            if (value is uint) return true;
            if (value is long) return true;
            if (value is ulong) return true;
            if (value is float) return true;
            if (value is double) return true;
            if (value is decimal) return true;
            return false;
        }

        private static IJsonSerializerStrategy currentJsonSerializerStrategy;
        public static IJsonSerializerStrategy CurrentJsonSerializerStrategy
        {
            get
            {
                return currentJsonSerializerStrategy ??
                    (currentJsonSerializerStrategy =
#if SIMPLE_JSON_DATACONTRACT
 DataContractJsonSerializerStrategy
#else
 PocoJsonSerializerStrategy
#endif
);
            }

            set
            {
                currentJsonSerializerStrategy = value;
            }
        }

        private static PocoJsonSerializerStrategy pocoJsonSerializerStrategy;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static PocoJsonSerializerStrategy PocoJsonSerializerStrategy
        {
            get
            {
                // todo: implement locking mechanism.
                return pocoJsonSerializerStrategy ?? (pocoJsonSerializerStrategy = new PocoJsonSerializerStrategy());
            }
        }

#if SIMPLE_JSON_DATACONTRACT

        private static DataContractJsonSerializerStrategy dataContractJsonSerializerStrategy;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static DataContractJsonSerializerStrategy DataContractJsonSerializerStrategy
        {
            get
            {
                return dataContractJsonSerializerStrategy ?? (dataContractJsonSerializerStrategy = new DataContractJsonSerializerStrategy());
            }
        }

#endif
    }

    #endregion

    #region Simple Json Serializer Strategies

#if SIMPLE_JSON_INTERNAL
    internal
#else
    public
#endif
 interface IJsonSerializerStrategy
    {
        bool SerializeNonPrimitiveObject(object input, out object output);

        /// <summary>
        /// retrieve all propertyinfo and fieldinfo from type, add a MemberMap for each one.
        /// </summary>
		void BuildMap(Type type, SafeDictionary<string, CacheResolver.MemberMap> memberMaps);

		/// <summary>
		/// convert obj of simple type, or list, or dictionary or a hiarachy of these to its final type
		/// </summary>
		object DeserializeObject(object json, Type type);
    }

#if SIMPLE_JSON_INTERNAL
    internal
#else
    public
#endif
 class PocoJsonSerializerStrategy : IJsonSerializerStrategy
    {
        internal CacheResolver _CacheResolver;

        //private static readonly string[] Iso8601Format = new string[]
        //                                                     {
        //                                                         @"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z",
        //                                                         @"yyyy-MM-dd\THH:mm:ss\Z",
        //                                                         @"yyyy-MM-dd\THH:mm:ssK"
        //                                                     };

        public PocoJsonSerializerStrategy()
        {
            _CacheResolver = new CacheResolver(BuildMap, this);
        }

		public virtual void BuildMap(Type type, SafeDictionary<string, CacheResolver.MemberMap> memberMaps)
        {
#if NETFX_CORE
            foreach (PropertyInfo info in type.GetTypeInfo().DeclaredProperties) {
                var getMethod = info.GetMethod;
                if(getMethod==null || !getMethod.IsPublic || getMethod.IsStatic) continue;
#else
            foreach (PropertyInfo info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
#endif
                memberMaps.Add(info.Name, new CacheResolver.MemberMap(info));
            }
#if NETFX_CORE
            foreach (FieldInfo info in type.GetTypeInfo().DeclaredFields) {
                if(!info.IsPublic || info.IsStatic) continue;
#else
            foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
#endif
				if (info.Name.EndsWith("__BackingField", StringComparison.InvariantCultureIgnoreCase))
					continue;
				memberMaps.Add(info.Name, new CacheResolver.MemberMap(info));
            }
        }

        public virtual bool SerializeNonPrimitiveObject(object input, out object output)
        {
            return TrySerializeKnownTypes(input, out output) || TrySerializeUnknownTypes(input, out output);
        }

		/// <summary>
		/// convert obj of simple type, or list, or dictionary or a hiarachy of these to its final type
		/// </summary>
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public virtual object DeserializeObject(object json, Type type)
        {
            object obj = null;
            if (json is string)
            {
                string str = json as string;

                if (!string.IsNullOrEmpty(str))
                {
                    if (type == typeof(DateTime) || (ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(DateTime)))
                        obj = DateTime.ParseExact(str, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToLocalTime();
                    else if (type == typeof(Guid) || (ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(Guid)))
                        obj = new Guid(str);
					else
					{
						JsonTypeConverter converter = _CacheResolver.LoadTypeConverter(type);
						obj = converter.Convert2Obj(str, type);
					}                        
                }
                else
                {
                    if (type == typeof(Guid))
                        obj= default(Guid);
                    else if(ReflectionUtils.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(Guid))
                        obj = null;
                    else
                        obj = str;                    
                }
            }
            else if (json is bool)
                obj = json;
            else if (json == null)
			{
				if (type == typeof(Guid))
					obj = default(Guid);				
			}
			else if ((json is long && type == typeof(long)) || (json is double && type == typeof(double)))
                obj = json;
            else if ((json is double && type != typeof(double)) || (json is long && type != typeof(long)))
            {
				if (type.IsEnum)
					obj = Enum.ToObject(type, json);
				else
					obj =
#if NETFX_CORE
                    type == typeof(int) || type == typeof(long) || type == typeof(double) ||type == typeof(float) || type == typeof(bool) || type == typeof(decimal) ||type == typeof(byte) || type == typeof(short)
#else
					typeof(IConvertible).IsAssignableFrom(type)
#endif
					 ? Convert.ChangeType(json, type, CultureInfo.InvariantCulture) : json;
            }
            else
            {
                if (json is IDictionary<string, object>)
                {
                    IDictionary<string, object> jsonObject = (IDictionary<string, object>)json;

                    if (ReflectionUtils.IsTypeDictionary(type))
                    {
						// if dictionary then
#if NETFX_CORE
						Type keyType = type.GetTypeInfo().GenericTypeArguments[0];
						Type valueType = type.GetTypeInfo().GenericTypeArguments[1];
#else
						Type[] targs = type.GetGenericArguments();
                        Type keyType = targs[0];
                        Type valueType = targs[1];
#endif

                        Type genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

						JsonTypeConverter converter = _CacheResolver.LoadTypeConverter(genericType);
						IDictionary dict = (IDictionary)converter.Convert2Dict(jsonObject, genericType, valueType);                        

                        obj = dict;
                    }
                    else // type is a custom type
                    {
						if (type == typeof(object))
							return json;

						JsonTypeConverter converter = _CacheResolver.LoadTypeConverter(type);
						obj = converter.Convert2Obj(jsonObject, type);
					}
                }
                else if (json is IList<object>)
                {
                    IList<object> jsonObject = (IList<object>)json;
                    IList list = null;

                    if (type.IsArray)
                    {
                        list = (IList)Activator.CreateInstance(type, jsonObject.Count);
                        int i = 0;
                        foreach (object o in jsonObject)
                            list[i++] = DeserializeObject(o, type.GetElementType());
                    }
                    else if (ReflectionUtils.IsTypeGenericeCollectionInterface(type) ||
#if NETFX_CORE
					typeof(IList).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
#else
					typeof(IList).IsAssignableFrom(type))
#endif					
                    {
#if NETFX_CORE
						Type innerType = type.GetTypeInfo().GenericTypeArguments[0];
#else
                        Type innerType = type.GetGenericArguments()[0];
#endif
                        Type genericType = typeof(List<>).MakeGenericType(innerType);

                        list = (IList)Activator.CreateInstance(genericType);

                        foreach (object o in jsonObject)
                            list.Add(DeserializeObject(o, innerType));
                    }

                    obj = list;
                }

                return obj;
            }

            if (ReflectionUtils.IsNullableType(type))
                return ReflectionUtils.ToNullableType(obj, type);

            return obj;
        }

        protected virtual object SerializeEnum(Enum p)
        {
            return Convert.ToDouble(p, CultureInfo.InvariantCulture);
        }

        protected virtual bool TrySerializeKnownTypes(object input, out object output)
        {
            bool returnValue = true;
            if (input is DateTime)
                output = ((DateTime)input).ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
            else if (input is Guid)
                output = ((Guid)input).ToString("D");
            else if (input is Uri)
                output = input.ToString();
            else if (input is Enum)
                output = SerializeEnum((Enum)input);
            else
            {
                returnValue = false;
                output = null;
            }

            return returnValue;
        }

        protected virtual bool TrySerializeUnknownTypes(object input, out object output)
        {        
            Type type = input.GetType();

			if (type.FullName == null)
			{
				output = null;
				return false;
			}

            JsonTypeConverter converter = _CacheResolver.LoadTypeConverter(type);
			output = converter.Serialize2Json(input);
            return output != null;
        }
    }

#if SIMPLE_JSON_DATACONTRACT
#if SIMPLE_JSON_INTERNAL
    internal
#else
    public
#endif
 class DataContractJsonSerializerStrategy : PocoJsonSerializerStrategy
    {
        public DataContractJsonSerializerStrategy()
        {
            _CacheResolver = new CacheResolver(BuildMap, this);
        }

        public override void BuildMap(Type type, SafeDictionary<string, CacheResolver.MemberMap> map)
        {
            bool hasDataContract = ReflectionUtils.GetAttribute(type, typeof(DataContractAttribute)) != null;
            if (!hasDataContract)
            {
                base.BuildMap(type, map);
                return;
            }

            string jsonKey;
#if NETFX_CORE
            foreach (PropertyInfo info in type.GetTypeInfo().DeclaredProperties)
#else
            foreach (PropertyInfo info in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
#endif
            {
                if (CanAdd(info, out jsonKey))
                    map.Add(jsonKey, new CacheResolver.MemberMap(info));
            }

#if NETFX_CORE
            foreach (FieldInfo info in type.GetTypeInfo().DeclaredFields)
#else
            foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
#endif
            {
				if (CanAdd(info, out jsonKey))
				{
					if (jsonKey.EndsWith("__BackingField", StringComparison.InvariantCultureIgnoreCase))
						continue;
					map.Add(jsonKey, new CacheResolver.MemberMap(info));
				}
            }
        }

        private static bool CanAdd(MemberInfo info, out string jsonKey)
        {
            jsonKey = null;

            if (ReflectionUtils.GetAttribute(info, typeof(IgnoreDataMemberAttribute)) != null)
                return false;

            DataMemberAttribute dataMemberAttribute = (DataMemberAttribute)ReflectionUtils.GetAttribute(info, typeof(DataMemberAttribute));

            jsonKey = string.IsNullOrEmpty(dataMemberAttribute?.Name) ? info.Name : dataMemberAttribute.Name;
            return true;
        }
    }
#endif

    #endregion

    #region Reflection helpers

    namespace Reflection
    {
#if SIMPLE_JSON_INTERNAL
		internal
#else
        public
#endif
 class ReflectionUtils
        {
            public static Attribute GetAttribute(MemberInfo info, Type attributeType)
            {
#if NETFX_CORE
                if (info == null || attributeType == null || !info.IsDefined(attributeType))
                    return null;
                return info.GetCustomAttribute(attributeType);
#else
                if (info == null || attributeType == null || !Attribute.IsDefined(info, attributeType))
                    return null;
                return Attribute.GetCustomAttribute(info, attributeType);
#endif
            }

            public static Attribute GetAttribute(Type objectType, Type attributeType)
            {

#if NETFX_CORE
                if (objectType == null || attributeType == null || !objectType.GetTypeInfo().IsDefined(attributeType))
                    return null;
                return objectType.GetTypeInfo().GetCustomAttribute(attributeType);
#else
                if (objectType == null || attributeType == null || !Attribute.IsDefined(objectType, attributeType))
                    return null;
                return Attribute.GetCustomAttribute(objectType, attributeType);
#endif
            }

            public static bool IsTypeGenericeCollectionInterface(Type type)
            {
#if NETFX_CORE
                if (!type.GetTypeInfo().IsGenericType)
#else
                if (!type.IsGenericType)
#endif
                    return false;

                Type genericDefinition = type.GetGenericTypeDefinition();

                return (genericDefinition == typeof(IList<>) || genericDefinition == typeof(ICollection<>) || genericDefinition == typeof(IEnumerable<>));
            }

            public static bool IsTypeDictionary(Type type)
            {
#if NETFX_CORE
                if (typeof(IDictionary<,>).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                    return true;

                if (!type.GetTypeInfo().IsGenericType)
                    return false;
#else
                if (typeof(IDictionary).IsAssignableFrom(type))
                    return true;

                if (!type.IsGenericType)
                    return false;
#endif
                Type genericDefinition = type.GetGenericTypeDefinition();
                return genericDefinition == typeof(IDictionary<,>);
            }

            public static bool IsNullableType(Type type)
            {
                return
#if NETFX_CORE
                    type.GetTypeInfo().IsGenericType
#else
					type.IsGenericType
#endif
					&& type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            /// <summary>
            /// convert obj to nullableBaseType
            /// </summary>            
            public static object ToNullableType(object obj, Type nullableBaseType)
            {
                return obj == null ? null : Convert.ChangeType(obj, Nullable.GetUnderlyingType(nullableBaseType), CultureInfo.InvariantCulture);
            }
        }

#if SIMPLE_JSON_INTERNAL
    internal
#else
        public
#endif
 delegate object GetHandler(object source);

#if SIMPLE_JSON_INTERNAL
    internal
#else
        public
#endif
 delegate void SetHandler(object source, object value);

#if SIMPLE_JSON_INTERNAL
    internal
#else
        public
#endif
 delegate void MemberMapLoader(Type type, SafeDictionary<string, CacheResolver.MemberMap> memberMaps);


        /// <summary>
        /// cache for JsonTypeConverter
        /// </summary>
#if SIMPLE_JSON_INTERNAL
		internal
#else
        public
#endif
 class CacheResolver
        {
            // private readonly MemberMapLoader _memberMapLoader;
			private IJsonSerializerStrategy _parentStg;

            private readonly SafeDictionary<Type, 
				JsonTypeConverter> _ConverterCache = new SafeDictionary<Type, JsonTypeConverter>();

            /// <summary>
            /// memberMapLoader not used.            
            /// </summary>
			public CacheResolver(MemberMapLoader memberMapLoader, IJsonSerializerStrategy parent)
            {
				_parentStg = parent;
				//_memberMapLoader = memberMapLoader;
            }

            /// <summary>
            /// return the JsonTypeConverter for type
            /// </summary>
            internal JsonTypeConverter LoadTypeConverter(Type type)
            {                
                JsonTypeConverter converter;
				if (!_ConverterCache.TryGetValue(type, out converter))
				{
					converter = new JsonTypeConverter(_parentStg);
					_ConverterCache.Add(type, converter);
				}

				return converter;                                
            }

#if SIMPLE_JSON_REFLECTIONEMIT
            static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
            {
                DynamicMethod dynamicMethod = !owner.IsInterface
                  ? new DynamicMethod(name, returnType, parameterTypes, owner, true)
                  : new DynamicMethod(name, returnType, parameterTypes, (Module)null, true);

                return dynamicMethod;
            }
#endif

			static GetHandler CreateGetHandler(FieldInfo fieldInfo)
            {
#if SIMPLE_JSON_REFLECTIONEMIT
                Type type = fieldInfo.FieldType;
                DynamicMethod dynamicGet = CreateDynamicMethod("Get" + fieldInfo.Name, fieldInfo.DeclaringType, new Type[] { typeof(object) }, fieldInfo.DeclaringType);
                ILGenerator getGenerator = dynamicGet.GetILGenerator();

                getGenerator.Emit(OpCodes.Ldarg_0);
                getGenerator.Emit(OpCodes.Ldfld, fieldInfo);
                if (type.IsValueType)
                    getGenerator.Emit(OpCodes.Box, type);
                getGenerator.Emit(OpCodes.Ret);

                return (GetHandler)dynamicGet.CreateDelegate(typeof(GetHandler));
#else
                return delegate(object instance) { return fieldInfo.GetValue(instance); };
#endif
            }

            static SetHandler CreateSetHandler(FieldInfo fieldInfo)
            {
                if (fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
                    return null;
#if SIMPLE_JSON_REFLECTIONEMIT
                Type type = fieldInfo.FieldType;
                DynamicMethod dynamicSet = CreateDynamicMethod("Set" + fieldInfo.Name, null, new Type[] { typeof(object), typeof(object) }, fieldInfo.DeclaringType);
                ILGenerator setGenerator = dynamicSet.GetILGenerator();

                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);
                if (type.IsValueType)
                    setGenerator.Emit(OpCodes.Unbox_Any, type);
                setGenerator.Emit(OpCodes.Stfld, fieldInfo);
                setGenerator.Emit(OpCodes.Ret);

                return (SetHandler)dynamicSet.CreateDelegate(typeof(SetHandler));
#else
                return delegate(object instance, object value) { fieldInfo.SetValue(instance, value); };
#endif
            }

            static GetHandler CreateGetHandler(PropertyInfo propertyInfo)
            {
#if NETFX_CORE
                MethodInfo getMethodInfo = propertyInfo.GetMethod;
#else
                MethodInfo getMethodInfo = propertyInfo.GetGetMethod(true);
#endif
                if (getMethodInfo == null)
                    return null;
#if SIMPLE_JSON_REFLECTIONEMIT
                Type type = propertyInfo.PropertyType;
                DynamicMethod dynamicGet = CreateDynamicMethod("Get" + propertyInfo.Name, propertyInfo.DeclaringType, new Type[] { typeof(object) }, propertyInfo.DeclaringType);
                ILGenerator getGenerator = dynamicGet.GetILGenerator();

                getGenerator.Emit(OpCodes.Ldarg_0);
                getGenerator.Emit(OpCodes.Call, getMethodInfo);
                if (type.IsValueType)
                    getGenerator.Emit(OpCodes.Box, type);
                getGenerator.Emit(OpCodes.Ret);

                return (GetHandler)dynamicGet.CreateDelegate(typeof(GetHandler));
#else
#if NETFX_CORE
                return delegate(object instance) { return getMethodInfo.Invoke(instance, new Type[] { }); };
#else
                return delegate(object instance) { return getMethodInfo.Invoke(instance, Type.EmptyTypes); };
#endif
#endif
            }

            static SetHandler CreateSetHandler(PropertyInfo propertyInfo)
            {
#if NETFX_CORE
                MethodInfo setMethodInfo = propertyInfo.SetMethod;
#else
                MethodInfo setMethodInfo = propertyInfo.GetSetMethod(true);
#endif
                if (setMethodInfo == null)
                    return null;
#if SIMPLE_JSON_REFLECTIONEMIT
                Type type = propertyInfo.PropertyType;
                DynamicMethod dynamicSet = CreateDynamicMethod("Set" + propertyInfo.Name, null, new Type[] { typeof(object), typeof(object) }, propertyInfo.DeclaringType);
                ILGenerator setGenerator = dynamicSet.GetILGenerator();

                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);
                if (type.IsValueType)
                    setGenerator.Emit(OpCodes.Unbox_Any, type);
                setGenerator.Emit(OpCodes.Call, setMethodInfo);
                setGenerator.Emit(OpCodes.Ret);
                return (SetHandler)dynamicSet.CreateDelegate(typeof(SetHandler));
#else
                return delegate(object instance, object value) { setMethodInfo.Invoke(instance, new[] { value }); };
#endif
            }

#if SIMPLE_JSON_INTERNAL
			internal
#else
            public
#endif
			sealed class MemberMap
            {
                public readonly MemberInfo MemberInfo;
                public readonly Type Type;
                public readonly GetHandler Getter;
                public readonly SetHandler Setter;

                public MemberMap(PropertyInfo propertyInfo)
                {
                    MemberInfo = propertyInfo;
                    Type = propertyInfo.PropertyType;
                    Getter = CreateGetHandler(propertyInfo);
                    Setter = CreateSetHandler(propertyInfo);
                }

                public MemberMap(FieldInfo fieldInfo)
                {
                    MemberInfo = fieldInfo;
                    Type = fieldInfo.FieldType;
                    Getter = CreateGetHandler(fieldInfo);
                    Setter = CreateSetHandler(fieldInfo);
                }
            }
        }

#if SIMPLE_JSON_INTERNAL
		internal
#else
		public
#endif
		class SafeDictionary<TKey, TValue>
        {
            private readonly object _padlock = new object();
            private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public TValue this[TKey key]
            {
                get { return _dictionary[key]; }
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).GetEnumerator();
            }

            public void Add(TKey key, TValue value)
            {
                lock (_padlock)
                {
                    if (_dictionary.ContainsKey(key) == false)
                        _dictionary.Add(key, value);
                }
            }
        }
    }

    #endregion
}