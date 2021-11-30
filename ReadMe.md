URP实现水滴融合效果(Metaball)
==============

Metaball是一种水滴融合效果又称融球，之前看到过在Uniy默认渲染管线下的实现方案。原理很简单：

- 先用一个单镜头渲染到一张RenderTextrue上
- 对RenderTexture进行一次blur
- 将RenderTexture渲染到一张四边面上
  
今天在URP下试了一下:
![](https://github.com/dafong/URP_2D_Metaball/blob/main/Recordings/gif_animation_005.gif?raw=true)

做法就是

- 建立两个Render，一个Forward Render用于正常相机的渲染，一个Metaball Render用于渲染RenderTexture并做模糊处理
![](https://github.com/dafong/URP_2D_Metaball/blob/main/Recordings/1.jpg?raw=true)
![](https://github.com/dafong/URP_2D_Metaball/blob/main/Recordings/2.jpg?raw=true)
- 然后将主摄像机设置为ForwardRender，将拍摄融球的摄像机设置为Metaball Render，并将Background设置为Solid Color同时清空Alpha通道。
- 为四边面画一个拖拽一个简单的Shader Graph，用_CutOff控制融球的大小，借助Step函数用StokeAlpha来控制哪部分是边缘(跟2D描边类似)。
![](https://github.com/dafong/URP_2D_Metaball/blob/main/Recordings/3.jpg?raw=true)

项目地址:[https://github.com/dafong/URP_2D_Metaball/](https://github.com/dafong/URP_2D_Metaball/)