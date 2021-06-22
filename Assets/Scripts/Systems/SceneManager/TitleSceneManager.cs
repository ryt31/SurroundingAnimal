using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneManager : MonoBehaviour
{
    private GameManager GameManager;
    [SerializeField] private Slider SlideBGM = null, SlideSE = null;
    [SerializeField] private Text TextBGM = null, TextSE = null;
    [SerializeField] private GameObject _GameManager = null;

    [SerializeField] AudioSource BGMAudioSource = null;
    [SerializeField] AudioSource SEAudioSource = null;

    /*[SerializeField] private float _StartXPos = -4;
    [SerializeField] private float _MoveSpeed = 1;
    [SerializeField] GameObject CatObj;*/

    void Start()
    {
        if (GameObject.FindWithTag("GameManager")==null)
        {
            Instantiate(_GameManager);
        }
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        SlideBGM.value = GameManager.BGMVolume.Value;
        SlideSE.value = GameManager.SEVolume.Value;

        //Debug.Log(GameManager.BGMVolume.Value);

        /*this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0) != true)
            .Where(_ => !Input.GetKeyDown(KeyCode.Escape))
            .Where(_ => Input.anyKeyDown)
            .Subscribe(_ =>GameManager.ChangeScene(SceneType.Main));*/

        GameManager.BGMVolume
            .Where(_ => TextSE != null)
            .Subscribe(_ => {
                TextBGM.text = (Mathf.RoundToInt(GameManager.BGMVolume.Value)).ToString();
                //SlideBGM.value = GameManager.BGMVolume.Value / 100;
            });
        GameManager.SEVolume
            .Where(_ => TextSE != null)
            .Subscribe(_ => {
                TextSE.text = (Mathf.RoundToInt(GameManager.SEVolume.Value)).ToString();
                //SlideSE.value = GameManager.SEVolume.Value / 100;
            });

        GameManager.BGMVolume
            .Where(_ => BGMAudioSource != null)
            .Subscribe(_ => BGMAudioSource.volume = GameManager.BGMVolume.Value / 100);
        GameManager.SEVolume
            .Where(_ => SEAudioSource != null)
            .Subscribe(_ => SEAudioSource.volume = GameManager.SEVolume.Value / 100);

        /*this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                float x = _StartXPos + Time.time / 5 * _MoveSpeed;
                CatObj.transform.position = new Vector3(x, -(x * x) / 4 - (x / 10) + 0.4f, 0);
                CatObj.transform.rotation = Quaternion.Euler(new Vector3(5 / 4 * x * x * x + 5 / 4 * x * x + 25 / 2, 90, 0));
            });*/
    }

    public void ChangeBGMVolume() => GameManager.ChangeBGMSlide(SlideBGM.value);
    public void ChangeSEVolume() => GameManager.ChangeSESlide(SlideSE.value);
    public void GotoViewAnimalsScene() => GameManager.ChangeScene(SceneType.ViewAnimals);

    public void PlaySE() { SEAudioSource.Play(); }

    public void GotoMainScene() => GameManager.ChangeScene(SceneType.Main);
}
