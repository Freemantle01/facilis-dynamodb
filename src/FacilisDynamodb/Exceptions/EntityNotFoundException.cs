using System;

using FacilisDynamodb.Entities;

namespace FacilisDynamodb.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(IIdentity identity)
        {
            PrimaryKey = identity.PrimaryKey;
            SortKey = identity.SortKey;
        }
        
        public string PrimaryKey { get; set; }
        public string SortKey { get; set; }
    }
}