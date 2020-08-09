using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Zos.Core.TweenAnimator
{
    [AddComponentMenu("UI/Animator/Animator")]
    public class TweenAnimator : MonoBehaviour, ITweenBranchElement
    {
        public bool IgnoreTweenBranch = false;
        public bool ResetPropertiesAfterShow = true;
        public bool ResetPropertiesAfterHide = false;

        public float ShowPreDelay = 0.0f;
        public float HidePreDelay = 0.0f;
        //list elem settings
        public bool IsListElement = false;
        public bool WaitForFinish = false;
        public bool ResetStartingPropertiesBeforeAnimating = true;
        public float ShowDelayOffset = 0.1f;
        public float HideDelayOffset = 0.1f;

        public TweenAnimatorPresetSO AnimatorPreset = null;
        public List<TweenAnimation> Animations = new List<TweenAnimation>();

        //events
        public event System.Action OnShowStartEvent = null;
        public event System.Action OnShowCompleteEvent = null;
        public event System.Action OnHideStartEvent = null;
        public event System.Action OnHideCompleteEvent = null;

        //variables
        public float CurrentListOffsetAtShow = 0.0f;
        public float CurrentListOffsetAtHide = 0.0f;
        private bool HasPositionsTween = false;
        private bool HasScaleTween = false;
        private bool HasCanvasGroupTween = false;
        private bool HasImageTween = false;
        public RectTransform RectTransform { get; private set; } = null;
        public CanvasGroup CanvasGroup { get; private set; } = null;
        public Image Image { get; private set; } = null;
        public TweenBranch Owner { get; private set; } = null;
        public Vector2 StartAnchoredPosition { get; private set; } = Vector2.zero;
        public Vector2 StartRectSize { get; private set; } = Vector2.zero;
        public Vector3 StartLocalScale { get; private set; } = Vector3.zero;
        public float StartImageAlpha { get; private set; } = 0.0f;
        public float StartCanvasGroupAlpha { get; private set; } = 0.0f;
        private bool IsAnimationListUpdated = false;
        private Sequence CurrentSequence = null;

        public float FullDurationShow
        {
            get
            {
                if(IsListElement && !WaitForFinish)
                {
                    return 0.0f;
                }
                if(!Animations.Any(x => x.DoShow))
                {
                    return 0.0f;
                }
                return Animations.Max(x => x.Duration + x.Delay + CurrentListOffsetAtShow) + ShowPreDelay;
            }
        }

        public float FullDurationHide
        {
            get
            {
                if(IsListElement && !WaitForFinish)
                {
                    return 0.0f;
                }
                if(!Animations.Any(x => x.DoHide))
                {
                    return 0.0f;
                }
                return Animations.Max(x => x.Duration + x.Delay + CurrentListOffsetAtHide) + HidePreDelay;
            }
        }

        private void Start()
        {
            if(Owner == null)
            {
                Initialize(null);
            }
        }

        public void Initialize(TweenBranch branch)
        {
            if(Owner)
            {
                Owner.OnShowStartEvent -= Owner_OnAnimationStartEvent;
                Owner.OnHideStartEvent -= Owner_OnAnimationStartEvent;
                Owner.OnShowCompleteEvent -= Owner_OnShowCompleteEvent;
                Owner.OnHideCompleteEvent -= Owner_OnHideCompleteEvent;
            }
            Owner = branch;
            if(Owner)
            {
                //Owner.OnShowStartEvent += Owner_OnAnimationStartEvent; //TODO fix it
                Owner.OnHideStartEvent += Owner_OnAnimationStartEvent;
                Owner.OnShowCompleteEvent += Owner_OnShowCompleteEvent;
                Owner.OnHideCompleteEvent += Owner_OnHideCompleteEvent;
            }
            if(!IsAnimationListUpdated)
            {
                IsAnimationListUpdated = true;
                UpdateAnimationList();
                SetupReferencesAndStartingValues();
            }
        }

        private void Owner_OnShowCompleteEvent()
        {
            if(ResetPropertiesAfterShow)
            {
                ResetGameObjectToStartingState();
            }
        }

        private void Owner_OnHideCompleteEvent()
        {
            if(ResetPropertiesAfterHide)
            {
                ResetGameObjectToStartingState();
            }
        }

        private void OnDestroy()
        {
            if(Owner)
            {
                Owner.OnShowStartEvent -= Owner_OnAnimationStartEvent;
                Owner.OnHideStartEvent -= Owner_OnAnimationStartEvent;
                Owner.OnShowCompleteEvent -= Owner_OnShowCompleteEvent;
                Owner.OnHideCompleteEvent -= Owner_OnHideCompleteEvent;
            }
        }

        private void Owner_OnAnimationStartEvent()
        {
            if(IsListElement && ResetStartingPropertiesBeforeAnimating)
            {
                SetupReferencesAndStartingValues();
            }
        }

        public void SetupReferencesAndStartingValues()
        {
            //update references
            RectTransform = transform as RectTransform;
            if(HasImageTween && !Image)
            {
                Image = GetComponent<Image>();
                if(!Image)
                {
                    Debug.LogError("[TweenAnimator] It has image fade animation, but doesn't have image component");
                }
            }
            if(HasCanvasGroupTween && !CanvasGroup)
            {
                CanvasGroup = GetComponent<CanvasGroup>();
                if(!CanvasGroup)
                {
                    CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            //save starting values
            StartImageAlpha = Image ? Image.color.a : 1.0f;
            StartCanvasGroupAlpha = CanvasGroup ? CanvasGroup.alpha : 1.0f;
            StartAnchoredPosition = RectTransform.anchoredPosition;
            StartRectSize = RectTransform.sizeDelta;
            StartLocalScale = RectTransform.localScale;
        }

        public void ResetGameObjectToStartingState()
        {
            if(RectTransform && HasPositionsTween)
            {
                RectTransform.anchoredPosition = StartAnchoredPosition;
            }
            if(HasScaleTween)
            {
                RectTransform.localScale = StartLocalScale;
            }
            if(CanvasGroup && HasCanvasGroupTween)
            {
                CanvasGroup.alpha = 1.0f;
            }
            if(Image && HasImageTween)
            {
                Color c = Image.color;
                c.a = StartImageAlpha;
                Image.color = c;
            }
        }

        public void UpdateAnimationList()
        {
            if(AnimatorPreset != null)
            {
                Animations = AnimatorPreset.animations.ConvertAll(x => x.Clone).ToList();
            }
            HasPositionsTween = Animations.Any(x => x.Type == TweenAnimationType.Position);
            HasScaleTween = Animations.Any(x => x.Type == TweenAnimationType.Scale);
            HasCanvasGroupTween = Animations.Any(x => x.Type == TweenAnimationType.CanvasGroupAlpha);
            HasImageTween = Animations.Any(x => x.Type == TweenAnimationType.ImageAlpha);
        }

        public Sequence Show()
        {
            OnShowStartEvent?.Invoke();
            CurrentSequence = DOTween.Sequence();
            foreach(TweenAnimation animation in Animations)
            {
                CurrentSequence.Join(animation.Show(this, false));
            }
            CurrentSequence.OnComplete(() =>
            {
                OnShowCompleteEvent?.Invoke();
                if(!Owner && ResetPropertiesAfterShow)
                {
                    ResetGameObjectToStartingState();
                }
            });
            return CurrentSequence;
        }

        public Sequence Hide()
        {
            OnHideStartEvent?.Invoke();
            CurrentSequence = DOTween.Sequence();
            foreach(TweenAnimation animation in Animations)
            {
                CurrentSequence.Join(animation.Hide(this, false));
            }
            CurrentSequence.OnComplete(() =>
            {
                OnHideCompleteEvent?.Invoke();
                if(!Owner && ResetPropertiesAfterHide)
                {
                    ResetGameObjectToStartingState();
                }
            });
            return CurrentSequence;
        }

        public void ShowImmediate()
        {
            foreach(TweenAnimation animation in Animations)
            {
                animation.Show(this, true);
            }
        }

        public void HideImmediate()
        {
            foreach(TweenAnimation animation in Animations)
            {
                animation.Hide(this, true);
            }
        }

        public void Kill()
        {
            if(CurrentSequence != null)
            {
                CurrentSequence.Kill();
            }
        }
    }
}