using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
    [HideInInspector] public static AnimationsManager instance;

    public List<AnimationEvent> animations = new List<AnimationEvent>();

    [SerializeField] Animator logoAnimator;
    int animationIndex = 1;

    void Start()
    {
        PlayAnimation();
    }

    public void TriggerAnimation()
    {
        animationIndex++;

        PlayAnimation();
    }

    void PlayAnimation()
    {
        
    }
}

public enum ACVariableType {
    Trigger,
    Bool,
}

[Serializable]
public struct AnimationEvent
{
    public AnimationEventTrigger animationEventTrigger;
    public Animator animator;
    public Animation animation;
}

[Serializable]
public struct AnimationEventTrigger
{
    public ACVariableType type;
    public string value;
} 
