using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace Zos.Core.TweenAnimator.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenAnimator))]
    public class TweenAnimatorEditor : UnityEditor.Editor
    {
        private ReorderableList ReorderableList = null;
        private TweenAnimation CurrentSelectedAnim = null;
        private TweenAnimatorPresetSO TweenAnimSo = null;
        private string ScriptableObjectName = "name of new preset";
        private string ErrorMsg = "";
        private string StatusMsg = "";

        private void OnEnable()
        {
            TweenAnimSo = t.AnimatorPreset;
            t.UpdateAnimationList();

            ReorderableList = new ReorderableList(t.Animations, typeof(TweenAnimation));
            ReorderableList.onSelectCallback = x => CurrentSelectedAnim = t.Animations[ReorderableList.index];
            ReorderableList.onAddCallback = x =>
            {
                TweenAnimation an;
                if(CurrentSelectedAnim != null)
                {
                    an = CurrentSelectedAnim.Clone;
                }
                else
                {
                    an = new TweenAnimation();
                }
                t.Animations.Add(an);
                CurrentSelectedAnim = an;
                x.index = t.Animations.Count - 1;

                if(Application.isPlaying)
                {
                    OnEnable();
                }
                else
                {
                    t.AnimatorPreset = null;
                    Undo.RecordObject(target, "added animation");
                    EditorUtility.SetDirty(t);
                    EditorSceneManager.MarkAllScenesDirty();
                }
            };
            ReorderableList.onRemoveCallback = x =>
            {
                if(CurrentSelectedAnim != null)
                {
                    t.Animations.Remove(CurrentSelectedAnim);
                    CurrentSelectedAnim = null;
                    if(t.Animations.Count > 0)
                    {
                        x.index = t.Animations.Count - 1;
                        CurrentSelectedAnim = t.Animations[x.index];
                    }

                    t.AnimatorPreset = null;

                    if(Application.isPlaying)
                    {
                        OnEnable();
                    }
                    else
                    {
                        t.AnimatorPreset = null;
                        Undo.RecordObject(target, "removed animation");
                        EditorUtility.SetDirty(t);
                        EditorSceneManager.MarkAllScenesDirty();

                    }
                }
            };
            ReorderableList.drawElementCallback =
        (rect, index, isActive, isFocused) =>
        {
            //rect.y += 2;
            string s = t.Animations[index].Type.ToString();

            if(!t.Animations[index].DoShow && !t.Animations[index].DoHide)
            {
                s += "(muted)";
            }
            else if(!t.Animations[index].DoShow)
            {
                s += "(hide only)";
            }
            else if(!t.Animations[index].DoHide)
            {
                s += "(show only)";
            }

            EditorGUI.LabelField(rect, s);
        };
            ReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                string s = "show full: " + t.FullDurationShow + " hide full: " + t.FullDurationHide;
                EditorGUI.LabelField(rect, s);
            };
        }

        public TweenAnimator t
        {
            get
            {
                return target as TweenAnimator;
            }
        }

        public override void OnInspectorGUI()
        {

            GUILayout.Space(20);
            serializedObject.Update();

            if(Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Branch: Show all"))
                {
                    Transform _currentTransform = t.transform;
                    TweenBranch branch = null;
                    while(branch == null && _currentTransform != null)
                    {
                        _currentTransform = _currentTransform.parent;
                        branch = _currentTransform.GetComponent<TweenBranch>();
                    }
                    if(branch != null)
                    {
                        branch.Show();
                    }
                }
                if(GUILayout.Button("Branch: Hide all"))
                {
                    t.GetComponentInParent<TweenBranch>().Hide();
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Show"))
                {
                    t.Show();
                }
                if(GUILayout.Button("Hide"))
                {
                    t.Hide();
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(20);
            }

            EditorGUI.BeginChangeCheck();

            t.IgnoreTweenBranch = EditorGUILayout.Toggle("Ignored by TweenBranch", t.IgnoreTweenBranch);
            t.ResetPropertiesAfterShow = EditorGUILayout.Toggle("Reset parameters after show", t.ResetPropertiesAfterShow);
            t.ResetPropertiesAfterHide = EditorGUILayout.Toggle("Reset parameters after hide", t.ResetPropertiesAfterHide);
            GUILayout.BeginHorizontal();
            t.ShowPreDelay = Mathf.Max(0, EditorGUILayout.FloatField("Show pre delay", t.ShowPreDelay));
            t.HidePreDelay = Mathf.Max(0, EditorGUILayout.FloatField("Hide pre delay", t.HidePreDelay));
            GUILayout.EndHorizontal();

            if(!t.IsListElement)
            {
                t.IsListElement = EditorGUILayout.Toggle("Is list elem", t.IsListElement);
            }
            else
            {
                t.IsListElement = EditorGUILayout.Toggle("Is list elem", t.IsListElement);
                GUILayout.BeginHorizontal();
                t.ResetStartingPropertiesBeforeAnimating = EditorGUILayout.Toggle("Reset starting properties before animating", t.ResetStartingPropertiesBeforeAnimating);
                t.WaitForFinish = EditorGUILayout.Toggle("Wait for finish", t.WaitForFinish);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                t.ShowDelayOffset = EditorGUILayout.FloatField("Show delay offset", t.ShowDelayOffset);
                t.HideDelayOffset = EditorGUILayout.FloatField("Hide delay offset", t.HideDelayOffset);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);
            if(t.AnimatorPreset)
            {
                EditorGUILayout.HelpBox("Works as preset: " + t.AnimatorPreset.name, MessageType.Info);
                GUILayout.Space(20);
            }
            if(EditorGUI.EndChangeCheck() && !Application.isPlaying)
            {
                Undo.RecordObject(target, "DelayChangeGroupChange");
                EditorUtility.SetDirty(t);
                EditorSceneManager.MarkAllScenesDirty();
            }

            EditorGUI.BeginChangeCheck();

            TweenAnimSo = (TweenAnimatorPresetSO)EditorGUILayout.ObjectField(TweenAnimSo, typeof(TweenAnimatorPresetSO), false);

            if(TweenAnimSo != null)
            {
                GUILayout.BeginHorizontal();

                if(GUILayout.Button("Load"))
                {
                    TweenAnimatorPresetSaveHandler.Load(TweenAnimSo, t, ref ErrorMsg, ref StatusMsg);
                    t.AnimatorPreset = TweenAnimSo;
                    EditorUtility.SetDirty(t);
                    EditorSceneManager.MarkAllScenesDirty();

                    CurrentSelectedAnim = null;
                    OnEnable();
                }

                if((Selection.gameObjects.Length == 1) && GUILayout.Button("Overwrite"))
                {
                    TweenAnimatorPresetSaveHandler.Save(TweenAnimSo, t, ref ErrorMsg, ref StatusMsg);
                    t.AnimatorPreset = TweenAnimSo;
                    EditorUtility.SetDirty(t);
                    EditorSceneManager.MarkAllScenesDirty();

                    OnEnable();
                }


                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            if(Selection.gameObjects.Length == 1)
            {
                GUILayout.BeginHorizontal();
                ScriptableObjectName = EditorGUILayout.TextField(ScriptableObjectName).Replace(" ", "");
                if(GUILayout.Button("Save as"))
                {
                    TweenAnimSo = TweenAnimatorPresetSaveHandler.CreateNew(ScriptableObjectName, t, ref ErrorMsg, ref StatusMsg);
                    t.AnimatorPreset = TweenAnimSo;
                }
                GUILayout.EndHorizontal();
            }

            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "SaveLoadNew");
                EditorUtility.SetDirty(t);
                EditorSceneManager.MarkAllScenesDirty();
            }

            if(ErrorMsg != "")
            {
                EditorGUILayout.HelpBox(ErrorMsg, MessageType.Error);
            }
            if(StatusMsg != "")
            {
                EditorGUILayout.HelpBox(StatusMsg, MessageType.Info);
            }

            GUILayout.Space(20);
            if(Selection.gameObjects.Length == 1)
            {
                ReorderableList.DoLayoutList();
            }

            if(CurrentSelectedAnim != null && (Selection.gameObjects.Length == 1))
            {
                EditorGUI.BeginChangeCheck();

                GUILayout.Label("Animation Type");
                CurrentSelectedAnim.Type = (TweenAnimationType)EditorGUILayout.EnumPopup(CurrentSelectedAnim.Type);
                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                CurrentSelectedAnim.DoShow = EditorGUILayout.Toggle("Do show", CurrentSelectedAnim.DoShow);
                GUILayout.Space(20);
                CurrentSelectedAnim.DoHide = EditorGUILayout.Toggle("Do hide", CurrentSelectedAnim.DoHide);
                EditorGUILayout.EndHorizontal();
                CurrentSelectedAnim.Ease = (Ease)EditorGUILayout.EnumPopup("Ease", CurrentSelectedAnim.Ease);

                CurrentSelectedAnim.Delay = Mathf.Max(0, EditorGUILayout.FloatField("Delay", CurrentSelectedAnim.Delay));
                CurrentSelectedAnim.Duration = Mathf.Max(0, EditorGUILayout.FloatField("Duration", CurrentSelectedAnim.Duration));
                GUILayout.Space(10);

                switch(CurrentSelectedAnim.Type)
                {
                    case TweenAnimationType.Position:
                        CurrentSelectedAnim.IsRelativeMove = EditorGUILayout.Toggle("Is relative", CurrentSelectedAnim.IsRelativeMove);
                        CurrentSelectedAnim.HiddenPosition = EditorGUILayout.Vector2Field("Hidden position", CurrentSelectedAnim.HiddenPosition);
                        break;

                    case TweenAnimationType.Scale:
                        CurrentSelectedAnim.HiddenScale = EditorGUILayout.Vector2Field("Hidden scale", CurrentSelectedAnim.HiddenScale);
                        break;

                    case TweenAnimationType.ImageAlpha:
                        CurrentSelectedAnim.ImageHiddenAlpha = EditorGUILayout.Slider("Hidden image alpha", CurrentSelectedAnim.ImageHiddenAlpha, 0f, 1f);
                        break;

                    case TweenAnimationType.CanvasGroupAlpha:
                        CurrentSelectedAnim.CanvasGroupHiddenAlpha = EditorGUILayout.Slider("Hidden canvas alpha", CurrentSelectedAnim.CanvasGroupHiddenAlpha, 0f, 1f);
                        break;

                    case TweenAnimationType.RectSize:
                        CurrentSelectedAnim.HiddenDeltaSize = EditorGUILayout.Vector2Field("Hidden delta size", CurrentSelectedAnim.HiddenDeltaSize);
                        break;
                }

                if(EditorGUI.EndChangeCheck() && !Application.isPlaying)
                {

                    Undo.RecordObject(target, "Changed animation tween");
                    t.AnimatorPreset = null;
                    EditorUtility.SetDirty(t);
                    EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }

        protected override void OnHeaderGUI()
        {
            EditorGUILayout.LabelField("Tween Animator");
        }
    }
}