using HutongGames.PlayMaker;

namespace ItemSyncMod.Extras
{
    public class AdditionalFeatureAction : FsmStateAction
    {
        private readonly Action _method;

        public AdditionalFeatureAction(Action method)
        {
            _method = method;
        }

        public override void OnEnter()
        {
            try
            {
                _method();
            }
            catch (Exception e)
            {
                LogError("Error in ItemSync+Action:\n" + e);
            }

            Finish();
        }
    }

}
