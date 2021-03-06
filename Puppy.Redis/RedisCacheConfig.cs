﻿#region	License
//------------------------------------------------------------------------------------------------
// <License>
//     <Copyright> 2017 © Top Nguyen → AspNetCore → Monkey </Copyright>
//     <Url> http://topnguyen.net/ </Url>
//     <Author> Top </Author>
//     <Project> Monkey </Project>
//     <File>
//         <Name> RedisCacheConfig.cs </Name>
//         <Created> 18/07/17 12:02:55 PM </Created>
//         <Key> e5beb171-1270-48c0-956c-4d77a9c5d849 </Key>
//     </File>
//     <Summary>
//         RedisCacheConfig.cs
//     </Summary>
// <License>
//------------------------------------------------------------------------------------------------
#endregion License

namespace Puppy.Redis
{
    public static class RedisCacheConfig
    {
        /// <summary>
        ///     Default is <c> localhost:6379 </c> 
        /// </summary>
        public static string ConnectionString { get; set; } = "localhost:6379";

        /// <summary>
        ///     Default is <c> Default </c> 
        /// </summary>
        public static string InstanceName { get; set; } = "Default";
    }
}