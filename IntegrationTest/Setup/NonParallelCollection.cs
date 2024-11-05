

namespace IntegrationTest.Setup;
[CollectionDefinition(CollectionDefinitions.NonParallelCollectionName, DisableParallelization = true)]
public class NonParallelCollection
{
	
}

public static class CollectionDefinitions
{
	public const string NonParallelCollectionName = "NonParallelCollection";
}