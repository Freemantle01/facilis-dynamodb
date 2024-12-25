using Amazon.Runtime;

namespace FacilisDynamoDb.Extensions.DependencyInjection.Credentials
{
    public class FakeImmutableCredentials : ImmutableCredentials
    {
        public FakeImmutableCredentials() 
            : base(
                "accessKey",
                "secretAccessKey",
                "token")
        {
        }
    }
}