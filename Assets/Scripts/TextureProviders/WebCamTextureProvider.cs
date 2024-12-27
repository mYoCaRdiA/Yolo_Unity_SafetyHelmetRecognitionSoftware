using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace Assets.Scripts.TextureProviders
{
    [Serializable]
    public class WebCamTextureProvider : TextureProvider
    {
        [Tooltip("Leave empty for automatic selection.")]
        [SerializeField]
        private string cameraName;
        //网络摄像机图，继承于texture
        public WebCamTexture webCamTexture;

        public WebCamTextureProvider() : base()
        {

        }
        public WebCamTextureProvider(int width, int height, TextureFormat format = TextureFormat.RGB24, string cameraName = null) : base(width, height, format)
        {
            cameraName = cameraName != null ? cameraName : SelectCameraDevice();

            webCamTexture = new WebCamTexture(cameraName);
       
            InputTexture = webCamTexture;
        }

        public WebCamTextureProvider(WebCamTextureProvider provider, int width, int height, TextureFormat format = TextureFormat.RGB24) : this(width, height, format, provider?.cameraName)
        {

        }

        

        public override void Start()
        {
            webCamTexture.Play();
        }

        public override void Stop()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
            }
        }

        public override TextureProviderType.ProviderType TypeEnum()
        {
            return TextureProviderType.ProviderType.WebCam;
        }

        /// <summary>
        ///如果可用，则返回第一个背面摄像头的名称；否则返回第一个可用的摄像头名称。
        /// </summary>
        private string SelectCameraDevice()
        {
            if (WebCamTexture.devices.Length == 0)
                throw new Exception("Any camera isn't avaible!");

            foreach (var cam in WebCamTexture.devices)
            {
                if (!cam.isFrontFacing)
                    return cam.name;
            }
            return WebCamTexture.devices[0].name;
        }

    }
}