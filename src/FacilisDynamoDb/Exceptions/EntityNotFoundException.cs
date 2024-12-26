using System;

using FacilisDynamodb.Entities;

namespace FacilisDynamodb.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(IIdentity identity) 
            : base($"Entity not found, primaryKey: {identity.PrimaryKey}, sortKey: {identity.SortKey}")
        {
            PrimaryKey = identity.PrimaryKey;
            SortKey = identity.SortKey;
        }
        
        public string PrimaryKey { get; set; }
        public string SortKey { get; set; }
    }
}