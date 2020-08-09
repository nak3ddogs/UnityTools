using System.Collections.Generic;
using UnityEngine;

namespace Zos.Core.TweenAnimator
{
    [CreateAssetMenu(fileName = "TweenAnimatorPresetSO",menuName ="Zos/Tween Animator/Tween Animator Preset")]
    public class TweenAnimatorPresetSO : ScriptableObject
    {
        public List<TweenAnimation> animations = new List<TweenAnimation>();
    }
}