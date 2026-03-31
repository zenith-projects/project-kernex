# Networking Reference

Complete reference for Godot 4.6 networking: multiplayer, HTTP, WebSocket. All C#, no GDScript.

---

## Architecture: Three Tiers

| Tier | API | Use Case |
|------|-----|----------|
| Low-level | Raw TCP/UDP/HTTP/SSL | Maximum control, maximum effort |
| Mid-level | `MultiplayerPeer` (ENet, WebRTC, WebSocket) | Serialization, peer management, transfer modes |
| High-level | SceneTree-integrated RPCs, authority, spawning | Game multiplayer |

**Protocol trade-offs:**
- **TCP**: Reliable, ordered, slow. Good for HTTP, bad for games.
- **UDP**: Unreliable, fast, low MTU. ENet rebuilds selective reliability on top.

---

## High-Level Multiplayer

### Creating Server/Client

```csharp
// Server
var peer = new ENetMultiplayerPeer();
peer.CreateServer(7000, 20); // port, max_clients
Multiplayer.MultiplayerPeer = peer;

// Client
var peer = new ENetMultiplayerPeer();
peer.CreateClient("127.0.0.1", 7000);
Multiplayer.MultiplayerPeer = peer;

// Disconnect
Multiplayer.MultiplayerPeer = null;
```

### Connection Signals

```csharp
public override void _Ready()
{
    Multiplayer.PeerConnected += OnPeerConnected;       // All peers
    Multiplayer.PeerDisconnected += OnPeerDisconnected; // All peers
    Multiplayer.ConnectedToServer += OnConnectedOk;     // Client only
    Multiplayer.ConnectionFailed += OnConnectionFail;   // Client only
    Multiplayer.ServerDisconnected += OnServerDown;     // Client only
}

private void OnPeerConnected(long id) { }
private void OnPeerDisconnected(long id) { }
private void OnConnectedOk() { }
private void OnConnectionFail() { }
private void OnServerDown() { }
```

### Peer IDs

- Server is always `1`
- Clients get random positive integer
- `Multiplayer.GetUniqueId()` — this peer's ID
- `Multiplayer.IsServer()` — true if server

### RPCs (Remote Procedure Calls)

```csharp
[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false,
     TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
private void MyRpcMethod(string data)
{
    GD.Print($"Received: {data} from {Multiplayer.GetRemoteSenderId()}");
}

// Call on all peers
Rpc(MethodName.MyRpcMethod, "hello");

// Call on specific peer
RpcId(1, MethodName.MyRpcMethod, "hello server");
```

**RPC Parameters:**

| Parameter | Values | Description |
|-----------|--------|-------------|
| `mode` | `Authority` (default) | Only authority can call remotely |
| | `AnyPeer` | Any client can call (for input transfer) |
| `CallLocal` | `false` (default) | Not called locally |
| | `true` | Also called on caller (for server-as-player) |
| `TransferMode` | `Reliable` | Resend until ack, ordered |
| | `Unreliable` | No ack, can be lost |
| | `UnreliableOrdered` | Ordered, drops late packets |

### RPC Gotchas (CRITICAL)

- Sending/receiving nodes **must have identical NodePath** in the tree
- Use `AddChild(node, forceReadableName: true)` for RPC nodes
- ALL RPCs in a script must exist on BOTH client and server
- RPC signatures are checksummed — mismatch causes cryptic errors
- Function **arguments are NOT checked** — only name, return type, [Rpc] declaration

### Input Transfer Pattern

```csharp
private void OnAttackInput()
{
    RpcId(1, MethodName.ServerAttack); // Send to server only
}

[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,
     TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
private void ServerAttack()
{
    int senderId = Multiplayer.GetRemoteSenderId();
    // Validate and process attack for senderId
}
```

---

## HTTP Requests

```csharp
public override void _Ready()
{
    var httpRequest = GetNode<HttpRequest>("HTTPRequest");
    httpRequest.RequestCompleted += OnRequestCompleted;
    httpRequest.Request("https://api.example.com/data");
}

private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
{
    if (result != (long)HttpRequest.Result.Success) return;

    var json = Json.ParseString(System.Text.Encoding.UTF8.GetString(body));
    var data = json.AsGodotDictionary();
    GD.Print(data["key"]);
}

// POST request
var jsonData = Json.Stringify(myDictionary);
httpRequest.Request(url, new[] { "Content-Type: application/json" },
    HttpClient.Method.Post, jsonData);
```

**Gotcha**: One `HttpRequest` node per active request. Create/destroy dynamically for parallelism.

---

## WebSocket

```csharp
public partial class WsClient : Node
{
    private WebSocketPeer _socket = new();

    public override void _Ready()
    {
        _socket.ConnectToUrl("wss://echo.websocket.org");
    }

    public override void _Process(double delta)
    {
        _socket.Poll(); // MUST call every frame

        var state = _socket.GetReadyState();
        if (state == WebSocketPeer.State.Open)
        {
            while (_socket.GetAvailablePacketCount() > 0)
            {
                var packet = _socket.GetPacket();
                if (_socket.WasStringPacket())
                    GD.Print(System.Text.Encoding.UTF8.GetString(packet));
            }
        }
        else if (state == WebSocketPeer.State.Closed)
        {
            GD.Print($"Closed: {_socket.GetCloseCode()}");
            SetProcess(false);
        }
    }
}
```

**Key rules**: `Poll()` every frame, check `GetReadyState()`, keep polling during `Closing` state.

---

## Platform Gotchas

| Platform | Issue |
|----------|-------|
| Android | Must enable INTERNET permission in export preset |
| Web/HTML5 | No raw TCP/UDP — use WebSocket or WebRTC |
| Windows | Port forwarding must be UDP for ENet multiplayer |
| Security | Never embed tokens/passwords in shipped binaries |
