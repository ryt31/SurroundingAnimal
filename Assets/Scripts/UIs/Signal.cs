using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Signal : MonoBehaviour
{

    //public
    public float speed = 1.0f;

    //private
    private Text text;
    private Image image;
    private float time;

    //public GameObject gameobject;
    private AnimalManager animalManager;

    private enum ObjType
    {
        TEXT,
        IMAGE
    };
    private ObjType thisObjType = ObjType.TEXT;

    void Start()
    {
        animalManager = GameObject.FindWithTag("Manager").GetComponent<AnimalManager>();
        //アタッチしてるオブジェクトを判別
        if (this.gameObject.GetComponent<Image>())
        {
            thisObjType = ObjType.IMAGE;
            image = this.gameObject.GetComponent<Image>();
        }
        else if (this.gameObject.GetComponent<Text>())
        {
            thisObjType = ObjType.TEXT;
            text = this.gameObject.GetComponent<Text>();
        }

    }

    void Update()
    {
        int animalNumber = animalManager.InLoad_AnimalCount.Value;
        if (animalNumber >= 10)
        {
            image.color = GetAlphaColor(image.color);
        }
        else
            image.color = FalseAlphaColor(image.color);

    }

    //Alpha値を更新してColorを返す
    Color GetAlphaColor(Color color)
    {
        time += Time.deltaTime * 5.0f * speed;
        color.a = Mathf.Sin(time) * 0.5f + 0.4f;

        return color;
    }

    Color FalseAlphaColor(Color color)
    {
        color.a = 0;

        return color;
    }
}