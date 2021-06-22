using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class ViewAnimalSceneManager : MonoBehaviour
{
    private GameManager GameManager;

    private AudioSource BGMAudio = null;

    [SerializeField] private float RotateSpeed = 1;
    public AnimalTypeEnum ShowingAnimal = AnimalTypeEnum.Dog;
    [SerializeField] private GameObject dog, cat, fox, racoon;

    [SerializeField]
    private float _Inertia = 0, DecrationAmount = 1;
    private Vector3 _CurrentPos = Vector3.zero;
    private Vector3 _PrePos = Vector3.zero;
    

    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        BGMAudio = GetComponent<AudioSource>();
        BGMAudio.volume = GameManager.BGMVolume.Value / 100;

        this.UpdateAsObservable()
            .Subscribe(_ => RotateAnimal());

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (Mathf.Abs(_Inertia) < 10)
                {
                    _Inertia = 0;
                }else if (_Inertia < 0)
                {
                    _Inertia += DecrationAmount;
                }
                else if(_Inertia > 0)
                {
                    _Inertia -= DecrationAmount;
                }
            });

        this.UpdateAsObservable()
            .Subscribe(_ => OnDraging());
    }

    public void GotoTitleScene() => GameManager.ChangeScene(SceneType.Title);

    private void OnDraging()
    {
        _CurrentPos = Input.mousePosition;
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            _Inertia = (_CurrentPos.x - _PrePos.x) / Time.deltaTime;
        }
        _PrePos = _CurrentPos;
    }

    private void RotateAnimal()
    {
        GameObject animal = null;
        switch (ShowingAnimal)
        {
            case AnimalTypeEnum.Dog:
                animal = dog;
                break;
            case AnimalTypeEnum.Cat:
                animal = cat;
                break;
            case AnimalTypeEnum.Fox:
                animal = fox;
                break;
            case AnimalTypeEnum.Racoon:
                animal = racoon;
                break;
            default:
                break;
        }
        animal.transform.Rotate(0, -_Inertia * Time.deltaTime * RotateSpeed, 0);
    }

    public void ChangeShowAnimal(string animalName)
    {
        switch (animalName)
        {
            case "dog":
                ShowingAnimal = AnimalTypeEnum.Dog;
                break;
            case "cat":
                ShowingAnimal = AnimalTypeEnum.Cat;
                break;
            case "fox":
                ShowingAnimal = AnimalTypeEnum.Fox;
                break;
            case "racoon":
                ShowingAnimal = AnimalTypeEnum.Racoon;
                break;
            default:
                break;
        }
    }
}
