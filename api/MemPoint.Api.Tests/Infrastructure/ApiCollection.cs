namespace MemPoint.Api.Tests.Infrastructure;

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<MemPointApiFactory>
{
    public const string Name = "api";
}