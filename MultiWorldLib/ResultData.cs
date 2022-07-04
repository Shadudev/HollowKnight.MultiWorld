namespace MultiWorldLib
{
    [Serializable]
    public struct ResultData
    {
        public int PlayerId;
        public int RandoId;
        public string[] Nicknames;
        public Placement[] Placements { get; set; }
        public string ItemsSpoiler { get; set; }
    }
}
