using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

//シーンをまたぐ
public class GameManager : MonoBehaviour
{
    private Subject<SceneType> _NextScene = new Subject<SceneType>();
    public IObservable<SceneType> NextScene => _NextScene;

    public SceneType NowScene()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Title":
                return SceneType.Title;
            case "Main":
                return SceneType.Main;
            case "Result":
                return SceneType.Result;
            default:
                return SceneType.Main;
        }
    }

    //マウスカーソル用変数
    [SerializeField] private Texture2D NomalCursorTexture = null, DragCursorTexture = null;
    private CursorMode cursorMode = CursorMode.ForceSoftware;
    private Vector2 hotspot = Vector2.zero;

    //スコア
    public AnimalScore AnimalScore = new AnimalScore();

    private ReactiveProperty<float> _BGMVolume = new ReactiveProperty<float>(60);
    public IReadOnlyReactiveProperty<float> BGMVolume => _BGMVolume;
    private ReactiveProperty<float> _SEVolume = new ReactiveProperty<float>(60);
    public IReadOnlyReactiveProperty<float> SEVolume => _SEVolume;
    private TitleSceneManager TitleSceneManager;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (DragCursorTexture.width != 32)
        {
            TextureScale.Bilinear(DragCursorTexture, 32, 32);
            TextureScale.Bilinear(NomalCursorTexture, 32, 32);
        }
        Cursor.SetCursor(NomalCursorTexture, new Vector2(DragCursorTexture.width / 2, DragCursorTexture.height / 2), cursorMode);
        //カーソルを画面内で動かす
        Cursor.lockState = CursorLockMode.Confined;

        //マウスカーソルの画像変更
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0))
            .Subscribe(_ => OnClicking());
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ => Cursor.SetCursor(NomalCursorTexture, new Vector2(DragCursorTexture.width / 2, DragCursorTexture.height / 2), cursorMode));

        if (NowScene() == SceneType.Title)
        {
            TitleSceneManager = GetComponent<TitleSceneManager>();
        }

        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyDown(KeyCode.Escape))
            .Subscribe(_ => Application.Quit());
    }

    private void OnClicking()
    {
        Cursor.SetCursor(DragCursorTexture, new Vector2(DragCursorTexture.width / 2, DragCursorTexture.height / 2), cursorMode);
    }

    /// <summary> シーン変移を行う </summary>
    public void ChangeScene(SceneType sceneType)
    {
        var sceneName = "";
        switch (sceneType)
        {
            case SceneType.Title:
                sceneName = "Title";
                _NextScene.OnNext(SceneType.Title);
                break;
            case SceneType.Main:
                sceneName = "Main";
                _NextScene.OnNext(SceneType.Main);
                break;
            case SceneType.Result:
                sceneName = "Result";
                _NextScene.OnNext(SceneType.Result);
                break;
            case SceneType.ViewAnimals:
                sceneName = "ViewAnimals";
                _NextScene.OnNext(SceneType.ViewAnimals);
                break;
            default:
                break;
        }
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void SetScore(AnimalScore _AnimalScore)
    {
        AnimalScore = _AnimalScore;
    }

    public void ChangeBGMSlide(float vol) => _BGMVolume.Value = vol;
    public void ChangeSESlide(float vol) => _SEVolume.Value = vol;
}

public class AnimalScore
{
    public int Dog = 0, Cat = 0, Fox = 0, Raccoon = 0;
}