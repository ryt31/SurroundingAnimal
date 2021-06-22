using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Animal : AnimalBase
{
    private ReactiveProperty<AnimalTypeEnum> _OnMiss = new ReactiveProperty<AnimalTypeEnum>(AnimalTypeEnum.None);
    public IReadOnlyReactiveProperty<AnimalTypeEnum> OnMiss => _OnMiss;

    private Vector3 screenPoint;
    private Vector3 offset;

    [SerializeField]
    private BoolReactiveProperty _Escaping = new BoolReactiveProperty(false);

    //白いもやもや　(パーティクル)
    [SerializeField] private GameObject ObjStruggle = null;
    [SerializeField] private AudioSource GrapAudioSource = null;
    [SerializeField] private AudioSource LeftAudioSource = null;
    [SerializeField] private AudioSource CorrectAudio = null;
    [SerializeField] private AudioSource MissAudio = null;
    //動物の上にある "せいかい！" とあるオブジェクト
    [SerializeField] private GameObject CorrectObj = null;

    private AnimalManager AnimalManager;
    private AnimalTypeEnum ExistArea = AnimalTypeEnum.None;

    private bool _Hitable = true;

    void Start()
    {
        tag = ("free");

        AnimalManager = GameObject.FindWithTag("Manager").GetComponent<AnimalManager>();

        _AnimalType.Value = AnimalTypeEnum;

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                //まっすぐ移動
                transform.Translate(Vector3.forward * Time.deltaTime * MoveSpeed);
                CheckInFence();
            })
            .AddTo(this.gameObject);

        this.OnTriggerEnterAsObservable()
            .Where(_ => _Escaping.Value == false && _Hitable)
            .Where(playerCol => playerCol.gameObject.CompareTag("Fence"))
            .Subscribe(_ =>
            {
                Debug.Log("柵に接触したため方向転換します");
                ChangeRotate();
            });

        //_OnMiss.Value = AnimalTypeEnum.None;
        //ミスしたとき
        OnMiss
            .SkipLatestValueOnSubscribe()
            .Subscribe(_ => Struggle())
            .AddTo(gameObject);

        //逃走開始から2秒後に逃走モード解除
        this.UpdateAsObservable()
            .Where(_ => _Escaping.Value == true)
            .Do(_ =>
            {
                transform.LookAt(new Vector3(0, transform.position.y, 0));
            })
            .Delay(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                _Escaping.Value = false;
            })
            .AddTo(gameObject);

        InFence
            .Where(_ => _)
            .Delay(TimeSpan.FromSeconds(30))
            .Where(_ => _)
            .Subscribe(_ =>
            {
                Debug.Log("30秒経ったのでスコアを加算します : スコア.." + AnimalManager.CurrentScore());
                AnimalManager.AddScore(gameObject, AnimalType.Value);
                Destroy(gameObject, 0.1f);
            })
            .AddTo(gameObject);

        InFence
            .Pairwise()
            .Where(tf => tf.Current == true && tf.Current != tf.Previous)
            .Do(_ =>
            {
                Debug.Log("正しく動物(" + AnimalType.Value + ")を仕分けました, この動物は" + ExistArea + "に存在してます");
                Debug.Log(CenterPos(ExistArea) + "を見ます");
                transform.LookAt(CenterPos(ExistArea));
                if (AnimalManager.AddInFList(gameObject) && (ExistArea == AnimalTypeEnum.None || ExistArea == AnimalType.Value))
                    {
                        //Debug.Log(OnMiss.Value + "の柵内に想定と異なる動物が入りました");
                        gameObject.tag = "InFence";
                        CorrectObj.SetActive(true);
                        CorrectAudio.Play();
                    }
                _Hitable = false;
            })
            .Delay(TimeSpan.FromSeconds(1))
            .Where(_ => CorrectObj != null)
            .Subscribe(_ =>
            {
                CorrectObj.SetActive(false);
                _Hitable = true;
            })
            .AddTo(gameObject);

        InFence
            .Pairwise()
            .Where(inf => inf.Current == false && inf.Current != inf.Previous)
            .Subscribe(_ =>
            {
                Debug.Log("柵から出たため" + gameObject.name + "をInFenceから出します");
                AnimalManager.RemoveInFenceList(gameObject, AnimalType.Value);
            })
            .AddTo(gameObject);
    }

