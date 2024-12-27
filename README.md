
# 基于Unity_Sentis的Yolo安全帽识别软件

 本篇README.md面向开发者
 
## 目录

- [简介](#简介)
  - [配置要求](#配置要求)
- [文件目录](#文件目录)
- [文件目录说明](#文件目录说明)
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
├── Model
├── OtherAssets
├── Scenes
├── Scripts
  ├── TextureProvider
  └── Main

```


### 目录说明


### 注意事项

NMS 以消除冗余框






### 版本控制

该项目使用Git进行版本管理。您可以在repository参看当前可用版本。

### 开发者

xxx@xxxx

知乎:xxxx  &ensp; qq:xxxxxx    








