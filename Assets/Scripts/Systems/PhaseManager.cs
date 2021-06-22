using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PhaseManager : MonoBehaviour
{
    [SerializeField]
    private IntReactiveProperty _PhaseNum = new IntReactiveProperty(0);
    public IReadOnlyReactiveProperty<int> PhaseNum => _PhaseNum;

    private AnimalManager AnimalManager { get; set; }

    [SerializeField]
    private float InstantateInterval = 1.8f;
    [SerializeField] private float StartInterval = 2.0f;
    [SerializeField] private int PhaseInterval = 30;
    private float Timer = 0;
    private int NowPhaseTime = 0;
    /// <summary> ゲームが始まってからの経過時間 </summary>
    public float GameingTime { get; private set; }
    /// <summary> 次のフェーズまでの時間 </summary>
    private IntReactiveProperty _TimeOfNextPhase = new IntReactiveProperty();
    public IReadOnlyReactiveProperty<int> TimeOfNextPhase => _TimeOfNextPhase;

    [SerializeField] private int[] AddPopArea = { 1, 2, 3, 4 };

    void Start()
    {
        AnimalManager = gameObject.GetComponent<AnimalManager>();

        PhaseNum
            .Where(_ => _ == AddPopArea[0])
            .Subscribe(_ => AnimalManager.ChangePopPos(PopableArea.Up));
        PhaseNum
            .Where(_ => _ == AddPopArea[1])
            .Subscribe(_ => AnimalManager.ChangePopPos(PopableArea.Up_Down));
        PhaseNum
            .Where(_ => _ == AddPopArea[2])
            .Subscribe(_ => AnimalManager.ChangePopPos(PopableArea.Up_Down_Right));
        PhaseNum
            .Where(_ => _ == AddPopArea[3])
            .Subscribe(_ => AnimalManager.ChangePopPos(PopableArea.All));

        //動物の生成判定
        /*this.UpdateAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(InstantateInterval))
            .Subscribe(_ => AnimalManager.InstantateAnimals(1));*/

        //フェーズの変更判定
        this.UpdateAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(PhaseInterval))
            .Subscribe(_ => ChangeNextPhase());

        /*this.UpdateAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(1))
            .DoOnSubscribe(_ => NowPhaseTime = NowPhaseTime++)
            .Subscribe(_ => { _TimeOfNextPhase.Value = PhaseInterval - NowPhaseTime; });*/

        //タイマーの管理
        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                NowPhaseTime++;
                _TimeOfNextPhase.Value = PhaseInterval - NowPhaseTime;
            });



        PhaseNum
            .Where(_ => _ > AddPopArea[3])
            .Subscribe(_ =>
            {
                if (InstantateInterval > 0.1f)
                {
                    InstantateInterval = StartInterval - _ * 0.1f + AddPopArea[3] * 0.1f;
                }
                //最低でも0.1秒を保証する
                else if(InstantateInterval != 0.1f)
                {
                    InstantateInterval = 0.1f;
                }
                Debug.Log("Interval : " + InstantateInterval);
            });
    }

    private void Update()
    {
        Timer += Time.deltaTime;
        if (Timer >= InstantateInterval)
        {
            if (AnimalManager.PopableType == PopableArea.All && UnityEngine.Random.Range(1, 11) == 10)
            {
                AnimalManager.InstantateAnimal(PopArea.Up);
                AnimalManager.InstantateAnimal(PopArea.Down);
                AnimalManager.InstantateAnimal(PopArea.Right);
                AnimalManager.InstantateAnimal(PopArea.Left);
            }
            else
            {
                AnimalManager.InstantateAnimal();
            }
            Timer = 0;
        }
    }

    public void ChangeNextPhase()
    {
        _PhaseNum.Value++;
        NowPhaseTime = 0;
    }
}
