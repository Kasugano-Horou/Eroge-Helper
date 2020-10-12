自娱自乐项目

### How to build

`git clone https://github.com/luojunyuan/Eroge-Helper` 

使用 VS2019 打开 `Eroge-Helper.sln`

Ctrl+Shift+B 生成解决方案

将[这里](https://pan.baidu.com/s/1Pk0cxsv84NtVdBbBlytSxQ)(提取码: y1ge)的文件解压，到项目目录下 `Eroge-Helper\bin\Debug\libs` 下。（需手动创建libs文件夹）

![屏幕截图 2020-09-26 141739.png](https://i.loli.net/2020/09/26/9QDZKaIwBbfu2eM.png)



两种方法启动

a.  选择VS上方工具栏 - 调试 - ErogeHelper属性

在调试页面输入命令行参数

`"C:\Users\ljy77\Downloads\游戏\syugaten\syugaten.exe" /le`

输入完整游戏路径即可，/le选项用于开启le，自行判断是否需要。

![屏幕截图 2020-09-26 142206.png](https://i.loli.net/2020/09/26/eKrl8tziucgqLZE.png)

输入完成，保存，F5 运行项目

### Install

将软件注册到右键选项单

Run in Admin

`.\ServerRegistrationManager.exe install .\EHShellMenuHandler.dll -codebase`
