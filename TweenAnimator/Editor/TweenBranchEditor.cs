using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Zos.Core.TweenAnimator.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenBranch))]
    public class TweenBranchEditor : UnityEditor.Editor
    {
        static bool IsInfoOpened = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            TweenBranch branch = (target as TweenBranch);
            if(Application.isPlaying)
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Show All"))
                {
                    branch.Show();
                }

                if(GUILayout.Button("Hide All"))
                {
                    branch.Hide();
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Show All Immediate"))
                {
                    branch.ShowImmediate(false);
                }
                if(GUILayout.Button("Hide All Immediate"))
                {
                    branch.HideImmediate(false);
                }
                GUILayout.EndHorizontal();
            }

            IsInfoOpened = GUILayout.Toggle(IsInfoOpened, "Show delay and duration info");
            if(IsInfoOpened)
            {
                List<TweenAnimator> anims = new List<TweenAnimator>();
                branch.RecursiveAnimatorSearch(anims);
                if(anims.Count == 0)
                {
                    EditorGUILayout.HelpBox("no animation", MessageType.Info);
                    return;
                }
                anims = anims.OrderBy(x => x.ShowPreDelay).ToList();
                string info = "";
                info += $"Show full duration: {anims.Max(x => x.FullDurationShow)}\n";
                info += $"Hide full duration: {anims.Max(x => x.FullDurationHide)}\n";
                info += "\n*****";
                for(int i = 0; i < anims.Count; i++)
                {
                    info += $"\nS delay:{anims[i].ShowPreDelay:0.0} H delay:{anims[i].HidePreDelay:0.0} S dur:{anims[i].FullDurationShow:0.0} H dur:{anims[i].FullDurationHide:0.0} {anims[i].gameObject.name}";
                }
                EditorGUILayout.HelpBox(info, MessageType.Info);
            }
        }
    }
}