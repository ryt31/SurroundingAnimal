using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;

public class Ranking : MonoBehaviour
{

    NCMBObject obj = new NCMBObject("HighScore");

    NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("HighScore");

    GameObject score = null;

    void Start()
    {
       
    }

    void Update()
    {
   
        //保存
        obj["Name"] = name;
        obj["Score"] = 0;

        obj.SaveAsync();

        obj.SaveAsync((NCMBException e) => {
            if (e != null)
            {
                //エラー処理
            }
            else
            {
                //成功時の処理

            }
        });

        //取得
        query.OrderByDescending("Score");

        query.FindAsync((List<NCMBObject> objList, NCMBException e) => {

            //検索成功したら
            if (e == null)
            {
                objList[0]["Score"] = score;
                objList[0].SaveAsync();
            }
        });

       // this.GetComponent<Text>()text = obj;
    }
}
