namespace BMRP.Runtime.PostFX
{
    public class DitherEffect : PostEffectBlit
    {
        public override string DisplayName => "Dither";

        public override string ShaderName => InternalDirectory + "Dither";
    }
}