using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UIElements;

class TextureTools
{
    // 左右反转纹理（GPU加速）
    public static Texture2D FlipTexture(Texture2D originalTexture)
    {
        RenderTexture rt = RenderTexture.GetTemporary(originalTexture.width, originalTexture.height);

        // 根据设备方向调整Blit函数的参数
        Vector2 scale = new Vector2(1, -1);

        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            // 如果是横向右旋转，则需要左右反转
            scale = new Vector2(-1, 1);
        }

        // 执行Blit操作
        Graphics.Blit(originalTexture, rt, scale, Vector2.zero);

        //// 创建新的Texture2D对象来保存结果

        //flippedTexture = null;  
        Resources.UnloadUnusedAssets();
        flippedTexture = new Texture2D(originalTexture.width, originalTexture.height);


        RenderTexture.active = rt;
        flippedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        flippedTexture.Apply();

        // 清理临时资源
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return flippedTexture;

        //return originalTexture;
    }

    public static Texture2D flippedTexture;
    public static Texture2D resultTexture;

    public static Texture ForceSquareAndFill(Texture texture, RectTransform yoloMask = null)
    {
        // 获取原始 Texture 的宽度和高度
        int width = texture.width;
        int height = texture.height;

        // 确定目标尺寸为较大的一边，使其成为正方形
        int targetSize = Mathf.Max(width, height);

        // 创建一个新的 Texture2D，大小为目标尺寸
        resultTexture = new Texture2D(targetSize, targetSize);

        // 将原始 Texture 的内容读取到新的 Texture2D 中
        RenderTexture temporary = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(texture, temporary);
        RenderTexture.active = temporary;
        resultTexture.ReadPixels(new Rect(0, 0, width, height), (targetSize - width) / 2, (targetSize - height) / 2);
        resultTexture.Apply();
        RenderTexture.active = null;

        // 释放临时资源
        RenderTexture.ReleaseTemporary(temporary);

        if (yoloMask != null)
        {
            // 更新 yoloMask 的尺寸和缩放
            UpdateYoloMask(yoloMask, width, height, targetSize);
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            resultTexture = FlipTexture(resultTexture);
        }

        return resultTexture;
    }

    private static void UpdateYoloMask(RectTransform yoloMask, int width, int height, int targetSize)
    {
        float scale = Screen.width / 640f;

        yoloMask.transform.localScale = new Vector3(scale, scale, 0);

        //Debug.Log(yoloMask.transform.localScale);

        //Debug.Log("sizeOr"+yoloMask.transform.localScale);
        //if (width > height)
        //{
        //    float realHeight = targetSize / (float)width * height;
        //    yoloMask.sizeDelta = new Vector2(targetSize, realHeight);
        //    float scale = Screen.height / realHeight;
        //    yoloMask.transform.localScale = new Vector3(scale, scale, 0);
        //    Debug.Log(yoloMask.transform.localScale);
        //}
        //else
        //{
        //    float realWidth = targetSize / (float)height * width;
        //    yoloMask.sizeDelta = new Vector2(realWidth, targetSize);
        //    float scale = Screen.width / realWidth;
        //    yoloMask.transform.localScale = new Vector3(scale, scale, 0);
        //    Debug.Log(yoloMask.transform.localScale);
        //}
    }




    public static Texture2D ResizeAndCropToCenter(Texture texture, ref Texture2D result, int width, int height)
    {
        float widthRatio = width / (float)texture.width;
        float heightRatio = height / (float)texture.height;
        float ratio = widthRatio > heightRatio ? widthRatio : heightRatio;

        Vector2Int renderTexturetSize = new Vector2Int((int)(texture.width * ratio), (int)(texture.height * ratio));
        RenderTexture renderTexture = RenderTexture.GetTemporary(renderTexturetSize.x, renderTexturetSize.y);
        Graphics.Blit(texture, renderTexture);

        RenderTexture previousRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;

        int xOffset = (renderTexturetSize.x - width) / 2;
        int yOffset = (renderTexturetSize.y - width) / 2;
        result.ReadPixels(new Rect(xOffset, yOffset, width, height), destX: 0, destY: 0);
        result.Apply();

        RenderTexture.active = previousRenderTexture;
        RenderTexture.ReleaseTemporary(renderTexture);
        return result;
    }

    /// <summary>
    /// Draw rectange outline on texture
    /// </summary>
    /// <param name="width">Width of outline</param>
    /// <param name="rectIsNormalized">Are rect values normalized?</param>
    /// <param name="revertY">Pass true if y axis has opposite direction than texture axis</param>
    public static void DrawRectOutline(Texture2D texture, Rect rect, Color color, int width = 1, bool rectIsNormalized = true, bool revertY = false)
    {
        if (rectIsNormalized)
        {
            rect.x *= texture.width;
            rect.y *= texture.height;
            rect.width *= texture.width;
            rect.height *= texture.height;
        }

        if (revertY)
            rect.y = rect.y * -1 + texture.height - rect.height;

        if (rect.width <= 0 || rect.height <= 0)
            return;

        DrawRect(texture, rect.x, rect.y, rect.width + width, width, color);
        DrawRect(texture, rect.x, rect.y + rect.height, rect.width + width, width, color);

        DrawRect(texture, rect.x, rect.y, width, rect.height + width, color);
        DrawRect(texture, rect.x + rect.width, rect.y, width, rect.height + width, color);
        texture.Apply();
    }

    static private void DrawRect(Texture2D texture, float x, float y, float width, float height, Color color)
    {
        if (x > texture.width || y > texture.height)
            return;

        if (x < 0)
        {
            width += x;
            x = 0;
        }
        if (y < 0)
        {
            height += y;
            y = 0;
        }

        width = x + width > texture.width ? texture.width - x : width;
        height = y + height > texture.height ? texture.height - y : height;

        x = (int)x;
        y = (int)y;
        width = (int)width;
        height = (int)height;

        if (width <= 0 || height <= 0)
            return;

        int pixelsCount = (int)width * (int)height;
        Color32[] colors = new Color32[pixelsCount];
        //Array.Fill(colors, color);
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }


        texture.SetPixels32((int)x, (int)y, (int)width, (int)height, colors);
    }

    //public static void RenderMaskOnTexture(Tensor mask, Texture2D texture, Color color, float maskFactor = 0.25f)
    //{
    //    Ops ops = BarracudaUtils.CreateOps(WorkerFactory.Type.ComputePrecompiled);
    //    Tensor imgTensor = new Tensor(texture);
    //    Tensor factorTensor = new Tensor(1, 3, new[] { color.r * maskFactor, color.g * maskFactor, color.b * maskFactor });
    //    Tensor colorMask = ops.Mul(new[] { mask, factorTensor });
    //    Tensor imgWithMasks = ops.Add(new[] { imgTensor, colorMask });

    //    RenderTensorToTexture(imgWithMasks, texture);

    //    factorTensor.tensorOnDevice.Dispose();
    //    imgTensor.tensorOnDevice.Dispose();
    //    colorMask.tensorOnDevice.Dispose();
    //    imgWithMasks.tensorOnDevice.Dispose();
    //}

    //private static void RenderTensorToTexture(Tensor tensor, Texture2D texture)
    //{
    //    RenderTexture renderTexture = tensor.ToRenderTexture();
    //    RenderTexture.active = renderTexture;
    //    texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    //    texture.Apply();
    //    RenderTexture.active = null;
    //    renderTexture.Release();
    //}
}

