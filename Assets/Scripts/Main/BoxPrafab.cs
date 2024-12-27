using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxPrafab : MonoBehaviour
{
    public Text text;
    public Image image;
    public Image image1;
    public Image image2;
    public Image image3;
    public Image image4;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DestroySeif()
    {
         Destroy(this.gameObject);
    }
    public void Set(bool haveHat)
    {
        if (haveHat)
        {
            text.text = "已佩戴安全帽";
            text.color = Color.green;
            image1.color = Color.green;
            image2.color = Color.green;
            image3.color = Color.green;
            image4.color = Color.green;
        }
        else
        {
            text.text = "未佩戴安全帽";
            text.color = Color.red;

            image1.color = Color.red;
            image2.color = Color.red;
            image3.color = Color.red;
            image4.color = Color.red;

        }


    }
}
