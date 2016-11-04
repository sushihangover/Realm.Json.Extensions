using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using Newtonsoft.Json;
using Realms;
using System.Linq.Expressions;
using System.Diagnostics.Contracts;
using System.Text;

[assembly: InternalsVisibleTo("RealmJson.Test")]

namespace RealmJson.Extensions
{
	/// <summary>
	/// .Net Realm does json.
	/// </summary>
	public static class RealmDoesJson
	{
		/// <summary>
		/// Malformed json exception message.
		/// </summary>
		public const string ExMalFormeJsonMessage = "Malformed Json";

		/// <summary>
		/// Creates a single RealmObject from a json string.
		/// </summary>
		/// <returns></returns>
		/// <param name="realm">Realm Instance</param>
		/// <param name="jsonString">Json string</param>
		/// <typeparam name="T">RealmOject-based Class..</typeparam>
		public static T CreateObjectFromJson<T>(this Realm realm, string jsonString) where T : RealmObject
		{
			Mapper.Initialize(cfg => { cfg.CreateMap<T, T>(); });

			var jsonObject = JsonConvert.DeserializeObject<T>(jsonString);
			T realmObject = null;
			realm.Write(() =>
			{
				realmObject = realm.CreateObject(typeof(T).Name);
				Mapper.Map<T, T>(jsonObject, realmObject);
			});
			return realmObject;
		}

		/// <summary>
		/// Creates the single RealmObject from a stream containing json.
		/// </summary>
		/// <returns>The object from json.</returns>
		/// <param name="realm">Realm Instance.</param>
		/// <param name="stream">Stream.</param>
		/// <typeparam name="T">RealmObject-based Class.</typeparam>
		public static T CreateObjectFromJson<T>(this Realm realm, Stream stream) where T : RealmObject
		{
			Mapper.Initialize(cfg => { cfg.CreateMap<T, T>(); });

			using (var streamReader = new StreamReader(stream))
			using (var jsonTextReader = new JsonTextReader(streamReader))
			{
				var serializer = new JsonSerializer();
				if (!jsonTextReader.Read())
					throw new Exception(ExMalFormeJsonMessage);
				var jsonObject = serializer.Deserialize<T>(jsonTextReader);
				T realmObject = null;
				realm.Write(() =>
				{
					realmObject = realm.CreateObject(typeof(T).Name);
					Mapper.Map<T, T>(jsonObject, realmObject);
				});
				return realmObject;
			}
		}

		/// <summary>
		/// Creates or updates a single RealmObject from a json string.
		/// </summary>
		/// <returns></returns>
		/// <param name="realm">Realm Instance.</param>
		/// <param name="jsonString">Json string.</param>
		/// <typeparam name="T">RealmObject-based Class.</typeparam>
		public static T CreateOrUpdateObjectFromJson<T>(this Realm realm, string jsonString) where T : RealmObject
		{
			Contract.Ensures(Contract.Result<T>() != null);
			var jsonObject = JsonConvert.DeserializeObject<T>(jsonString);
			var pkProperty = typeof(T).GetRuntimeProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
			var realmObject = (T)FindByPKDynamic(realm, jsonObject.GetType(), pkProperty.GetValue(jsonObject), pkProperty.PropertyType != typeof(string));
			realm.Write(() =>
			{
				if (realmObject == null)
				{
					realmObject = realm.CreateObject(typeof(T).Name);
					Mapper.Initialize(cfg => { cfg.CreateMap<T, T>(); });
					Mapper.Map<T, T>(jsonObject, realmObject);
				}
				else
				{
					Mapper.Initialize(cfg => // Map all, except for the PK
					{
						cfg.CreateMap<T, T>().ForMember(pkProperty.Name, opt => opt.Ignore());
					});
					Mapper.Map<T, T>(jsonObject, realmObject);
				}
			});
			return realmObject;
		}

