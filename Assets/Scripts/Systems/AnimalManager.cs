using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

/// <summary>
/// 動物の生成、管理を行う
/// </summary>
public class AnimalManager : MonoBehaviour
{
    [SerializeField] private int GameEndAnimalCount = 20;

    private ReactiveProperty<int> _InLoad_AnimalCount = new ReactiveProperty<int>(0);
    public IReadOnlyReactiveProperty<int> InLoad_AnimalCount => _InLoad_AnimalCount;

    public PopableArea PopableType { get; private set; }

    public float KickOutPercent = 20.0f;

    [SerializeField] private GameObject AnimalParent = null;
    /// <summary> 動物のプレハブ </summary>
    public GameObject Dog, Cat, Fox, Raccoon;

    public Vector3 Up, Down, Right, Left;

    [SerializeField, Range(0, 360)]
    private float WidthAngle = 0;

    private AnimalScore _AnimalScore = new AnimalScore();
    private GameManager GameManager { get; set; }

    [SerializeField] private List<GameObject> InFDog = new List<GameObject>();
    [SerializeField] private List<GameObject> InFCat = new List<GameObject>();
    [SerializeField] private List<GameObject> InFFox = new List<GameObject>();
    [SerializeField] private List<GameObject> InFRacoon = new List<GameObject>();

    private Subject<Unit> _MissDog = new Subject<Unit>();
    public IObservable<Unit> MissDog => _MissDog;
    private Subject<Unit> _MissCat = new Subject<Unit>();
    public IObservable<Unit> MissCat => _MissCat;
    private Subject<Unit> _MissFox = new Subject<Unit>();
    public IObservable<Unit> MissFox => _MissFox;
    private Subject<Unit> _MissRacoon = new Subject<Unit>();
    public IObservable<Unit> MissRacoon => _MissRacoon;

    void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        GameManager.NextScene
            .Subscribe(_ => GameManager.SetScore(_AnimalScore));

        GameManager.NextScene
            .Where(_ => _ == SceneType.Result)
            .Subscribe(_ => ToResult());

        InLoad_AnimalCount
            .Where(count => GameEndAnimalCount <= count)
            .Subscribe(count => GameManager.ChangeScene(SceneType.Result));

