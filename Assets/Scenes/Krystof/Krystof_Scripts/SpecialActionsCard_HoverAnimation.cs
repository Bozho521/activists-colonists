using System;
using UnityEngine;

public class SpecialActionsCard_HoverAnimation : MonoBehaviour
{
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnMouseEnter()
    {
        _animator.SetInteger("PlayerHover", 1);
    }

    private void OnMouseExit()
    {
        _animator.SetInteger("PlayerHover", 2);
    }
}
