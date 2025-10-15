using UnityEngine;

namespace IdleBiz.UI
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "IdleBiz/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Backgrounds")]
        public Color appBackground = new Color(0.08f, 0.12f, 0.18f, 1f);    // #15202E
        public Color cardBackground = new Color(0.12f, 0.18f, 0.27f, 1f);    // #1F2D44
        public Color topBarBackground = new Color(0.10f, 0.16f, 0.24f, 1f);  // #1A293D

        [Header("Text")]
        public Color textPrimary = Color.white;
        public Color textSecondary = new Color(1f, 1f, 1f, 0.75f);

        [Header("Buttons")]
        public Color btnNormal = new Color(0.20f, 0.56f, 0.36f, 1f);         // þalia
        public Color btnHighlighted = new Color(0.24f, 0.66f, 0.42f, 1f);
        public Color btnPressed = new Color(0.16f, 0.44f, 0.28f, 1f);
        public Color btnDisabled = new Color(0.4f, 0.4f, 0.4f, 1f);

        [Header("Accents")]
        public Color accent = new Color(0.97f, 0.74f, 0.26f, 1f);            // auksinë
        public Color danger = new Color(0.82f, 0.23f, 0.23f, 1f);            // raudona
    }
}
