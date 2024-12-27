
# 基于Unity_Sentis的Yolo安全帽识别软件

 本篇README.md面向开发者
 
## 目录

- [简介](#简介)
  - [配置要求](#配置要求)
- [文件目录](#文件目录)
- [目录说明](#目录说明)
- [项目截图](#项目截图)
- [注意事项](#注意事项)
- [版本控制](#版本控制)
- [开发者](#开发者)


### 简介

本项目基于Unity官方最新的 AI 模型本地推理引擎 "Sentis" ，完成对Yolo识别模型的本地推理，并输出推理结果显示识别框。项目内预留接口支持替换自己训练的识别任意物体的onnx模型，包括YoloV8和V5模型。需要注意的是，项目默认使用的是YoloV8模型，使用V5模型需替换推理引擎为Sentis的旧版本：Barracuda引擎。项目支持移植web端、安卓端和iOS端。


 
###### 配置要求

1. Unity 2021.3.15f1 (使用c1版可能导致TextureProvider脚本内存泄漏);
2. Sentis 1.2.0-exp.2 (使用UnityPackageManager安装). 

### 文件目录
eg:

```
Assets 
├── Models
├── OtherAssets
├── Scenes
├── Scripts
  ├── TextureProvider
  └── Main

```

### 目录说明

Models：onnx模型文件夹；<br />
OtherAssets：包括识别框的Shader和盒子预制件等；<br />
Scenes：主场景文件夹（此项目仅有一个场景）；<br />
TextureProvider：为Yolo推理器提供画面的脚本，包括了Web端、安卓端、iOS端画面提供脚本；<br />
Main：Yolo推理核心代码。<br />

### 项目截图

安全帽识别模型训练结果如下图（使用开源安全帽训练集和验证集）
<br />
<br />
<h3 align="center">模型参数</h3>
<p align="center">
<img src="Images/2.png" alt="模型参数" width="750" height="500">
<br />
<h3 align="center">验证集识别结果</h3>
<p align="center">
<img src="Images/3.jpg" alt="验证集识别结果" width="750" height="500">
<br />
<h3 align="center">Unity识别效果</h3>
<p align="center">
<img src="Images/11.jpg" alt="Unity识别效果" width="750" height="500" >
<br />
 
### 注意事项

1.相机画面渲染分辨率为 640*640；<br />
2.使用 NMS算法 以消除冗余框，可通过配置confThreshold和nmsThreshold参数控制抑制效果；<br />
3.textureProviderType参数无需手动设置；<br />
4.sentis的张量维度顺序为NHWC，Barracuda的顺序为NCHW，更改引擎时需要注意；<br />
5.输出结果列表根据置信度降序排序；<br />

### 版本控制

该项目使用Git进行版本管理。您可以在repository参看当前可用版本。

### 开发者

Github : mYoCaRdiA









