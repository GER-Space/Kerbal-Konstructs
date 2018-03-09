using System.Collections.Generic;
using System.Text.RegularExpressions;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    internal static class CareerUtils
    {


        internal static bool isCareerGame
        {
            get
            {
                return ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER) & (!KerbalKonstructs.instance.disableCareerStrategyLayer));
            }
        }

        internal static bool isSandboxGame
        {
            get
            {
                if (HighLogic.LoadedScene == GameScenes.MAINMENU)
                {
                    return false;
                }
                else
                {
                    return ((HighLogic.CurrentGame.Mode != Game.Modes.CAREER) || (KerbalKonstructs.instance.disableCareerStrategyLayer));
                }
                
            }
        }

        internal static string KeyFromString(string orginalString)
        {
            return Regex.Replace(orginalString, @"\s+", "");
        }

        internal static string LSKeyFromName(string name)
        {
            return name.Replace(' ', '_').ToUpperInvariant();
        }

    }
}
