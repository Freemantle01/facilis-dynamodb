using Amazon.Runtime;

namespace FacilisDynamoDb.Extensions.DependencyInjection.Credentials
{
    public class FakeAwsCredentials : AWSCredentials
    {
        public override ImmutableCredentials GetCredentials()
        {
            return new FakeImmutableCredentials();
        }
    }
}