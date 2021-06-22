using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public enum AnimalTypeEnum
{
    Dog,
    Cat,
    Fox,
    Racoon,
    None
}

public abstract class AnimalBase : MonoBehaviour
{
    protected ReactiveProperty<AnimalTypeEnum> _AnimalType = new ReactiveProperty<AnimalTypeEnum>();
    public IReadOnlyReactiveProperty<AnimalTypeEnum> AnimalType => _AnimalType;

    [SerializeField] protected AnimalTypeEnum AnimalTypeEnum;
    public float MoveSpeed;

    public bool Draging;

    [SerializeField]
    protected BoolReactiveProperty _InFence = new BoolReactiveProperty(false);
    public IReadOnlyReactiveProperty<bool> InFence => _InFence;
}
