namespace MDD4All.SpecIF.DataProvider.Contracts.Authorization
{
    public interface IJwtConfigurationReader
    {
        string GetSecret();

        string GetIssuer();
    }
}
