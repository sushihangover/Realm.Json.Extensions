using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
//using AutoMapper;
using Newtonsoft.Json;
using Realms;

[assembly: InternalsVisibleTo("RealmJson.Test")]

namespace SushiHangover.RealmJson
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
        /// <param name="inTransaction">bool.</param>
        /// <typeparam name="T">RealmOject-based Class..</typeparam>
        public static T CreateObjectFromJson<T>(this Realm realm, string jsonString, bool inTransaction = false) where T : RealmObject
        {
            var jsonObject = JsonConvert.DeserializeObject<T>(jsonString);
            return CreateObject<T>(realm, jsonObject, updateRecord: false, inTransaction: inTransaction);
        }

        /// <summary>
        /// Creates the single RealmObject from a stream containing json.
        /// </summary>
        /// <returns>The object from json.</returns>
        /// <param name="realm">Realm Instance.</param>
        /// <param name="stream">Stream.</param>
        /// <param name="inTransaction">bool.</param>
        /// <typeparam name="T">RealmObject-based Class.</typeparam>
        public static T CreateObjectFromJson<T>(this Realm realm, Stream stream, bool inTransaction = false) where T : RealmObject
        {
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                if (!jsonTextReader.Read())
                    throw new Exception(ExMalFormeJsonMessage);
                var jsonObject = serializer.Deserialize<T>(jsonTextReader);
                return CreateObject<T>(realm, jsonObject, updateRecord: false, inTransaction: inTransaction);
            }
        }


        /// <summary>
        /// Creates or updates a single RealmObject from a json string.
        /// </summary>
        /// <returns>the updatex object from json.</returns>
        /// <param name="realm">Realm Instance.</param>
        /// <param name="jsonString">Json string.</param>
        /// <param name="inTransaction">If set to <c>true</c> in transaction.</param>
        /// <typeparam name="T">RealmObject-based Class.</typeparam>
        public static T CreateOrUpdateObjectFromJson<T>(this Realm realm, string jsonString, bool inTransaction = false) where T : RealmObject
        {
            var jsonObject = JsonConvert.DeserializeObject<T>(jsonString);
            return CreateObject<T>(realm, jsonObject, updateRecord: true, inTransaction: inTransaction);
        }

        /// <summary>
        /// Creates multiple RealmObjects from a json stream
        /// </summary>
        /// <param name="realm">Realm Instance.</param>
        /// <param name="stream">Stream.</param>
        /// <param name="updateExistingRecords">If set to <c>true</c> update existing records.</param>
        /// <param name="inTransaction">If set to <c>true</c> in transaction.</param>
        /// <typeparam name="T">RealmObject-based Class.</typeparam>
        public static void CreateAllFromJson<T>(this Realm realm, Stream stream, bool updateExistingRecords = true, bool inTransaction = false) where T : RealmObject
        {
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                // This initial Read() is required to alllow Android Asset Streams to work w/ Newtonsoft 9.0.x?
                if (!jsonTextReader.Read() || jsonTextReader.TokenType != JsonToken.StartArray)
                    throw new Exception(ExMalFormeJsonMessage);

                if (inTransaction)
                {
                    CreateAllStream<T>(realm, updateExistingRecords, jsonTextReader, serializer);
                }
                else
                {
                    realm.Write(() =>
                    {
                        CreateAllStream<T>(realm, updateExistingRecords, jsonTextReader, serializer);
                    });
                }
            }
        }

        /// <summary>
        /// Creates multiple RealmObjects from a json string.
        /// </summary>
        /// <param name="realm">Realm Instance.</param>
        /// <param name="jsonString">Json string.</param>
        /// <param name="updateExistingRecords">If set to <c>true</c> update existing records.</param>
        /// <param name="inTransaction">If set to <c>true</c> in transaction.</param>
        /// <typeparam name="T">RealmObject-based Class.</typeparam>
        public static void CreateAllFromJson<T>(this Realm realm, string jsonString, bool updateExistingRecords = true, bool inTransaction = false) where T : RealmObject
        {
            var jsonList = JsonConvert.DeserializeObject<List<T>>(jsonString);
            if (inTransaction)
            {
                foreach (var jsonObject in jsonList)
                {
                    CreateObject<T>(realm, jsonObject, updateRecord: updateExistingRecords, inTransaction: true);
                }
            }
            else
            {
                realm.Write(() =>
                {
                    foreach (var jsonObject in jsonList)
                    {
                        CreateObject<T>(realm, jsonObject, updateRecord: updateExistingRecords, inTransaction: true);
                    }
                });
            }
        }

        /// <summary>
        /// Creates multiple RealmObjects from a json stream using AutoMapper
        /// </summary>
        /// <param name="realm">Realm Instance.</param>
        /// <param name="stream">Stream.</param>
        /// <param name="inTransaction">bool.</param>
        /// <typeparam name="T">RealmObject-based Class.</typeparam>
        //public static void CreateAllFromJsonViaAutoMapper<T>(this Realm realm, Stream stream, bool inTransaction = false) where T : RealmObject
        //{
        //    var pkProperty = typeof(T).GetRuntimeProperties().SingleOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
        //    var newRecordConfiguration = new MapperConfiguration(cfg =>
        //    {
        //        cfg.CreateMap<T, T>();
        //    });
        //    var newMapper = newRecordConfiguration.CreateMapper();
        //    var updateRecordConfiguration = new MapperConfiguration(cfg =>
        //    {
        //        if (pkProperty == null)
        //        {
        //            cfg.CreateMap<T, T>();
        //        }
        //        else // If RealmObject has PrimaryKey attrib, remove it from the mapping...
        //        {
        //            cfg.CreateMap<T, T>().ForMember(pkProperty.Name, opt => opt.Ignore());
        //        }
        //    });
        //    var updateMapper = updateRecordConfiguration.CreateMapper();

        //    using (var streamReader = new StreamReader(stream))
        //    using (var jsonTextReader = new JsonTextReader(streamReader))
        //    {
        //        var serializer = new JsonSerializer();
        //        // This initial Read() is required to alllow Andoird Asset Streams to work w/ Newtonsoft 9.0.?
        //        if (!jsonTextReader.Read() || jsonTextReader.TokenType != JsonToken.StartArray)
        //            throw new Exception(ExMalFormeJsonMessage);
        //        if (inTransaction)
        //        {
        //            CreateUpdateRecordsViaAutoMapper<T>(realm, pkProperty, newMapper, updateMapper, jsonTextReader, serializer);
        //        }
        //        else
        //        {
        //            realm.Write(() =>
        //            {
        //                CreateUpdateRecordsViaAutoMapper<T>(realm, pkProperty, newMapper, updateMapper, jsonTextReader, serializer);
        //            });
        //        }
        //    }
        //}

        //static void CreateUpdateRecordsViaAutoMapper<T>(Realm realm, PropertyInfo pkProperty, IMapper newMapper, IMapper updateMapper, JsonTextReader jsonTextReader, JsonSerializer serializer) where T : RealmObject
        //{
        //    while (jsonTextReader.Read() & jsonTextReader.TokenType == JsonToken.StartObject & jsonTextReader.TokenType != JsonToken.EndArray)
        //    {
        //        var jsonObject = serializer.Deserialize<T>(jsonTextReader);
        //        T realmObject = null;
        //        if (pkProperty != null)
        //        {
        //            realmObject = (T)FindByPKDynamic(realm, jsonObject.GetType(), pkProperty.GetValue(jsonObject), pkProperty.PropertyType != typeof(string));
        //        }
        //        if (realmObject == null)
        //        {
        //            realmObject = (T)Activator.CreateInstance(typeof(T));
        //            newMapper.Map<T, T>(jsonObject, realmObject);
        //            realm.Add(realmObject, true);
        //        }
        //        else
        //        {
        //            updateMapper.Map<T, T>(jsonObject, realmObject);
        //        }
        //    }
        //}

        //Contract.Ensures(Contract.Result<T>() != null);
        //	var jsonObject = JsonConvert.DeserializeObject<T>(jsonString);
        //var pkProperty = typeof(T).GetRuntimeProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
        //var realmObject = (T)FindByPKDynamic(realm, jsonObject.GetType(), pkProperty.GetValue(jsonObject), pkProperty.PropertyType != typeof(string));
        //realm.Write(() =>
        //	{
        //		if (realmObject == null)
        //		{
        //			realmObject = realm.CreateObject(typeof(T).Name);
        //			Mapper.Initialize(cfg => { cfg.CreateMap<T, T>(); });
        //			Mapper.Map<T, T>(jsonObject, realmObject);
        //		}
        //		else
        //		{
        //			Mapper.Initialize(cfg => // Map all, except for the PK
        //			{
        //				cfg.CreateMap<T, T>().ForMember(pkProperty.Name, opt => opt.Ignore());
        //			});
        //			Mapper.Map<T, T>(jsonObject, realmObject);
        //		}
        //	});

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T CreateObject<T>(Realm realm, T realmObject, bool updateRecord, bool inTransaction) where T : RealmObject
        {
            if (inTransaction)
            {
                realm.Add(realmObject, updateRecord);
            }
            else
            {
                realm.Write(() =>
                {
                    realm.Add(realmObject, updateRecord);
                });
            }
            return realmObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CreateAllStream<T>(Realm realm, bool updateExistingRecords, JsonTextReader jsonTextReader, JsonSerializer serializer) where T : RealmObject
        {
            while (jsonTextReader.Read() & jsonTextReader.TokenType == JsonToken.StartObject & jsonTextReader.TokenType != JsonToken.EndArray)
            {
                var jsonObject = serializer.Deserialize<T>(jsonTextReader);
                CreateObject<T>(realm, jsonObject, updateRecord: updateExistingRecords, inTransaction: true);
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(
        //    this IMappingExpression<TSource, TDestination> map, Expression<Func<TDestination, object>> selector)
        //{
        //    map.ForMember(selector, config => config.Ignore());
        //    return map;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                return realm.Find(type.Name, (long)castPKValue);
            }
            return realm.Find(type.Name, (string)primaryKeyValue);
        }

        /// <summary>
        /// Create a Generic non-managed RealmObject Copy
        /// </summary>
        /// <returns>The managed copy.</returns>
        /// <param name="realmObject">Realm object.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NonManagedCopy<T>(this RealmObject realmObject) where T : RealmObject
        {
            //var newRecordConfiguration = new MapperConfiguration(cfg =>
            //{
            //	cfg.CreateMap<T, T>();
            //});
            //var newMapper = newRecordConfiguration.CreateMapper();
            //var realmObjectCopy = (T)Activator.CreateInstance(typeof(T));
            //newMapper.Map(realmObject, realmObjectCopy);

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(realmObject));
        }

        /// <summary>
        /// Create a non-managed RealmObject Copy.
        /// </summary>
        /// <returns>The managed copy.</returns>
        /// <param name="realmObject">Realm object.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static RealmObject NonManagedCopy(this RealmObject realmObject)
        //{
            //var objectType = realmObject.GetType();
            //var newRecordConfiguration = new MapperConfiguration(cfg =>
            //{
            //	cfg.CreateMap(objectType, objectType);
            //});
            //var newMapper = newRecordConfiguration.CreateMapper();
            //var realmObjectCopy = Activator.CreateInstance(objectType) as RealmObject;
            //newMapper.Map(realmObject, realmObjectCopy);
            //return realmObjectCopy;
        //}

        /// <summary>
        /// Create a non-managed copy of Realm's Query Result.
        /// </summary>
        /// <param name="results">Realm collection</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the results.</typeparam>
        /// <returns>The collection of non-managed realm objects.</returns>
        public static IQueryable<T> NonManagedCopy<T>(this IQueryable<T> results) where T : RealmObject
        {
            var realmCollectionCopy = CopyRealmCollectionElements(results);
            return realmCollectionCopy.AsQueryable();
        }

        /// <summary>
        /// Create a non-managed copy of Realm's Query Result.
        /// </summary>
        /// <param name="list">Realm collection</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the results.</typeparam>
        /// <returns>The collection of non-managed realm objects.</returns>
        public static IList<T> NonManagedCopy<T>(this IList<T> list) where T : RealmObject
        {
            return CopyRealmCollectionElements(list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IList<T> CopyRealmCollectionElements<T>(IEnumerable<T> collection) where T : RealmObject
        {
            var realmCollectionCopy = new List<T>();
            foreach (var element in collection)
            {
                var nonManagedElementCopy = NonManagedCopy<T>(element);
                realmCollectionCopy.Add(nonManagedElementCopy);
            }
            return realmCollectionCopy;
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
