using MushroomMap.IntegrationTests.Fixtures;
using Xunit;

namespace MushroomMap.IntegrationTests;

[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
