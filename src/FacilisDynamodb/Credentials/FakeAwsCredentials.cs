using Amazon.Runtime;

namespace FacilisDynamoDb.Credentials
{
    public class FakeAwsCredentials : AWSCredentials
    {
        public override ImmutableCredentials GetCredentials()
        {
            return new FakeImmutableCredentials();
        }
    }
}