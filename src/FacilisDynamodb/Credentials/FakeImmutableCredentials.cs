using Amazon.Runtime;

namespace FacilisDynamoDb.Credentials
{
    public class FakeImmutableCredentials : ImmutableCredentials
    {
        public FakeImmutableCredentials() 
            : base("accessKey",
                "secretAccessKey",
                "token")
        {
        }
    }
}