namespace MultiWorldLib.MultiWorldSettings
{
    public class MultiWorldGenerationSettings
    {
        [Obsolete]
        public int Seed = 0;

        public RandomizationAlgorithm RandomizationAlgorithm;
    }

    public enum RandomizationAlgorithm
    {
        Default = 0,
        OnlyOthersItemsLeastFillers,
        Balanced
    }
}
