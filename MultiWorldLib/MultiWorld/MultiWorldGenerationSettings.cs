namespace MultiWorldLib.MultiWorldSettings
{
    public class MultiWorldGenerationSettings
    {
        public int Seed;
        public RandomizationAlgorithm RandomizationAlgorithm;
    }

    public enum RandomizationAlgorithm
    {
        Default = 0,
        OnlyOthersItemsLeastFillers,
        Balanced
    }
}
