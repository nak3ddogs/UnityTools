using DG.Tweening;
using UnityEngine;

namespace Zos.Core.TweenAnimator
{
    public enum TweenAnimationType
    {
        Position,
        Scale,
        ImageAlpha,
        CanvasGroupAlpha,
        RectSize,
    }

    [System.Serializable]
    public class TweenAnimation
    {
        public TweenAnimationType Type = default;
        //General
        public Ease Ease = Ease.InOutQuad;
        public float Delay = 0.0f;
        public float Duration = 1.0f;
        public bool DoShow = true;
        public bool DoHide = true;

        //Position
        public bool IsRelativeMove = true;
        public Vector2 HiddenPosition = Vector2.zero;

        //Scale
        public Vector2 HiddenScale = Vector2.zero;

        //FadeImage
        public float ImageHiddenAlpha = 0.0f;

        //CanvasGroupFade
        public float CanvasGroupHiddenAlpha = 0.0f;

        //RectSize
        public Vector2 HiddenDeltaSize = Vector2.zero;

        //Local Variables
        private TweenAnimator Owner = null;

        public float ShowDelay
        {
            get
            {
                return Delay + Owner.ShowPreDelay + Owner.CurrentListOffsetAtShow;
            }
        }

        public float HideDelay
        {
            get
            {
                return Delay + Owner.HidePreDelay + Owner.CurrentListOffsetAtHide;
            }
        }

        public TweenAnimation Clone
        {
            get
            {
                TweenAnimation a = this.MemberwiseClone() as TweenAnimation;
                return a;
            }
        }

        public DG.Tweening.Tween Show(TweenAnimator owner, bool immediate)
        {
            if(!DoShow)
            {
                return null;
            }
            this.Owner = owner;
            return Animate(true, immediate);
        }

        public DG.Tweening.Tween Hide(TweenAnimator owner, bool immediate)
        {
            if(!DoHide)
            {
                return null;
            }
            this.Owner = owner;
            return Animate(false, immediate);
        }

        private DG.Tweening.Tween Animate(bool toShow, bool immediate)
        {
            switch(Type)
            {
                case TweenAnimationType.Position:
                    return AnimatePosition(toShow, immediate);
                case TweenAnimationType.Scale:
                    return AnimateScale(toShow, immediate);
                case TweenAnimationType.ImageAlpha:
                    return AnimateImageAlpha(toShow, immediate);
                case TweenAnimationType.CanvasGroupAlpha:
                    return AnimateCanvasAlpha(toShow, immediate);
                case TweenAnimationType.RectSize:
                    return AnimateDeltaSize(toShow, immediate);
            }
            return null;
        }

        private DG.Tweening.Tween AnimatePosition(bool toShow, bool immediate)
        {
            if(immediate)
            {
                if(toShow)
                {
                    Owner.RectTransform.anchoredPosition = Owner.StartAnchoredPosition;
                }
                else
                {
                    Vector2 hiddenPosition = (IsRelativeMove) ? (Owner.StartAnchoredPosition + HiddenPosition) : HiddenPosition;
                    Owner.RectTransform.anchoredPosition = hiddenPosition;
                }
                return null;
            }
            else
            {
                if(toShow)
                {
                    DG.Tweening.Core.TweenerCore<Vector2, Vector2, DG.Tweening.Plugins.Options.VectorOptions> tw = Owner.RectTransform.DOAnchorPos(Owner.StartAnchoredPosition, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(ShowDelay);
                    return tw;
                }
                else
                {
                    Vector2 hiddenPosition = (IsRelativeMove) ? (Owner.StartAnchoredPosition + HiddenPosition) : HiddenPosition;
                    DG.Tweening.Tween tw = Owner.RectTransform.DOAnchorPos(hiddenPosition, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(HideDelay);
                    return tw;
                }
            }
        }

        private DG.Tweening.Tween AnimateScale(bool toShow, bool immediate)
        {
            if(immediate)
            {
                if(toShow)
                {
                    Owner.RectTransform.localScale = Owner.StartLocalScale;
                }
                else
                {
                    Owner.RectTransform.localScale = HiddenScale;
                }
                return null;
            }
            else
            {
                if(toShow)
                {
                    DG.Tweening.Tween tw = Owner.RectTransform.DOScale(Owner.StartLocalScale, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(ShowDelay);
                    return tw;
                }
                else
                {
                    DG.Tweening.Tween tw = Owner.RectTransform.DOScale(HiddenScale, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(HideDelay);
                    return tw;
                }
            }
        }

        private DG.Tweening.Tween AnimateImageAlpha(bool toShow, bool immediate)
        {
            if(immediate)
            {
                if(toShow)
                {
                    Color color = Owner.Image.color;
                    color.a = Owner.StartImageAlpha;
                    Owner.Image.color = color;
                }
                else
                {
                    Color color = Owner.Image.color;
                    color.a = ImageHiddenAlpha;
                    Owner.Image.color = color;
                }
                return null;
            }
            else
            {
                if(toShow)
                {
                    DG.Tweening.Tween tw = Owner.Image.DOFade(Owner.StartImageAlpha, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(ShowDelay);
                    return tw;
                }
                else
                {
                    DG.Tweening.Tween tw = Owner.Image.DOFade(ImageHiddenAlpha, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(HideDelay);
                    return tw;
                }
            }
        }

        private DG.Tweening.Tween AnimateCanvasAlpha(bool toShow, bool immediate)
        {
            if(immediate)
            {
                if(toShow)
                {
                    Owner.CanvasGroup.alpha = Owner.StartCanvasGroupAlpha;
                }
                else
                {
                    Owner.CanvasGroup.alpha = CanvasGroupHiddenAlpha;
                }
                return null;
            }
            else
            {
                if(toShow)
                {
                    DG.Tweening.Tween tw = Owner.CanvasGroup.DOFade(Owner.StartCanvasGroupAlpha, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(ShowDelay);
                    return tw;
                }
                else
                {
                    DG.Tweening.Tween tw = Owner.CanvasGroup.DOFade(CanvasGroupHiddenAlpha, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(HideDelay);
                    return tw;
                }
            }
        }

        private DG.Tweening.Tween AnimateDeltaSize(bool toShow, bool immediate)
        {
            if(immediate)
            {
                if(toShow)
                {
                    Owner.RectTransform.sizeDelta = Owner.StartRectSize;
                }
                else
                {
                    Owner.RectTransform.sizeDelta = HiddenDeltaSize;
                }
                return null;
            }
            else
            {
                if(toShow)
                {
                    DG.Tweening.Tween tw = Owner.RectTransform.DOSizeDelta(Owner.StartRectSize, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(ShowDelay);
                    return tw;
                }
                else
                {
                    DG.Tweening.Tween tw = Owner.RectTransform.DOSizeDelta(HiddenDeltaSize, Duration);
                    tw.SetEase(Ease);
                    tw.SetDelay(HideDelay);
                    return tw;
                }
            }
        }
    }
}