/* 

        if (Draging == false  && InFence == false)
        {
            this.tag = ("free");
        }

        if(tmp.x < 9.0f && -9.0f < tmp.x && tmp.z < 5.0f && -5.0f < tmp.z)
        {

        }
        else
        {
            Destroy(this, 1.0f);
        }

    }
    */

    void OnMouseDown()
    {
        //カメラから見たオブジェクトの現在位置を画面位置座標に変換
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);

        //取得したscreenPointの値を変数に格納
        float x = Input.mousePosition.x;
        float y = Input.mousePosition.y;

        //オブジェクトの座標からマウス位置(つまりクリックした位置)を引いている。
        //これでオブジェクトの位置とマウスクリックの位置の差が取得できる。
        //ドラッグで移動したときのずれを補正するための計算だと考えれば分かりやすい
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(x, y , screenPoint.z + 1));
        GrapAudioSource.Play();
    }

    void OnMouseDrag()
    {
        Draging = true;
        //ドラッグ時のマウス位置を変数に格納
        float x = Input.mousePosition.x;
        float y = Input.mousePosition.y;

        //Debug.Log(x.ToString() + " - " + y.ToString());

        //ドラッグ時のマウス位置をシーン上の3D空間の座標に変換する
        Vector3 currentScreenPoint = new Vector3(x, y , screenPoint.z);

        //上記にクリックした場所の差を足すことによって、オブジェクトを移動する座標位置を求める
        Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenPoint) + offset;

        //オブジェクトの位置を変更する
        transform.position = currentPosition;
    }

    void OnMouseUp()
    {
        Draging = false;
        //ドラッグ時のマウス位置を変数に格納
        float x = Input.mousePosition.x;
        float y = Input.mousePosition.y;

        //Debug.Log(x.ToString() + " - " + y.ToString());

        //ドラッグ時のマウス位置をシーン上の3D空間の座標に変換する
        Vector3 currentScreenPoint = new Vector3(x, y , screenPoint.z);

        //上記にクリックした場所の差を足すことによって、オブジェクトを移動する座標位置を求める
        Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenPoint);

        //オブジェクトの位置を変更する
        transform.position = currentPosition;
        LeftAudioSource.Play();
        AnimalManager.UpdateInLoad_AnimalCount();
    }

    // アニマルが柵の中にいるか確かめる
    void CheckInFence()
    {
        Vector3 tmp = transform.position;
        if (!Draging && OnMiss.Value == AnimalTypeEnum.None)
        {
            if (4.6f < tmp.x && tmp.x < 8.9f && tmp.z > 2.1f && 4.9f > tmp.z)
            {//ねこ
                ExistArea = AnimalTypeEnum.Cat;
                _InFence.Value = true;
                if (AnimalType.Value != ExistArea)
                {
                    _OnMiss.Value = AnimalTypeEnum.Cat;
                }
            }
            else if (tmp.x < -4.6f && -8.9f < tmp.x && tmp.z > 2.1f && 4.9f > tmp.z)
            {//いぬ
                ExistArea = AnimalTypeEnum.Dog;
                _InFence.Value = true;
                if (AnimalType.Value != ExistArea)
                {
                    _OnMiss.Value = AnimalTypeEnum.Dog;
                }
            }
            else if (tmp.x > 4.6f && 8.9f > tmp.x && tmp.z < -2.1f && -4.9f < tmp.z)
            {//きつね
                ExistArea = AnimalTypeEnum.Fox;
                _InFence.Value = true;
                if (AnimalType.Value != ExistArea)
                {
                    _OnMiss.Value = AnimalTypeEnum.Fox;
                }
            }
            else if (tmp.x < -4.6f && -8.9f < tmp.x && tmp.z < -2.1f && -4.9f < tmp.z)
            {//たぬき
                ExistArea = AnimalTypeEnum.Racoon;
                _InFence.Value = true;
                if (AnimalType.Value != ExistArea)
                {
                    _OnMiss.Value = AnimalTypeEnum.Racoon;
                }
            }
            else if (-11 < tmp.x && tmp.x < 11 && -7 < tmp.z && tmp.z < 7)
            {
                ExistArea = AnimalTypeEnum.None;
                _InFence.Value = false;
            }
            else
            {
                Debug.Log(gameObject.name+" が想定されるエリア内から出ました, 場所は " + gameObject.transform.position);
                Destroy(gameObject);
            }
        }
    }

    //ミスしたとき
    private void Struggle()
    {
        Instantiate(ObjStruggle, this.transform.position, transform.rotation);
        //AnimalManager.AddScore(gameObject, false);
        MissAudio.Play();
        Debug.Log(gameObject.name + "をInFList<" + AnimalType.Value + ">から削除するように要請");
        AnimalManager.RemoveInFenceList(gameObject, AnimalType.Value);
        //2Sec後削除
        Destroy(gameObject, 2.0f);
    }

    public void Escape()
    {
        //Debug.Log("逃走モードになったので" + gameObject.name);
        _Escaping.Value = true;
        //AnimalManager.RemoveInFenceList(gameObject, AnimalTypeEnum);
    }

    void ChangeRotate()
    {
        if(InFence.Value)
        {
            transform.Rotate(new Vector3(0,180,0));
        }else
        {
            transform.Rotate(new Vector3(0,160,0));
        }
    }

    public void SetVol(float v)
    {
        GrapAudioSource.volume = v;
        LeftAudioSource.volume = v;
        CorrectAudio.volume = v;
        MissAudio.volume = v;
    }

    private Vector3 CenterPos(AnimalTypeEnum animalType)
    {
        var position = Vector3.zero;
        switch (animalType)
        {
            case AnimalTypeEnum.Dog:
                position = new Vector3((-8.9f + -4.6f) / 2, 0, (4.9f + 2.1f) / 2);
                break;
            case AnimalTypeEnum.Cat:
                position = new Vector3((8.9f + 4.6f) /   2, 0, (4.9f + 2.1f) / 2);
                break;
            case AnimalTypeEnum.Fox:
                position = new Vector3((8.9f + 4.6f) /   2, 0, (-4.9f + -2.1f) / 2);
                break;
            case AnimalTypeEnum.Racoon:
                position = new Vector3((-8.9f + -4.6f) / 2, 0, (-4.9f + -2.1f) / 2);
                break;
            default:
                break;
        }
        position.y = transform.position.y;
        return position;
    }
}
