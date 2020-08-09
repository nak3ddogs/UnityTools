using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zos.Core.TweenAnimator
{
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("UI/Animator/Branch")]
    public class TweenBranch : MonoBehaviour
    {
        //settings
        [Tooltip("It is necessary if there are list elements")]
        public bool ReInitializeBeforeAnimation = false;
        public bool DisableInteractionWhenAnimating = true;
        public bool ReversedListAnimationOrderOnHide = true;
        //events
        public event System.Action OnShowStartEvent = null;
        public event System.Action OnShowCompleteEvent = null;
        public event System.Action OnHideStartEvent = null;
        public event System.Action OnHideCompleteEvent = null;
        //variables
        private List<TweenAnimator> Animators = new List<TweenAnimator>();
        private List<ITweenBranchElement> TweenBranchElements = new List<ITweenBranchElement>();
        private CanvasGroup CanvasGroup = null;
        private Sequence CurrentSequence = null;
        public bool IsAnimating
        {
            get
            {
                return CurrentSequence != null;
            }
        }

        //helper properties
        public float AnimationShowFullDuration
        {
            get
            {
                if(Animators.Count == 0)
                {
                    return 0.0f;
                }
                return Animators.Max(x => x.FullDurationShow);
            }
        }

        public float AnimationHideFullDuration
        {
            get
            {
                if(Animators.Count == 0)
                {
                    return 0.0f;
                }
                return Animators.Max(x => x.FullDurationHide);
            }
        }

        private void Start()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            Initialize();
        }

        private void Initialize()
        {
            TweenBranchElements.Clear();
            RecursiveComponentSearch<ITweenBranchElement>(TweenBranchElements);
            Animators.Clear();
            RecursiveAnimatorSearch(Animators);

            foreach(ITweenBranchElement element in TweenBranchElements)
            {
                element.Initialize(this);
            }

            //handle list elements
            float showDelay = 0.0f;
            foreach(TweenAnimator animator in Animators)
            {
                if(animator.IsListElement)
                {
                    animator.CurrentListOffsetAtShow = showDelay;
                    showDelay += animator.ShowDelayOffset;
                    if(!ReversedListAnimationOrderOnHide)
                    {
                        animator.CurrentListOffsetAtHide = showDelay;
                    }
                }
            }
            if(ReversedListAnimationOrderOnHide)
            {
                float hideDelay = 0.0f;
                for(int i = Animators.Count - 1; i >= 0; i--)
                {
                    TweenAnimator animator = Animators[i];
                    if(animator.IsListElement)
                    {
                        animator.CurrentListOffsetAtHide = hideDelay;
                        hideDelay += animator.ShowDelayOffset;
                    }
                }
            }
        }

        public void Show()
        {
            OnShowStartEvent?.Invoke();
            if(CurrentSequence != null)
            {
                CurrentSequence.Kill();
            }
            if(ReInitializeBeforeAnimation)
            {
                Initialize();
            }
            if(DisableInteractionWhenAnimating)
            {
                CanvasGroup.interactable = false;
            }
            CurrentSequence = DOTween.Sequence();
            foreach(TweenAnimator animator in Animators)
            {
                CurrentSequence.Join(animator.Show());
            }
            CurrentSequence.OnComplete(() =>
            {
                CanvasGroup.interactable = true;
                OnShowCompleteEvent?.Invoke();
                CurrentSequence = null;
            });
        }

        public void ShowImmediate(bool triggerEvents)
        {
            if(triggerEvents)
            {
                OnShowStartEvent?.Invoke();
            }
            if(CurrentSequence != null)
            {
                CurrentSequence.Kill();
                CurrentSequence = null;
            }
            if(ReInitializeBeforeAnimation)
            {
                Initialize();
            }
            foreach(TweenAnimator animator in Animators)
            {
                animator.ShowImmediate();
            }
            CanvasGroup.interactable = true;
            if(triggerEvents)
            {
                OnShowCompleteEvent?.Invoke();
            }
        }

        public void Hide()
        {
            OnHideStartEvent?.Invoke();
            if(CurrentSequence != null)
            {
                CurrentSequence.Kill();
            }
            if(ReInitializeBeforeAnimation)
            {
                Initialize();
            }
            if(DisableInteractionWhenAnimating)
            {
                CanvasGroup.interactable = false;
            }
            CurrentSequence = DOTween.Sequence();
            foreach(TweenAnimator animator in Animators)
            {
                CurrentSequence.Join(animator.Hide());
            }
            CurrentSequence.OnComplete(() =>
            {
                CanvasGroup.interactable = true;
                OnHideCompleteEvent?.Invoke();
                CurrentSequence = null;
            });
        }

        public void HideImmediate(bool triggerEvents)
        {
            if(triggerEvents)
            {
                OnHideStartEvent?.Invoke();
            }
            if(CurrentSequence != null)
            {
                CurrentSequence.Kill();
                CurrentSequence = null;
            }
            if(ReInitializeBeforeAnimation)
            {
                Initialize();
            }
            foreach(TweenAnimator animator in Animators)
            {
                animator.HideImmediate();
            }
            CanvasGroup.interactable = true;
            if(triggerEvents)
            {
                OnHideCompleteEvent?.Invoke();
            }
        }

        public void RecursiveAnimatorSearch(List<TweenAnimator> list)
        {
            void InnerSearch(Transform target)
            {
                foreach(Transform tr in target)
                {
                    TweenAnimator animator = tr.GetComponent<TweenAnimator>();
                    TweenBranch branch = tr.GetComponent<TweenBranch>();

                    if(branch == null)
                    {
                        if(animator != null && !animator.IgnoreTweenBranch)
                        {
                            list.Add(animator);
                        }
                        InnerSearch(tr);
                    }
                }
            }

            TweenAnimator anim = transform.GetComponent<TweenAnimator>();
            if(anim != null && !anim.IgnoreTweenBranch)
            {
                list.Add(anim);
            }
            InnerSearch(transform);
        }

        private void RecursiveComponentSearch<T>(List<T> list)
        {
            void InnerSearch(Transform target)
            {
                foreach(Transform tr in target)
                {
                    T component = tr.GetComponent<T>();
                    TweenBranch branch = tr.GetComponent<TweenBranch>();

                    if(branch == null)
                    {
                        if(component != null)
                        {
                            list.Add(component);
                        }
                        InnerSearch(tr);
                    }
                }
            }

            T rootComponent = transform.GetComponent<T>();
            if(rootComponent != null)
            {
                list.Add(rootComponent);
            }
            InnerSearch(transform);
        }
    }
}