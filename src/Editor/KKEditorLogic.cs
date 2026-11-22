using KerbalKonstructs.UI2;

namespace KerbalKonstructs.Core
{
    internal static class KKEditorLogic
    {

        private static MainEditorMode currentMode = MainEditorMode.Instance;


        internal enum MainEditorMode
        {
            Instance,
            Group,
            Terrain
        }


        internal static MainEditorMode CurrentMode => currentMode;


        /// <summary>
        /// True while the editor (the Editor Mode toolbar, Ctrl+K) is open.
        /// Editor windows use this to decide whether they may stay open.
        /// </summary>
        internal static bool IsEditorOpen()
        {
            return EditorModeSelector.IsOpen();
        }

        internal static string GetMainMode()
        {
            return currentMode.ToString();
        }

        internal static void SetMainMode(MainEditorMode newMode)
        {
            if (currentMode != newMode)
            {
                CloseCurrentEditor();
                currentMode = newMode;
                OpenCurrentToolbar();
            }

        }


        internal static void OpenCurrentToolbar()
        {
            switch (currentMode)
            {
                case MainEditorMode.Instance:
                    InstanceEditorToolbar.Open();
                    break;

                case MainEditorMode.Group:
                    GroupEditorToolbar.Open();
                    break;

                case MainEditorMode.Terrain:
                    TerrainEditorToolbar.Open();
                    break;
                default:
                    Log.UserError("Not implemented");
                    break;
            }
        }

        internal static void CloseCurrentEditor()
        {
            switch (currentMode)
            {
                case MainEditorMode.Instance:
                    InstanceEditorToolbar.Close();
                    break;

                case MainEditorMode.Group:
                    GroupEditorToolbar.Close();
                    break;

                case MainEditorMode.Terrain:
                    TerrainEditorToolbar.Close();
                    break;
                default:
                    Log.UserError("Not implemented");
                    break;
            }
        }



    }
}
