namespace BMRP.Runtime.PostFX
{
    [System.Serializable]
    public class AcesEffect : PostEffectBlit
    {
        public override string DisplayName => "Tonemapping";
        public override string ShaderName => InternalDirectory + "ACES";
    }
}
