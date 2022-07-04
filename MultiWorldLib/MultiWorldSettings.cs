namespace MultiWorldLib
{
    public enum RandomizationAlgorithm
    {
        Default = 0,
        OnlyOthersItemsLeastFillers,
        Balanced
    }

    public struct MultiWorldSettings
    {
        public int Seed;
        public RandomizationAlgorithm RandomizationAlgorithm;
    }
}
