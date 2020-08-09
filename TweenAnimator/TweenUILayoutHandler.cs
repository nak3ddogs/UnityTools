using UnityEngine;
using UnityEngine.UI;

namespace Zos.Core.TweenAnimator
{
    /// <summary>
    /// It disables the layout elements when tween animation is on going
    /// </summary>
    [AddComponentMenu("UI/Animator/Layout Handler")]
    public class TweenUILayoutHandler : MonoBehaviour, ITweenBranchElement
    {
        private TweenBranch Owner;
        private ContentSizeFitter ContentSizeFitter;
        private HorizontalOrVerticalLayoutGroup LayoutGroup;

        public void Initialize(TweenBranch branch)
        {
            Owner = branch;
            ContentSizeFitter = GetComponent<ContentSizeFitter>();
            LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            Owner.OnShowStartEvent -= Owner_OnAnimationStartEvent;
            Owner.OnHideStartEvent -= Owner_OnAnimationStartEvent;
            Owner.OnShowCompleteEvent -= Owner_OnAnimationEndEvent;
            Owner.OnHideCompleteEvent -= Owner_OnAnimationEndEvent;
            Owner.OnShowStartEvent += Owner_OnAnimationStartEvent;
            Owner.OnHideStartEvent += Owner_OnAnimationStartEvent;
            Owner.OnShowCompleteEvent += Owner_OnAnimationEndEvent;
            Owner.OnHideCompleteEvent += Owner_OnAnimationEndEvent;
        }

        private void OnDestroy()
        {
            Owner.OnShowStartEvent -= Owner_OnAnimationStartEvent;
            Owner.OnHideStartEvent -= Owner_OnAnimationStartEvent;
            Owner.OnShowCompleteEvent -= Owner_OnAnimationEndEvent;
            Owner.OnHideCompleteEvent -= Owner_OnAnimationEndEvent;
        }

        private void Owner_OnAnimationStartEvent()
        {
            if(ContentSizeFitter)
            {
                ContentSizeFitter.enabled = false;
            }
            if(LayoutGroup)
            {
                LayoutGroup.enabled = false;
            }
        }

        private void Owner_OnAnimationEndEvent()
        {
            if(ContentSizeFitter)
            {
                ContentSizeFitter.enabled = true;
            }
            if(LayoutGroup)
            {
                LayoutGroup.enabled = true;
            }
        }
    }
}