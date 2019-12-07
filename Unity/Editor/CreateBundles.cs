using System.IO;
using UnityEngine;
using System.Collections;
using UnityEditor;


public class AssetBundleCompiler
{	

    [MenuItem ("Assets/Build Selected AssetBundle")]
    static void BuildBundles ()
    {
        string path = EditorUtility.SaveFilePanel("Build Asset Bundle", "Bundles", "KKShaders", "bundle");
        exportAssetBundle(BuildTarget.StandaloneWindows64, path);
        exportAssetBundle(BuildTarget.StandaloneOSX, path);
        exportAssetBundle(BuildTarget.StandaloneLinux64, path);
    }


    [MenuItem ("Assets/Build Windows AssetBundle")]
    static void BuildBundles_win ()
    {
        string path = EditorUtility.SaveFilePanel("Build Asset Bundle", "Bundles", "KKShaders", "bundle");
        exportAssetBundle(BuildTarget.StandaloneWindows64, path);
    }
	
    private static void exportAssetBundle(BuildTarget target, string path)
    {
        string extention ;
        switch (target)
        {
        case BuildTarget.StandaloneWindows64:
            extention = ".windows";
            break;
        case BuildTarget.StandaloneOSX:
            extention = ".osx";
            break;
        case BuildTarget.StandaloneLinux64:
            extention = ".linux";
            break;
        default: 
            extention = ".windows";
        break;
        }

        string directory = path.Substring(0, path.LastIndexOf('/'));
        string name = path.Substring(path.LastIndexOf('/') + 1);
        name = name.Substring(0,name.IndexOf("."));
        name = name + extention;
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = name;
        build.assetNames = new string[selection.Length];
        int len = selection.Length;
        for (int i = 0; i < len; i++)
        {
            build.assetNames[i] = AssetDatabase.GetAssetPath((UnityEngine.Object)selection[i]);
        }
        BuildPipeline.BuildAssetBundles(directory, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.None, target);

    }

}