        //InLoad_AnimalCount
        //    .Subscribe(_ => Debug.Log("道にいる動物の数 : " + _));// +", スコア"+AnimalScore))
    }

    /// <summary> 動物を生成する </summary>
    public void InstantateAnimal()
    {
        var Obj = ChoiceAnimal();
        Obj = Instantiate(Obj, AnimalParent.transform);
        Obj.transform.position = PopPos();
        Obj.transform.LookAt(new Vector3(0, Obj.transform.position.y, 0));
        float angle = UnityEngine.Random.Range(-WidthAngle / 2, WidthAngle / 2);
        Obj.transform.Rotate(new Vector3(0, angle, 0));
        Animal animal = Obj.GetComponent<Animal>();
        animal.OnMiss
            .SkipLatestValueOnSubscribe()
            .Subscribe(animalType => EscapesAnimals(animalType));
        animal.SetVol(GameManager.SEVolume.Value / 100);
        UpdateInLoad_AnimalCount();
    }

    /// <summary> 動物を生成する </summary>
    /// <param name="popArea">生成する位置</param>
    public void InstantateAnimal(PopArea popArea)
    {
        var Obj = ChoiceAnimal();
        Obj = Instantiate(Obj, AnimalParent.transform);
        Obj.transform.position = PopPos(popArea);
        Obj.transform.LookAt(new Vector3(0, Obj.transform.position.y, 0));
        float angle = UnityEngine.Random.Range(-WidthAngle / 2, WidthAngle / 2);
        Obj.transform.Rotate(new Vector3(0, angle, 0));
        Animal animal = Obj.GetComponent<Animal>();
        animal.OnMiss
            .SkipLatestValueOnSubscribe()
            .Subscribe(animalType => EscapesAnimals(animalType));
        animal.SetVol(GameManager.SEVolume.Value / 100);
        UpdateInLoad_AnimalCount();
    }

    /// <summary> 出現動物のランダム化 </summary>
    private GameObject ChoiceAnimal()
    {
        int rand = UnityEngine.Random.Range(0, 4);
        GameObject Obj;
        switch (rand)
        {
            case 0:
                Obj = Dog;
                break;
            case 1:
                Obj = Cat;
                break;
            case 2:
                Obj = Fox;
                break;
            case 3:
                Obj = Raccoon;
                break;
            default:
                Obj = Dog;
                break;
        }
        return Obj;
    }

    /// <summary> 出現位置をランダムで返す </summary>
    private Vector3 PopPos()
    {
        int Rand;
        switch (PopableType)
        {
            case PopableArea.Up:
                return Up;
            case PopableArea.Up_Down:
                if (UnityEngine.Random.Range(0, 2) == 0) return Up;
                else return Down;
            case PopableArea.Up_Down_Right:
                Rand = UnityEngine.Random.Range(0, 3);
                if (Rand == 0) return Up;
                else if (Rand == 1) return Down;
                else return Right;
            case PopableArea.All:
                Rand = UnityEngine.Random.Range(0, 4);
                switch (Rand)
                {
                    case 0:
                        return Up;
                    case 1:
                        return Down;
                    case 2:
                        return Right;
                    case 3:
                        return Left;
                    default:
                        return Up;
                }
            default:
                return Up;
        }
    }

    private Vector3 PopPos(PopArea popArea)
    {
        switch (popArea)
        {
            case PopArea.Up:
                return Up;
            case PopArea.Down:
                return Down;
            case PopArea.Right:
                return Right;
            case PopArea.Left:
                return Left;
            default:
                return Vector3.zero;
        }
    }

    public void ChangePopPos(PopableArea popType) => PopableType = popType;

    public void AddScore(GameObject gameObject, AnimalTypeEnum animalType)
    {
        switch (animalType)
        {
            case AnimalTypeEnum.Dog:
                _AnimalScore.Dog += 1;
                RemoveInFenceList(gameObject, animalType);
                break;
            case AnimalTypeEnum.Cat:
                _AnimalScore.Cat += 1;
                RemoveInFenceList(gameObject, animalType);
                break;
            case AnimalTypeEnum.Fox:
                _AnimalScore.Fox += 1;
                RemoveInFenceList(gameObject, animalType);
                break;
            case AnimalTypeEnum.Racoon:
                _AnimalScore.Raccoon += 1;
                RemoveInFenceList(gameObject, animalType);
                break;
            case AnimalTypeEnum.None:
                break;
            default:
                break;
        }
    }

    public void UpdateInLoad_AnimalCount() => _InLoad_AnimalCount.Value = GameObject.FindGameObjectsWithTag("free").Length;

    public bool AddInFList(GameObject addObject)
    {
        var animalType = addObject.GetComponent<Animal>().AnimalType.Value;
        switch (animalType)
        {
            case AnimalTypeEnum.Dog:
                if (InFDog.Contains(addObject)) { return false; }
                else { InFDog.Add(addObject); }
                break;
            case AnimalTypeEnum.Cat:
                if (InFCat.Contains(addObject)) { return false; }
                else { InFCat.Add(addObject); }
                break;
            case AnimalTypeEnum.Fox:
                if (InFFox.Contains(addObject)) { return false; }
                else { InFFox.Add(addObject); }
                break;
            case AnimalTypeEnum.Racoon:
                if (InFRacoon.Contains(addObject)) { return false; }
                else { InFRacoon.Add(addObject); }
                break;
        }
        Debug.Log(addObject.name + "をInFList<" + animalType + ">に追加しました");
        return true;
    }

    public void RemoveInFenceList(GameObject removeObject, AnimalTypeEnum animalType)
    {
        switch (animalType)
        {
            case AnimalTypeEnum.Dog:
                if (InFDog.Remove(removeObject))
                {
                    Debug.Log(removeObject.name + "をInFList<" + animalType + ">から出しました");
                }
                else
                {
                    Debug.Log("InFList<" + animalType + ">の要素数は" + InFDog.Count);
                }
                break;
            case AnimalTypeEnum.Cat:
                if (InFCat.Remove(removeObject))
                {
                    Debug.Log(removeObject.name + "をInFList<" + animalType + ">から出しました");
                }
                else
                {
                    Debug.Log("InFList<" + animalType + ">の要素数は" + InFCat.Count);
                }
                break;
            case AnimalTypeEnum.Fox:
                if (InFFox.Remove(removeObject))
                {
                    Debug.Log(removeObject.name + "をInFList<" + animalType + ">から出しました");
                }
                else
                {
                    Debug.Log("InFList<" + animalType + ">の要素数は" + InFFox.Count);
                }
                break;
            case AnimalTypeEnum.Racoon:
                if (InFRacoon.Remove(removeObject))
                {
                    Debug.Log(removeObject.name + "をInFList<" + animalType + ">から出しました");
                }
                else
                {
                    Debug.Log("InFList<" + animalType + ">の要素数は" + InFRacoon.Count);
                }
                break;
            case AnimalTypeEnum.None:
                break;
            default:
                break;
        }
    }

    /// <summary> 柵内の動物を開放する </summary>
    /// <param name="animalType"></param>
    private void EscapesAnimals(AnimalTypeEnum animalType)
    {
        Debug.Log(animalType+"にいる動物を開放します");
        switch (animalType)
        {
            case AnimalTypeEnum.Dog:
                foreach (var d in InFDog)
                {
                    d.GetComponent<Animal>().Escape();
                }
                break;
            case AnimalTypeEnum.Cat:
                foreach (var c in InFCat)
                {
                    c.GetComponent<Animal>().Escape();
                }
                break;
            case AnimalTypeEnum.Fox:
                foreach (var f in InFFox)
                {
                    f.GetComponent<Animal>().Escape();
                }
                break;
            case AnimalTypeEnum.Racoon:
                foreach (var r in InFRacoon)
                {
                    r.GetComponent<Animal>().Escape();
                }
                break;
            case AnimalTypeEnum.None:
                break;
            default:
                break;
        }
    }

    private void ToResult()
    {
        foreach (var d in InFDog)
        {
            AddScore(d,AnimalTypeEnum.Dog);
        }
        foreach (var c in InFCat)
        {
            AddScore(c,AnimalTypeEnum.Cat);
        }
        foreach (var f in InFFox)
        {
            AddScore(f,AnimalTypeEnum.Fox);
        }
        foreach (var r in InFRacoon)
        {
            AddScore(r,AnimalTypeEnum.Racoon);
        }
    }

    //現在のスコアを返す
    public int CurrentScore()
    {
        return _AnimalScore.Cat + _AnimalScore.Dog + _AnimalScore.Fox + _AnimalScore.Raccoon;
    }
    //現在のスコアを返す
    public int CurrentScore(AnimalTypeEnum animalType)
    {
        var score = 0;
        switch (animalType)
        {
            case AnimalTypeEnum.Dog:
                score = _AnimalScore.Dog;
                break;
            case AnimalTypeEnum.Cat:
                score = _AnimalScore.Cat;
                break;
            case AnimalTypeEnum.Fox:
                score = _AnimalScore.Fox;
                break;
            case AnimalTypeEnum.Racoon:
                score = _AnimalScore.Raccoon;
                break;
            case AnimalTypeEnum.None:
                break;
            default:
                break;
        }
        return score;
    }

    public int InFenceAnimalsCount() => InFDog.Count + InFCat.Count + InFFox.Count + InFRacoon.Count;
}

public enum PopableArea{
    Up,
    Up_Down,
    Up_Down_Right,
    All,
}

public enum PopArea
{
    Up, Down, Right, Left
}