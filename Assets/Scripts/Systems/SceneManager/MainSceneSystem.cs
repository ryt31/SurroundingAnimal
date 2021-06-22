using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class MainSceneSystem : MonoBehaviour
{
    private GameManager GameManager = null;
    private PhaseManager PhaseManager = null;

    private AnimalManager AnimalManager = null;

    [SerializeField] private Image ArrartImg = null;
    [SerializeField] private Text ArrTxt = null;
    [SerializeField] private AudioSource ArrartAudioSource = null;
    [SerializeField] private AudioSource BGMAudio = null;

    //[SerializeField] private Text CountTxt = null;
 
    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        PhaseManager = GameObject.FindWithTag("Manager").GetComponent<PhaseManager>();
        AnimalManager = GameObject.FindWithTag("Manager").GetComponent<AnimalManager>();
        PhaseManager.TimeOfNextPhase
            .Where(_ => _ == 10)
            .Subscribe(_ => ShowArrart());
        PhaseManager.TimeOfNextPhase
            .Where(_ => _ == 9)
            .Subscribe(_ => HideArrart());
        ArrartAudioSource.volume = GameManager.SEVolume.Value/100;
        BGMAudio.volume = GameManager.BGMVolume.Value/100;
        HideArrart();

        /*AnimalManager.InLoad_AnimalCount
            .Subscribe(count => CountTxt.text = count.ToString()+" : "+AnimalManager.InFenceAnimalsCount());*/
    }

    void ShowArrart()
    {
        ArrartImg.enabled = true;
        ArrTxt.enabled = true;
        ArrartAudioSource.Play();
    }
    void HideArrart()
    {
        ArrartImg.enabled = false;
        ArrTxt.enabled = false;
    }
}
