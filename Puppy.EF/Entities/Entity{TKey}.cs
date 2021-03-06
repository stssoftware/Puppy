﻿#region	License

//------------------------------------------------------------------------------------------------
// <Auto-generated>
//     <Author> Top Nguyen (http://topnguyen.net) </Author>
//     <Project> TopCore.Auth.Domain </Project>
//     <File> 
//         <Name> Entity.cs </Name>
//         <Created> 27 03 2017 02:39:27 PM </Created>
//         <Key> 9BBFB04B-177D-4D87-B047-2F94F957E579 </Key>
//     </File>
//     <Summary>
//         Entity is abstract entity to another entity inherit
//     </Summary>
// </Auto-generated>
//------------------------------------------------------------------------------------------------

#endregion License

using Puppy.EF.Interfaces.Entities;

namespace Puppy.EF.Entities
{
    /// <inheritdoc cref="EntityBase" />
    /// <summary>
    ///     Entity for Entity Framework
    /// </summary>
    /// <typeparam name="TKey">Id type of this entity</typeparam>
    public abstract class Entity<TKey> : EntityBase, ISoftDeletableEntity<TKey>, IAuditableEntity<TKey> where TKey : struct
    {
        public TKey Id { get; set; }

        public TKey? CreatedBy { get; set; }

        public TKey? LastUpdatedBy { get; set; }

        public TKey? DeletedBy { get; set; }
    }

    public abstract class Entity : Entity<int>
    {
    }
}