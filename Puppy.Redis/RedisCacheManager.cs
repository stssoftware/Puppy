﻿#region	License

//------------------------------------------------------------------------------------------------
// <License>
//     <Copyright> 2017 © Top Nguyen → AspNetCore → Puppy </Copyright>
//     <Url> http://topnguyen.net/ </Url>
//     <Author> Top </Author>
//     <Project> Puppy </Project>
//     <File>
//         <Name> RedisCacheManager.cs </Name>
//         <Created> 17/07/17 4:40:21 PM </Created>
//         <Key> f2340ebd-5b03-4ffb-a45e-71d9cc1748e3 </Key>
//     </File>
//     <Summary>
//         RedisCacheManager.cs
//     </Summary>
// <License>
//------------------------------------------------------------------------------------------------

#endregion License

using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Puppy.Redis
{
    public class RedisCacheManager : IRedisCacheManager
    {
        protected IDistributedCache Cache;

        public RedisCacheManager(IDistributedCache cache)
        {
            Cache = cache;
        }

        /// <summary>
        ///     Absolute Expiration by duration
        /// </summary>
        /// <param name="key">     </param>
        /// <param name="data">    </param>
        /// <param name="duration"></param>
        public void Set(string key, object data, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null)
                return;

            DateTimeOffset absoluteExpiration = DateTime.UtcNow + duration;

            Set(key, data, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration
            });
        }

        public void Set(string key, object data, DistributedCacheEntryOptions options)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null)
                return;

            var dataStr = data as string;
            var dataStore = dataStr ?? JsonConvert.SerializeObject(data);
            Cache.Set(key, Encoding.UTF8.GetBytes(dataStore), options);
        }

        public void Remove(string key)
        {
            if (IsExist(key))
            {
                Cache.Remove(key);
            }
        }

        public bool IsExist(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            var fromCache = Cache.Get(key);
            return fromCache != null;
        }

        public T Get<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var fromCache = Cache.Get(key);
            if (fromCache == null)
                return null;

            var str = Encoding.UTF8.GetString(fromCache);
            if (typeof(T) == typeof(string))
                return str as T;

            return JsonConvert.DeserializeObject<T>(str);
        }

        public void VerifySetup()
        {
            string verifyKey = "VerifySetup";
            string verifyOriginalData = "Check Redis is Healthy";

            // Verify Set
            try
            {
                Set(verifyKey, verifyOriginalData, TimeSpan.FromSeconds(10));
            }
            catch (Exception)
            {
                throw new Exception($"{nameof(Set)} of {nameof(IRedisCacheManager)} is not working, you missing setup Redis Cache or wrong Redis Connection String.");
            }

            // Verify IsExist
            if (!IsExist(verifyKey))
            {
                throw new Exception($"{nameof(IsExist)} of {nameof(IRedisCacheManager)} is not working.");
            }

            // Verify Get
            var verifyData = Get<string>(verifyKey);
            if (verifyData != verifyOriginalData)
            {
                throw new Exception($"{nameof(Get)} of {nameof(IRedisCacheManager)} is not working.");
            }

            // Verify Delete
            Remove(verifyKey);
            if (IsExist(verifyKey))
            {
                throw new Exception($"{nameof(Remove)} of {nameof(IRedisCacheManager)} is not working.");
            }
        }
    }
}