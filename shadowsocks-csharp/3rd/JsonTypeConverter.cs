using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace SimpleJson.Reflection
{
	internal delegate object CtorDelegate();

	internal delegate object CtorObjDelegate(params object[] args);
		
	internal class JsonTypeConverter
	{
		// Fields
		private SerializeApproach _Approach;
		private bool _ApproachSet;
		private CtorDelegate _Ctor0;
		private CtorObjDelegate _CtorSerialize;
		private bool _MapLoaded;
		private SafeDictionary<string, CacheResolver.MemberMap> _Maps;
		private IJsonSerializerStrategy _stg;

		// Methods
		internal JsonTypeConverter(IJsonSerializerStrategy stg)
		{
			this._stg = stg;
		}

		internal object Convert2Dict(IDictionary<string, object> json, Type type, Type valueType)
		{
			IDictionary dict = (IDictionary)GetCtor(type)();
			foreach (KeyValuePair<string, object> kvp in json)
			{
				dict.Add(kvp.Key, this._stg.DeserializeObject(kvp.Value, valueType));
			}
			return dict;
		}

		internal object Convert2Obj(IDictionary<string, object> json, Type type)
		{
			object obj = null;
			this.GetOrSetApproach(type);
			if (this._Approach == SerializeApproach.UseConverter)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(type);
				PropertyDescriptorCollection props = TypeDescriptor.GetProperties(type);
				Dictionary<string, object> objDic = new Dictionary<string, object>(json.Count);
				foreach (KeyValuePair<string, object> item in json)
				{
					Type itemType = props[item.Key].PropertyType;
					objDic.Add(item.Key, this._stg.DeserializeObject(item.Value, itemType));
				}
				return converter.CreateInstance(objDic);
			}
			if (this._Approach == SerializeApproach.UseISerializable)
			{
				FormatterConverter converter = new FormatterConverter();
				SerializationInfo info = new SerializationInfo(type, converter);
				StreamingContext ct = new StreamingContext(StreamingContextStates.Persistence);
				PropertyDescriptorCollection props = TypeDescriptor.GetProperties(type);
				foreach (KeyValuePair<string, object> item in json)
				{
					Type itemType = props[item.Key].PropertyType;
					info.AddValue(item.Key, this._stg.DeserializeObject(item.Value, itemType));
				}
				object[] args = new object[] { info, ct };
				return this._CtorSerialize(args);
			}
			if (this._Approach != SerializeApproach.UseConstructor)
			{
				throw new SerializationException(type.Name + " cann't be serialized, It must either have an TypeConverter, or implement ISerialize, or have a default constructor");
			}
			obj = this._Ctor0();
			this.GetOrLoadMap(type);
			foreach (KeyValuePair<string, CacheResolver.MemberMap> keyValuePair in this._Maps)
			{
				CacheResolver.MemberMap v = keyValuePair.Value;
				if (v.Setter != null)
				{
					string jsonKey = keyValuePair.Key;
					if (json.ContainsKey(jsonKey))
					{
						object value = this._stg.DeserializeObject(json[jsonKey], v.Type);
						v.Setter(obj, value);
					}
				}
			}
			return obj;
		}

		internal object Convert2Obj(string input, Type type)
		{
			this.GetOrSetApproach(type);
			if ((this._Approach == SerializeApproach.UseString) && (typeof(string) != type))
			{
				return TypeDescriptor.GetConverter(type).ConvertFrom(input);
			}
			return input;
		}

		private static SerializeApproach FindApproach(Type type)
		{
			SerializeApproach c = SerializeApproach.UseNone;
			TypeConverter converter = TypeDescriptor.GetConverter(type);
			if ((converter != null) && !typeof(TypeConverter).Equals(converter.GetType()))
			{
				if (converter.GetPropertiesSupported() && converter.GetCreateInstanceSupported())
				{
					return SerializeApproach.UseConverter;
				}
				if (!converter.CanConvertFrom(typeof(string)))
				{
					throw new SerializationException(type.Name + "'s TypeConverter is not suitable for serialization.");
				}
				return SerializeApproach.UseString;
			}
			if (typeof(ISerializable).IsAssignableFrom(type))
			{
				Type[] types = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
				if (type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, types, null) != null)
				{
					c = SerializeApproach.UseISerializable;
				}
				return c;
			}
			if (type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) != null)
			{
				c = SerializeApproach.UseConstructor;
			}
			return c;
		}

		public static CtorDelegate GetCtor(Type type)
		{
			ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
			if (constructorInfo == null)
			{
				throw new SerializationException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Could not get constructor for {0}.", type));
			}
			return delegate {
				return constructorInfo.Invoke(null);
			};
		}

		private SafeDictionary<string, CacheResolver.MemberMap> GetOrLoadMap(Type type)
		{
			if (!this._MapLoaded)
			{
				this._Maps = new SafeDictionary<string, CacheResolver.MemberMap>();
				this._stg.BuildMap(type, this._Maps);
				this._MapLoaded = true;
			}
			return this._Maps;
		}

		private SerializeApproach GetOrSetApproach(Type type)
		{
			if (!this._ApproachSet)
			{
				this._Approach = FindApproach(type);
				if (this._Approach == SerializeApproach.UseConstructor)
				{
					this._Ctor0 = GetCtor(type);
				}
				else if (this._Approach == SerializeApproach.UseISerializable)
				{
					Type[] types = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
					ConstructorInfo method = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, types, null);
					if (method != null)
					{
						this._CtorSerialize = delegate (object[] a) {
							return method.Invoke(a);
						};
					}
				}
				this._ApproachSet = true;
			}
			return this._Approach;
		}

		internal object Serialize2Json(object obj)
		{
			JsonObject json = new JsonObject();
			Type type = obj.GetType();
			this.GetOrSetApproach(type);
			if (this._Approach == SerializeApproach.UseConverter)
			{
				foreach (PropertyDescriptor item in TypeDescriptor.GetConverter(type).GetProperties(obj))
				{
					json.Add(item.Name, item.GetValue(obj));
				}
				return json;
			}
			if (this._Approach == SerializeApproach.UseString)
			{
				return TypeDescriptor.GetConverter(type).ConvertTo(obj, typeof(string));
			}
			if (this._Approach == SerializeApproach.UseISerializable)
			{
				FormatterConverter converter = new FormatterConverter();
				SerializationInfo info = new SerializationInfo(type, converter);
				StreamingContext ct = new StreamingContext(StreamingContextStates.Persistence);
				((ISerializable)obj).GetObjectData(info, ct);
				SerializationInfoEnumerator raws = info.GetEnumerator();
				while (raws.MoveNext())
				{
					json.Add(raws.Name, raws.Value);
				}
				return json;
			}
			if (this._Approach != SerializeApproach.UseConstructor)
			{
				throw new SerializationException(type.Name + " cann't be serialized, It must either have an TypeConverter, or implement ISerialize, or have a default constructor");
			}
			this.GetOrLoadMap(type);
			foreach (KeyValuePair<string, CacheResolver.MemberMap> keyValuePair in this._Maps)
			{
				if (keyValuePair.Value.Getter != null)
				{
					json.Add(keyValuePair.Key, keyValuePair.Value.Getter(obj));
				}
			}
			return json;
		}
	}

	public enum SerializeApproach
	{
		UseNone,
		UseConstructor,
		UseConverter,
		UseString,
		UseISerializable
	}
}

