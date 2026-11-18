using KSP.UI.Screens;
using System;
using System.Globalization;

namespace KerbalKonstructs
{
    public class MiscUtils
    {
        /// <summary>
        /// Parses user-typed text into a finite float without throwing.
        /// Accepts both '.' and ',' as decimal separator (the editors display values with the current culture).
        /// Returns false for empty, partial ("-", "."), malformed, NaN and infinite input; value is 0 in that case.
        /// </summary>
        public static bool TryParseFloat(string text, out float value)
        {
            value = 0f;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            text = text.Trim().Replace(',', '.');
            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || float.IsNaN(value) || float.IsInfinity(value))
            {
                value = 0f;
                return false;
            }
            return true;
        }

        public static Boolean CareerStrategyEnabled(Game gGame)
        {
            if (gGame.Mode == Game.Modes.CAREER)
            {
                if (!KerbalKonstructs.instance.disableCareerStrategyLayer) return true;
                else return false;
            }
            else return false;
        }

        public static Boolean isCareerGame()
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                if (!KerbalKonstructs.instance.disableCareerStrategyLayer) return true;
                else return false;
            }
            else
                return false;
        }

        public static void PostMessage(string sTitle, string sMessage, MessageSystemButton.MessageButtonColor cColor,
            MessageSystemButton.ButtonIcons bIcon)
        {
            MessageSystem.Message m = new MessageSystem.Message(sTitle, sMessage, cColor, bIcon);
            MessageSystem.Instance.AddMessage(m);
        }

        public static void HUDMessage(string sMessage, float fDuration = 10f, int iStyle = 2)
        {
            ScreenMessageStyle smsStyle = (ScreenMessageStyle)iStyle;
            ScreenMessages.PostScreenMessage(sMessage, fDuration, smsStyle);
        }
    }
}
