namespace MultiWorldLib.MultiWorldSettings
{
    public class MultiWorldGenerationSettings
    {
        [Obsolete]
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
