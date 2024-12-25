namespace FacilisDynamodb.Entities
{
    public interface IIdentity
    {
        string PrimaryKey { get; set; }
        string SortKey { get; set; }
    }
}