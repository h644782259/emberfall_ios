using UnityEngine;

namespace Emberfall
{
    /// <summary>One packaged Chinese font for UI and world labels on every platform.</summary>
    public static class GameFont
    {
        private static Font shared;
        public static Font Shared
        {
            get
            {
                if (shared == null)
                    shared = Resources.Load<Font>("Fonts/NotoSansSC-Regular");
                if (shared == null)
                    Debug.LogError("Missing packaged font: Fonts/NotoSansSC-Regular");
                return shared;
            }
        }

        public static void Apply(TextMesh text)
        {
            text.font = Shared;
            if (text.font != null)
                text.GetComponent<Renderer>().sharedMaterial = text.font.material;
        }
    }
}
