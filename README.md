讨论QQ群: 87481002

 **简介** 
------------
YouYou Framework 是一个开源的客户端（基于Unity3d）服务端双端的游戏框架.<br>
使用C# .NET Core开发的分布式游戏服务端, Redis做共享缓存, MongoDB做数据存储, 双端支持热更新.<br>
在最新的 YouYou Framework 版本中, 包含以下内置模块. 

 **YouYouEditor(Odin可视化编辑界面)**
------------------------------------
{
>宏配置(Macro) - 选择资源加载模式,如AssetBundle加载,AssetDatabase加载,Resource加载

>全局配置(Config) - 支持设备等级参数（高 中 低 配机）,如Http失败重试次数，不同等级设备匹配相应参数值

>调试器 (Debugger) - 选择开启<br>
1.可在打包APK后,出现调试器窗口，便于查看运行时日志、调试信息等<br>
2.每次启动后会生成日志记录文本,可设置日志缓存数量,单文件最大存储数量

>资源打包配置(AssetBundle) - 支持配置 资源包版本号, 资源包加密:<br>
1.可以设置把文件夹打成一个整体资源包，还是文件夹内容打散打包,如Excel,Lua脚本,图集等可以打成整包，角色、场景、特效等可以打成散包<br>
2.打包到【对应版本号/平台】文件夹内,自动生成"依赖关系文件"，加载时可自动读取

>对象池分析(Pool Analyze) - 方便查看项目里各个池中的资源的资源计数和剩余释放时间:<br>
1.类对象池<br>
2.AssetBundle池<br>
3.Asset池

}

 **GameEntry(游戏框架入口)** 
------------------------------------
{
>数据 (Data) - 将任意类型的数据进行全局保存，用于管理游戏运行时的各种数据。

>数据表 (Data Table) - 将游戏数据以表格（如 Microsoft Excel）的形式进行配置后，可以使用此模块读取数据表。数据表的格式是支持自定义的。

>下载 (Download) - 提供下载文件的功能，支持断点续传，并可指定允许几个下载器进行同时下载。更新资源时会主动调用此模块。

>事件 (Event) - 游戏逻辑监听、抛出事件的机制。YouYouFramework 中的很多模块在完成操作后会抛出内置事件,用户也可以定义自己的游戏逻辑事件。

>有限状态机 (FSM) - 提供创建、使用和销毁有限状态机的功能。

>本地化 (Localization) - 提供本地化功能，也就是我们平时所说的多语言。支持UIText的本地化，UIImage的本地化

>网络 (Network) - 提供使用 Socket 长连接的功能，支持 TCP 协议。用户可以同时建立多个连接与多个服务器同时进行通信，比如除了连接常规的游戏服务器，还可以连接语音聊天服务器。已接入 ProtoBuf 协议库。

>对象池 (Object Pool) - 提供对象缓存池的功能，避免频繁地创建和销毁各种游戏对象，提高游戏性能。除了 YouYouFramework 自身使用了对象池，用户还可以很方便地创建和管理自己的对象池。目前支持类对象池,变量池,GameObject对象池,AssetBundle池,Asset池等(如Prefab).

>流程 (Procedure) - 是贯穿游戏运行时整个生命周期的有限状态机。通过流程，将不同的游戏状态进行解耦将是一个非常好的习惯。对于网络游戏，你可能需要如检查资源流程、更新资源流程、检查服务器列表流程、选择服务器流程、登录服务器流程、创建角色流程等流程，而对于单机游戏，你可能需要在游戏选择菜单流程和游戏实际玩法流程之间做切换。如果想增加流程，只要派生自 ProcedureBase 类并实现自己的流程类即可使用。

>资源 (Resource) - 使用了一套完整的异步加载资源体系。不论简单的数据表、本地化字典，还是复杂的实体、场景、界面，都可使用异步加载。同时，YouYouFramework 提供了默认的内存管理策略（当然，你也可以定义自己的内存管理策略, 甚至切换Resource加载模式）。

>场景 (Scene) - 提供场景管理的功能，可以同时加载多个场景，也可以随时卸载任何一个场景，从而很容易地实现场景的分部加载。

>声音 (Sound) - 提供管理声音和声音组的功能，使用FMOD声音引擎, 根据传入参数,支持多个音轨之间的过度转换,以及音量调节和3D声音等。

>界面 (UI) - 提供管理界面和界面组的功能，如 激活界面、改变界面层级等。界面使用结束后可以不立刻销毁(UI池)，从而等待下一次重新使用。

>Web 请求 (Web Request) - 提供使用短连接的功能，可以用 Get 或者 Post 方法向服务器发送请求并获取响应数据，可指定允许几个 Web 请求器进行同时请求。

>WebSocket - 提供使用 WebSocket 长连接的功能, 支持 TCP 协议。目前以Json进行监听通讯

>SDK接入 - 提供微信SDK,支付宝SDK,苹果SDK,谷歌SDK等渠道的接入, 在服务端Config配置相关的签名以及密钥后可使用

>xLua - 提供了基于xLua的框架,绕过了LuaAPI实现CSharp和Lua的共享内存,可调用YouYouFramework的任意模块, 基于xLua的代码热更新.

}


 **YouYouServer** 
------------------------------------
{

>**YouYouServer.Core-核心基类库**<br>
引用了CSRedisCore, MongoDB.driver, Google.Protobuf等第三方包,用来存放项目的基类,工具类,核心类等

>**YouYouServer.Common-公共数据类库**<br>
引用了YouYouServer.Core库, 用来读取项目的配置文件, 常量, Excel数据, Protobuf数据, DB数据等

>**YouYouServer.Model-线程模型库**<br>
引用了YouYouServer.Core, YouYouServer.Common库, 用来处理分布式服务端和客户端的交互接口, 连接请求, 派发了数据消息事件等

>**YouYouServer.Hofix-服务器热补丁库**<br>
引用了YouYouServer.Model库, 但是不能被任何库直接引用, 而是通过反射加载程序集的方式被调用, 监听了数据消息事件, 用来处理数据读写以及真正的业务逻辑, 如PVP战斗, 商城, 背包等

>**YouYouServer.WebAccount-Web服务器集群**

>**YouYouServer.GatewayServer-网关服务器,多节点控制台**

>**YouYouServer.GameServer-游戏服务器,多节点控制台**

>**YouYouServer.WorldServer-中心服务器,单节点控制台**

}