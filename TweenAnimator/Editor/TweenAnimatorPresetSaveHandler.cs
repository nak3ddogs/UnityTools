using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Zos.Core.TweenAnimator.Editor
{
    public static class TweenAnimatorPresetSaveHandler
    {
        const string DirPath = "Assets/UI/Data/TweenAnimatorPresets/";

        private static string GetSaveFilePath(string fileName)
        {
            return AssetDatabase.GenerateUniqueAssetPath(DirPath + fileName + ".asset");
        }

        public static void Save(TweenAnimatorPresetSO so, TweenAnimator anim, ref string errorMsg, ref string statusMsg)
        {
            errorMsg = "";
            statusMsg = "";
            if(so == null)
            {
                errorMsg = "scriptable object is null";
                return;
            }
            so.animations = anim.Animations.ConvertAll(x => x.Clone).ToList();
            string presetPath = AssetDatabase.GetAssetPath(so);
            if(presetPath != null && presetPath != "")
            {
                statusMsg = "saved\npath:" + presetPath;
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static TweenAnimatorPresetSO CreateNew(string name, TweenAnimator anim, ref string errorMsg, ref string statusMsg)
        {
            string targetPath = GetSaveFilePath(name);
            TweenAnimatorPresetSO so = ScriptableObject.CreateInstance<TweenAnimatorPresetSO>() as TweenAnimatorPresetSO;
            so.animations = anim.Animations.ConvertAll(x => x.Clone).ToList();
            AssetDatabase.CreateAsset(so, targetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            statusMsg = "new save file\npath:" + targetPath;
            errorMsg = "";
            return so;
        }

        public static void Load(TweenAnimatorPresetSO so, TweenAnimator anim, ref string errorMsg, ref string statusMsg)
        {
            errorMsg = "";
            statusMsg = "";
            if(so == null)
            {
                errorMsg = "save file null";
                return;
            }
            anim.Animations = so.animations.ConvertAll(x => x.Clone).ToList();
            statusMsg = "load complete";
        }
    }
}