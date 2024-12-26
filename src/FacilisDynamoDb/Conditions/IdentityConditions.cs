using FacilisDynamodb.Constants;

namespace FacilisDynamoDb.Conditions
{
    public static class IdentityConditions
    {
        public static string GetEntityExistsCondition()
            => $"attribute_exists({TableConstants.PrimaryKeyName}) AND attribute_exists({TableConstants.SortKeyName})";
    }
}