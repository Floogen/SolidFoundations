namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class FadeAction
    {
        public float Speed { get; set; } = 0.02f;
        public SpecialAction ActionAfterFade { get; set; }
    }
}
