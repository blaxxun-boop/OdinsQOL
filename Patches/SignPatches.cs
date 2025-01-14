﻿using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    internal class SignPatches
    {
        public static ConfigEntry<bool> useRichText;
        public static ConfigEntry<string> fontName;
        public static ConfigEntry<Vector2> textPositionOffset;
        public static ConfigEntry<Vector3> signScale;
        public static ConfigEntry<string> signDefaultColor;
        public static Font currentFont;
        public static string lastFontName;

        public static void FixSign(ref Sign sign)
        {
            sign.transform.localScale = signScale.Value;

            sign.m_textWidget.supportRichText = useRichText.Value;
            sign.m_characterLimit = 0;
            sign.m_textWidget.material = null;
            //sign.m_textWidget.fontSize = fontSize.Value;
            sign.m_textWidget.gameObject.GetComponent<RectTransform>().anchoredPosition = textPositionOffset.Value;
            if (lastFontName != fontName.Value) // call when config changes
            {
                lastFontName = fontName.Value;
                OdinQOLplugin.Dbgl($"new font {fontName.Value}");
                Font? font = GetFont(fontName.Value, 20);
                if (font == null)
                    OdinQOLplugin.Dbgl("new font not found");
                else
                    currentFont = font;
            }

            if (currentFont != null && sign.m_textWidget.font?.name != currentFont.name)
            {
                OdinQOLplugin.Dbgl($"setting font {currentFont.name}");
                sign.m_textWidget.font = currentFont;
            }
        }

        public static Font GetFont(string fontName, int fontSize)
        {
            Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
            foreach (Font? font in fonts)
                if (font.name == fontName)
                    return font;
            return Font.CreateDynamicFontFromOSFont(fontName, fontSize);
        }

        [HarmonyPatch(typeof(Sign), nameof(Sign.Awake))]
        private static class Sign_Awake_Patch
        {
            private static void Postfix(Sign __instance)
            {
                FixSign(ref __instance);
            }
        }

        [HarmonyPatch(typeof(Sign), nameof(Sign.UpdateText))]
        private static class Sign_UpdateText_Patch
        {
            private static void Postfix(Sign __instance)
            {
                FixSign(ref __instance);
                if (!__instance.m_nview.IsValid() || __instance.m_nview == null) return;
                if (signDefaultColor.Value is not { Length: > 0 }) return;
                if (__instance.m_defaultText.Contains("<color=")) return;
                string newText = $"<color={signDefaultColor.Value}>" +
                                 __instance.m_nview.GetZDO().GetString("text", __instance.m_defaultText) +
                                 "</color>";
                __instance.m_nview.ClaimOwnership();
                __instance.m_textWidget.text = newText;
                __instance.m_nview.GetZDO().Set(nameof(newText), newText);
            }
        }
    }
}