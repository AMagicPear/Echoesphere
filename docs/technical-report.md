# Unity 客户端技术方案

**版本**：v1.0
**日期**：2026-04-23

---

## 1. 系统定位

Unity 客户端是执行层的核心模块之一，通过 TCP 与决策层（EchoAgent）双向通信：
- **接收**：EchoAgent 下发的 `command` 类型指令，驱动游戏逻辑
- **上报**：将内部事件以 `text` 消息上报至 EchoAgent，形成感知闭环

---

## 2. 网络通信

### 2.1 帧格式

与全系统统一的**长度前缀 JSON 协议**：
```
[4 bytes big-endian int32] payload_length
[N bytes UTF-8 JSON]       payload
```

### 2.2 消息结构

```csharp
[Serializable]
public class JsonMessage {
    public string type;        // "text" | "image" | "command" | "register"
    public string data;         // 文本内容或 base64 数据
    public string client_type;  // 发送者身份: "unity"
    public string request_id;   // 请求标识 UUID
}
```

### 2.3 消息流向

| 方向 | `type` | `data` 说明 |
|------|--------|-------------|
| EchoAgent → Unity | `"command"` | 控制指令字符串 |
| Unity → EchoAgent | `"text"` | 内部事件描述文本 |
| Unity → EchoAgent | `"image"` | base64 截图（响应 `request_screenshot` 命令） |

---

## 3. AgentCommunicator

**文件**：`Assets/Scripts/Agent/AgentCommunicator.cs`

单例 MonoBehaviour，负责所有 TCP 通信，跨场景持久化（`DontDestroyOnLoad`）。

### 3.1 核心事件

```csharp
public event Action<string>      OnMessageReceived;   // text 消息
public event Action<string>      OnImageReceived;    // base64 图像
public event Action<JsonMessage> OnCommandReceived;  // command 指令
public event Action<bool>        OnConnectionStatusChanged;
```

### 3.2 接收循环

`ReceiveLoopAsync()` 在独立 Task 中运行，精确实现长度前缀协议：
1. 读取 4 字节 big-endian int32 → `payload_length`
2. 循环读取 exactly `payload_length` 字节
3. UTF-8 解码 → `JsonUtility.FromJson<JsonMessage>()`
4. 根据 `msg.type` 分发至对应事件回调

所有回调通过 `SynchronizationContext.Post()` 投递回主线程。

### 3.3 发送接口

| 方法 | `type` | 用途 |
|------|--------|------|
| `SendText(message)` | `"text"` | 上报内部事件 |
| `SendImage(base64, requestId)` | `"image"` | 响应截图请求 |
| `SendCommand(command, requestId)` | `"command"` | 主动发送命令 |

发送端使用 `SemaphoreSlim(1,1)` 锁防止 TCP 并发粘包。

---

## 4. EchoEventCenter（事件调度中心）

**文件**：`Assets/Scripts/Agent/EchoGameEvents.cs`

单例 MonoBehaviour，充当**中介者**，承担双向调度：

```
网络命令 ──▶ AgentCommunicator.OnCommandReceived
                    │
                    ▼
              EchoEventCenter（命令解析 + 事件分发）
                    │
                    ▼
              OnGameplayEvent(GameInternalEvent)
                    │
        ┌───────────┴───────────┐
        ▼                       ▼
   游戏物体响应            AgentCommunicator（反向上报）
                                    │
                                    ▼
                              SendText() → EchoAgent
```

同时订阅键盘输入，与网络命令统一处理，统一对外分发 `GameInternalEvent`。

### 4.1 事件枚举

```csharp
public enum GameInternalEvent {
    None,
    NoteWaterDrop,   // 水滴音符
    NoteCrossing,    // 道路交错音符
    NoteTide,        // 海浪音符
    NoteBreeze,      // 微风音符
}
```

### 4.2 使用方式

游戏物体订阅 `EchoEventCenter.Instance.OnGameplayEvent`，根据事件类型执行业务逻辑。

> `GameInternalEvent` 与 JSON `command` **非一一对应**，内部可包含业务逻辑运算。

---

## 5. 关键设计决策

### 5.1 单例 + DontDestroyOnLoad

`AgentCommunicator` 与 `EchoEventCenter` 均实现 `static Instance { get; private set; }`，挂载于 `DontDestroyOnLoad` 的 GameObject 上，保证 TCP 连接与事件总线在场景切换时不中断。

### 5.2 SynchronizationContext 主线程回调

所有网络回调通过 `SynchronizationContext.Current.Post()` 投递回主线程，确保 Unity API 调用安全。

### 5.3 协程响应截图

`ScreenCapture.CaptureScreenshotAsTexture()` 必须在 `WaitForEndOfFrame` 后调用。协程可自然实现帧同步，无需复杂状态机。

---

## 6. 文件结构

```
Assets/Scripts/Agent/
├── AgentCommunicator.cs    # TCP 通信中枢（单例）
├── AudioRecorder.cs        # 音频录制
└── EchoGameEvents.cs       # 游戏事件调度中心（单例，中介者）
```
