namespace SP.ElasticSearchLibrary
{
    public sealed class ElasticSearchSettings
    {
        public int NumberOfReplicas { get; }

        public int NumberOfShards { get; }

        public ElasticSearchSettings(int numberOfReplicas, int numberOfShards)
        {
            NumberOfReplicas = numberOfReplicas;
            NumberOfShards   = numberOfShards;
        }
    }
}
