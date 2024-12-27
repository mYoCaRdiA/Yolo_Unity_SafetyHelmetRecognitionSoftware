using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using Unity.Sentis.Layers;
using Assets.Scripts.TextureProviders;
using System.ComponentModel;
using UnityEngine.UI;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.Networking;
using System.Net.Http;
using System.IO;

public class Test : MonoBehaviour
{
    public RawImage detect;
    public ModelAsset modelAsset;
    private Model model;
    private IWorker worker;
    Ops ops;
    //������Ⱦ��ai�����������棬640*640
    public RawImage yoloCameraImage;
    public float confThreshold = 0.95f;
    public float nmsThreshold = 0.5f;
    public List<string> group_id_list = new List<string>();
    //ai���������
    public RectTransform yoloCameraImageMask;
    [SerializeField]
    protected TextureProviderType.ProviderType textureProviderType;
    private Texture2D texture;
    [SerializeReference]
    protected TextureProvider textureProvider = null;
    private Dictionary<string, Tensor> m_Inputs;
    List<BoundingBox> BoundingBoxs = new List<BoundingBox>();
    List<ResultBox> boxesss = new List<ResultBox>();
    void Start()
    {
        model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, model);
        ops = WorkerFactory.CreateOps(BackendType.GPUCompute, null);
        textureProvider = GetTextureProvider(model);
        textureProvider.Start();
        //StartCoroutine(a());
        //StartCoroutine(Yolo());
        //detect.transform.SetParent(yoloCameraImage.transform);
    }
    private void Update()
    {
        texture = textureProvider.GetTexture(yoloCameraImageMask);
        Predict(texture);
        yoloCameraImage.texture = texture;

        
        //yoloCameraImage.transform.localScale = Vector3.one;
        //Destroy(texture);
    }
    
    // ������еĿ�

    
    public void Predict(Texture camImage) //������ʹ�õ�������ͷͼ����Ҳ��������ͨͼƬ��
    {

        using Tensor inputImage = TextureConverter.ToTensor(camImage, width: 640, height: 640, channels: 3); //�������ͼ��������
        m_Inputs = new Dictionary<string, Tensor>
        {
            {"images", inputImage }
        };

        worker.Execute(m_Inputs);//ִ������

        var output0 = worker.PeekOutput("output0") as TensorFloat;  //��ȡ������
        output0.MakeReadable(); //��GPU��ȡ�����ݣ�������һ��֮��Ϳ��Զ�ȡoutput0�е�������
        PostProcess(output0, confThreshold, nmsThreshold);
        inputImage.Dispose();
        
        output0.Dispose();
    }
    void PostProcess(TensorFloat outputTensor, float confThreshold, float nmsThreshold)
    {
        int numDetections = outputTensor.shape[2]; // 8400 ������

        Debug.Log(outputTensor.shape);
        for (int i = 0; i < numDetections; i++)
        {
            float confidence = outputTensor[0, 4, i]; // ��5��ֵ�����Ŷ�
            //Debug.Log(confidence);
            //Debug.Log(outputTensor[0, 5, i] + "-----" + outputTensor[0, 6, i] + "-----" + outputTensor[0, 7, i] + "-----" + outputTensor[0, 8, i]);
            if (confidence > confThreshold)
            {
                // ��ȡ�߽������ (x_center, y_center, width, height)
                float xCenter = outputTensor[0, 0, i];
                float yCenter = outputTensor[0, 1, i];
                float width = outputTensor[0, 2, i];
                float height = outputTensor[0, 3, i];

                //// ת��Ϊ (x_min, y_min, x_max, y_max)
                //float xMin = xCenter - width / 2;
                //float yMin = yCenter - height / 2;
                float xMax = xCenter + width / 2;
                float yMax = yCenter + height / 2;

                float xMin = xCenter - width / 2;
                float yMin = yCenter - height / 2;
                xMin = xMin < 0 ? 0 : xMin;
                yMin = yMin < 0 ? 0 : yMin;
                var rect = new Rect(xMin, yMin, width, height);
                rect.xMax = rect.xMax > width ? width : rect.xMax;
                rect.yMax = rect.yMax > height ? height : rect.yMax;
                // ��������
                //BoundingBox box = new BoundingBox(xMin, yMin, xMax, yMax, confidence);

                //Debug.Log(outputTensor[0, 5, i] + "-----" + outputTensor[0, 6, i] + "-----" + outputTensor[0, 7, i] + "-----" + outputTensor[0, 8, i]);

                if (outputTensor[0, 5, i] > 0.15f)
                {
                    ResultBox result = new ResultBox(rect, confidence, true);
                    boxesss.Add(result);
                }
                else
                {
                    ResultBox result = new ResultBox(rect, confidence, false);
                    boxesss.Add(result);
                }

                //BoundingBoxs.Add(box);
            }
        }

        // ִ�� NMS �����������
        List<ResultBox> finalBoxes = NonMaxSuppression(boxesss, nmsThreshold);
        ShowBox(finalBoxes);
        //if (finalBoxes.Count != 0)
        //{
        //    StartCoroutine(SearchDetectAsync(SliceTexture2DToBase64(texture, finalBoxes[0].rect)));

        //}
    }

    // �Ǽ���ֵ����
    List<ResultBox> NonMaxSuppression(List<ResultBox> boxes, float nmsThreshold)
    {
        List<ResultBox> result = new List<ResultBox>();

        // �������ŶȽ�������
        boxes.Sort((a, b) => b.score.CompareTo(a.score));

        while (boxes.Count > 0)
        {
            ResultBox bestBox = boxes[0];
            result.Add(bestBox);
            boxes.RemoveAt(0);

            boxes.RemoveAll(box => IOU(bestBox, box) > nmsThreshold);
        }

        return result;
    }

    // ����������� IOU
    float IOU(ResultBox box1, ResultBox box2)
    {
        float xMin = Math.Max(box1.rect.xMin, box2.rect.xMin);
        float yMin = Math.Max(box1.rect.yMin, box2.rect.yMin);
        float xMax = Math.Min(box1.rect.xMax, box2.rect.xMax);
        float yMax = Math.Min(box1.rect.yMax, box2.rect.yMax);

        float intersection = Math.Max(0, xMax - xMin) * Math.Max(0, yMax - yMin);
        float union = (box1.rect.xMax - box1.rect.xMin) * (box1.rect.yMax - box1.rect.yMin) + (box2.rect.xMax - box2.rect.xMin) * (box2.rect.yMax - box2.rect.yMin) - intersection;

        return intersection / union;
    }

    
    public RectTransform canvasRect; // Canvas �� RectTransform
    public BoxPrafab boxPrefab;          // Ԥ�ƿ� Image
    private List<BoxPrafab> boxessss = new List<BoxPrafab>();
    public void ShowBox(List<ResultBox> box, int width = 1)
    {
        // ������еĿ�
        foreach (var item in boxessss)
        {
            Destroy(item.gameObject);
        }
        boxessss.Clear();

        foreach (var item in box)
        {
            BoxPrafab box1 = Instantiate(boxPrefab, canvasRect);
            box1.transform.SetParent(yoloCameraImage.transform);
            if (item.haveHat)
            {
                box1.Set(true);
            }
            else
            {
                box1.Set(false);
            }
            Vector3 leftUpLocalPos = new Vector2(item.rect.x - yoloCameraImage.rectTransform.sizeDelta.x / 2, yoloCameraImage.rectTransform.sizeDelta.y / 2 - item.rect.y);

            //������Ŀ��ߴ�
            Vector2 targetSize = item.rect.size * yoloCameraImageMask.localScale.x;
            // ������Ŀ��С���������ÿ��С
            box1.image.rectTransform.sizeDelta = targetSize / 2;

            //���ݼ��������ƫ��ԭ�������Ͻ�λ�ã����趨���λ��
            leftUpLocalPos.x += item.rect.size.x / 4;
            box1.transform.localPosition = leftUpLocalPos;
            boxessss.Add(box1);
        }
        
    }


    // �� Canvas �ϻ��ƿ�
    //public void DrawBoxes(List<BoundingBox> boundingBoxes, int imageWidth, int imageHeight)
    //{
    //    // ������еĿ�
    //    foreach (var box in boxes)
    //    {
    //        Destroy(box.gameObject);
    //    }
    //    boxes.Clear();

    //    // �������ű������� BoundingBox ������ת��Ϊ Canvas ������
    //    float xScale = canvasRect.rect.width / imageWidth;
    //    float yScale = canvasRect.rect.height / imageHeight;

    //    foreach (BoundingBox bbox in boundingBoxes)
    //    {
    //        // ʵ����һ�� Image �򣬲�������Ϊ Canvas ��������
    //        Image box = Instantiate(boxPrefab, canvasRect);

    //        // �����Ŀ�Ⱥ͸߶�
    //        float width = (bbox.xMax - bbox.xMin) * xScale;
    //        float height = (bbox.yMax - bbox.yMin) * yScale;

    //        // �������½ǵ����� (ת���� Canvas ����ϵ�����ǵ� Canvas ���ĵ��� (0, 0))
    //        float xCenterCanvas = (bbox.xMin + (bbox.xMax - bbox.xMin) / 2) * xScale - canvasRect.rect.width / 2;
    //        float yCenterCanvas = (bbox.yMin + (bbox.yMax - bbox.yMin) / 2) * yScale - canvasRect.rect.height / 2;

    //        // ���ÿ��λ�úʹ�С
    //        box.rectTransform.anchoredPosition = new Vector2(-xCenterCanvas, -yCenterCanvas);
    //        box.rectTransform.sizeDelta = new Vector2(width, height);

    //        // ������ӵ��б��Ա��Ժ����ٻ����
    //        boxes.Add(box);
    //    }
    //}
    public class BoundingBox
    {
        public float xMin, yMin, xMax, yMax, confidence;

        public BoundingBox(float xMin, float yMin, float xMax, float yMax, float confidence)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
            this.confidence = confidence;
        }
    }
    public class ResultBox
    {
        public readonly Rect rect;
        public float score;
        public bool haveHat;
        public ResultBox(Rect rect, float score, bool haveHat)
        {
            this.rect = rect;
            this.score = score;
            this.haveHat = haveHat;
        }
    }
    protected TextureProvider GetTextureProvider(Model model)
    {
        var firstInput = model.inputs[0];
        int height = firstInput.shape[2].value;
        int width = firstInput.shape[3].value;
        //yoloCameraImage.rectTransform.sizeDelta = new Vector2(width, height);
        TextureProvider provider;
        switch (textureProviderType)
        {
            case TextureProviderType.ProviderType.WebCam:
                provider = new WebCamTextureProvider(textureProvider as WebCamTextureProvider, width, height);
                break;

            case TextureProviderType.ProviderType.Video:
                provider = new VideoTextureProvider(textureProvider as VideoTextureProvider, width, height);
                break;


            default:
                throw new InvalidEnumArgumentException();
        }
        return provider;
    }
    public Texture2D SliceTest(Texture2D originalTexture, Rect rect)
    {
        Debug.Log(rect);
        // ����Rect��y���꣬��ȷ����ȡ��λ����ȷ
        // Unity��Texture2D����ԭ�������½ǣ����ںܶ�����£���UI������ϰ���������Ͻ�Ϊԭ��
        rect.y = originalTexture.height - rect.y - rect.height;

        // ȷ��rect���ᳬ��ԭʼ����ı߽�
        rect.x = Mathf.Clamp(rect.x, 0, originalTexture.width);
        rect.y = Mathf.Clamp(rect.y, 0, originalTexture.height);
        rect.width = Mathf.Clamp(rect.width, 0, originalTexture.width - rect.x);
        rect.height = Mathf.Clamp(rect.height, 0, originalTexture.height - rect.y);

        // �����µ�Texture2D���ߴ����ȡ������ƥ��
        Texture2D newTexture = new Texture2D((int)rect.width, (int)rect.height, originalTexture.format, false);

        // ��ȡָ�����������
        Color[] pixels = originalTexture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

        // ����ȡ������Ӧ�õ���Texture2D��
        newTexture.SetPixels(pixels);
        newTexture.Apply();
        return newTexture;




    }
    public string SliceTexture2DToBase64(Texture2D originalTexture, Rect rect)
    {
        
        Debug.Log("------" + originalTexture.height+originalTexture.width);
        Debug.Log(rect);
        rect.width = Math.Abs(rect.width);
        rect.height = Math.Abs(rect.height);
        // ����Rect��y���꣬��ȷ����ȡ��λ����ȷ
        // Unity��Texture2D����ԭ�������½ǣ����ںܶ�����£���UI������ϰ���������Ͻ�Ϊԭ��
        rect.y = originalTexture.height - rect.y - rect.height;

        // ȷ��rect���ᳬ��ԭʼ����ı߽�
        rect.x = Mathf.Clamp(rect.x, 0, originalTexture.width);
        rect.y = Mathf.Clamp(rect.y, 0, originalTexture.height);
        rect.width = Mathf.Clamp(rect.width, 0, originalTexture.width - rect.x);
        rect.height = Mathf.Clamp(rect.height, 0, originalTexture.height - rect.y);

        Debug.Log(rect);
        // �����µ�Texture2D���ߴ����ȡ������ƥ��
        Texture2D newTexture = new Texture2D((int)rect.width, (int)rect.height, originalTexture.format, false);

        // ��ȡָ�����������
        Color[] pixels = originalTexture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

        // ����ȡ������Ӧ�õ���Texture2D��
        newTexture.SetPixels(pixels);
        newTexture.Apply();
        if (newTexture != null)
        {
            
            Debug.Log(newTexture.isReadable);
            // ����Texture2Dת��Ϊ�ֽ�����������PNG��ʽΪ��
            byte[] bytes = newTexture.EncodeToPNG();
            File.WriteAllBytes("C:\\Users\\Administrator\\Desktop\\"+ rect.x+".png", bytes);
            // ������ʱ������Texture2D����
            Destroy(newTexture);

            // ���ֽ���ת��ΪBase64�ַ���
            string base64String = Convert.ToBase64String(bytes);
            // ����Base64�ַ���
            return base64String;
        }
        else
        {
            Debug.LogError("return null!");
            return null;
        }




    }
    public static class AccessToken
    {
        //public static string apiKey = "ek4stEhxjdaMkou979ejpDPh";              //��д�Լ���apiKey(��ĳ��Լ���)
        //public static string secretKey = "hcGrtORkKk4UP14HcPAxyMDG0X8Ic1Ca";         //��д�Լ���secretKey

        public static string apiKey = "";              //��д�Լ���apiKey(��ĳ��Լ���)
        public static string secretKey = "";         //��д�Լ���secretKey

        public static String getAccessToken()
        {
            String authHost = "";
            HttpClient client = new HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", apiKey));
            paraList.Add(new KeyValuePair<string, string>("client_secret", secretKey));

            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            String result = response.Content.ReadAsStringAsync().Result;
            // Debug.Log(result);
            string[] tokens = result.Split(new string[] { "\"access_token\":\"", "\",\"scope" }, StringSplitOptions.RemoveEmptyEntries);
            result = tokens[1];
            return result;
        }
    }

    //public IEnumerator SearchDetectAsync(string Base64Image, Action<FaceSearchInfo> callback = null)
    //{

    //    string host = "" + AccessToken.getAccessToken();
    //    Encoding encoding = Encoding.Default;

    //    // ������������
    //    string groupIdListString = "";
    //    if (group_id_list.Count != 0 && group_id_list.Count <= 10)
    //    {
    //        for (int i = 0; i < group_id_list.Count; i++)
    //        {
    //            groupIdListString += group_id_list[i];
    //            if (i != group_id_list.Count - 1)
    //            {
    //                groupIdListString += ",";
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("����ʧ��\n��������������!\n����������");
    //        callback?.Invoke(null);
    //        yield break;
    //    }

    //    string requestData = "{\"image\":\"" + Base64Image + "\",\"image_type\":\"BASE64\",\"group_id_list\":\"" + groupIdListString + "\",\"quality_control\":\"LOW\",\"liveness_control\":\"NORMAL\"}";
    //    byte[] requestDataBytes = encoding.GetBytes(requestData);

    //    // �����������
    //    using (UnityWebRequest request = new UnityWebRequest(host, "POST"))
    //    {
    //        request.uploadHandler = new UploadHandlerRaw(requestDataBytes);
    //        request.downloadHandler = new DownloadHandlerBuffer();
    //        request.SetRequestHeader("Content-Type", "application/json");

    //        // �������󲢵ȴ���Ӧ
    //        yield return request.SendWebRequest();

    //        // ������Ӧ����
    //        if (request.result == UnityWebRequest.Result.Success)
    //        {
    //            Debug.Log("��������������Ϣ:" + request.downloadHandler.text);

    //            FaceSearchInfo faceSearchInfo = JsonConvert.DeserializeObject<FaceSearchInfo>(request.downloadHandler.text);
    //            if (faceSearchInfo.error_code == 0)
    //            {
    //                StopAllCoroutines();
    //                Debug.Log("-------------------------------");
    //            }
    //            callback?.Invoke(faceSearchInfo);
    //        }
    //        else
    //        {
    //            Debug.LogError("������������ʧ��: " + request.error);
    //            callback?.Invoke(null);
    //        }
    //    } // ʹ��using���ȷ����Դ���ͷ�


    //}
    public class FaceSearchInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public long error_code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string error_msg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long log_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long timestamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long cached { get; set; }
        /// <summary>
        /// 
        /// </summary>

    }
}
