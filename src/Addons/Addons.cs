using System.Linq;

namespace KerbalKonstructs.Core
{
    class AddonUtils
    {
        /// <summary>
        /// Checks if the addon ist installed
        /// </summary>
        /// <param name="addonName"></param>
        /// <returns></returns>
        internal static bool IsInstalled(string addonName)
        {
            var assembly = (from a in AssemblyLoader.loadedAssemblies
                            where a.name.ToLower().Equals(addonName.ToLower())
                            select a).FirstOrDefault();
            return assembly != null;
        }
    }
}
