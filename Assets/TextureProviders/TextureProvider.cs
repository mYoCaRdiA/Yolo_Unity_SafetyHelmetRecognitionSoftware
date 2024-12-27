using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Video;

namespace Assets.Scripts.TextureProviders
{
    [Serializable]
    public abstract class TextureProvider
    {
        //输出的画面
        protected Texture2D ResultTexture;

        //输入进来的画面
        protected Texture InputTexture;

        public TextureProvider(int width, int height, TextureFormat format = TextureFormat.RGB24)
        {
            ResultTexture = new Texture2D(width, height, format, mipChain: false);
        }

        protected TextureProvider()
        {
        }

        ~TextureProvider()
        {
            Stop();
        }

        public abstract void Start();

        public abstract void Stop();


        
        public virtual Texture2D GetTexture(RectTransform yoloMask=null)
        {
            //将输入的画面转成texture2d，返回结果（此处要保证画面分辨率一致）
            

            Texture targetTexture= TextureTools.ForceSquareAndFill(InputTexture, yoloMask);
            
            return TextureTools.ResizeAndCropToCenter(targetTexture, ref ResultTexture, ResultTexture.width, ResultTexture.height);

        }

        public virtual Texture GetOringnalTexture()
        {

            return InputTexture;
        }
       
        public abstract TextureProviderType.ProviderType TypeEnum();
    }


    public static class TextureProviderType
    {
        static TextureProvider[] providers;

        static TextureProviderType()
        {
            
            providers = new TextureProvider[]{
               Activator.CreateInstance<WebCamTextureProvider>(),
               Activator.CreateInstance<VideoTextureProvider>()};          
        }

        public enum ProviderType
        {
            WebCam,
            Video,
            ArCamera
        }

        static public Type GetProviderType(ProviderType type)
        {
            foreach(var provider in providers)
            {
                if (provider.TypeEnum() == type)
                    return provider.GetType();
            }
            throw new InvalidEnumArgumentException();
        }
    }
}