		static RealmObject FindByPKDynamic(Realm realm, Type type, object primaryKeyValue, bool isIntegerPK)
		{
			if (isIntegerPK)
			{
				long? castPKValue;
				if (primaryKeyValue == null)
				{
					castPKValue = null;
				}
				else
				{
					castPKValue = Convert.ToInt64(primaryKeyValue);
				}
				return realm.ObjectForPrimaryKey(type.Name, (long)castPKValue);
			}
			return realm.ObjectForPrimaryKey(type.Name, (string)primaryKeyValue);
		}

		/// <summary>
		/// Creates multiple RealmObjects from a json stream.
		/// </summary>
		/// <param name="realm">Realm Instance.</param>
		/// <param name="stream">Stream.</param>
		/// <typeparam name="T">RealmObject-based Class.</typeparam>
		public static void CreateAllFromJson<T>(this Realm realm, Stream stream) where T : RealmObject
		{
			Mapper.Initialize(cfg => { cfg.CreateMap<T, T>(); });

			using (var streamReader = new StreamReader(stream))
			using (var jsonTextReader = new JsonTextReader(streamReader))
			{
				var serializer = new JsonSerializer();
				// This initial Read() is required to alllow Andoird Asset Streams to work w/ Newtonsoft 9.0.?
				if (!jsonTextReader.Read() || jsonTextReader.TokenType != JsonToken.StartArray)
					throw new Exception("MALFORMED JSON, Start of Array missing");
				realm.Write(() =>
				{
					while (jsonTextReader.Read() & jsonTextReader.TokenType == JsonToken.StartObject & jsonTextReader.TokenType != JsonToken.EndArray)
					{
						//System.Diagnostics.Debug.WriteLine("Token: {0}, Value: {1}", jsonTextReader.TokenType, jsonTextReader.Value);
						var jsonObject = serializer.Deserialize<T>(jsonTextReader);
						var realmObject = realm.CreateObject(typeof(T).Name);
						Mapper.Map<T, T>(jsonObject, realmObject);
					}
				});
			}
		}

		/// <summary>
		/// Creates multiple RealmObjects from a json stream.
		/// </summary>
		/// <param name="realm">Realm Instance.</param>
		/// <param name="jsonString">Json string.</param>
		/// <typeparam name="T">RealmObject-based Class.</typeparam>
		public static void CreateAllFromJson<T>(this Realm realm, string jsonString) where T : RealmObject
		{
			Mapper.Initialize(cfg => { cfg.CreateMap<T, T>(); });

			var jsonList = JsonConvert.DeserializeObject<List<T>>(jsonString);
			realm.Write(() =>
			{
				foreach (var item in jsonList)
				{
					var realmObject = realm.CreateObject(typeof(T).Name);
					Mapper.Map<T, T>(item, realmObject);
				}
			});
		}

		static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(
			this IMappingExpression<TSource, TDestination> map, Expression<Func<TDestination, object>> selector)
		{
			map.ForMember(selector, config => config.Ignore());
			return map;
		}

		//void createOrUpdateAllFromJson(Class<E> clazz, InputStream in)
		//Tries to update a list of existing objects identified by their primary key with new JSON data.
		//<E extends RealmModel>
		//void createOrUpdateAllFromJson(Class<E> clazz, org.json.JSONArray json)
		//Tries to update a list of existing objects identified by their primary key with new JSON data.
		//<E extends RealmModel>
		//void createOrUpdateAllFromJson(Class<E> clazz, String json)
		//Tries to update a list of existing objects identified by their primary key with new JSON data.
		//<E extends RealmModel>

		//E createOrUpdateObjectFromJson(Class<E> clazz, InputStream in)
		//Tries to update an existing object defined by its primary key with new JSON data.
		//<E extends RealmModel>
		//E createOrUpdateObjectFromJson(Class<E> clazz, org.json.JSONObject json)
		//Tries to update an existing object defined by its primary key with new JSON data.
		//<E extends RealmModel>
		//E createOrUpdateObjectFromJson(Class<E> clazz, String json)
		//Tries to update an existing object defined by its primary key with new JSON data.

	}
}
