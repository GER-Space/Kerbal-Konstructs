using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalKonstructs.Utilities
{
	public class MiscUtils
	{
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
