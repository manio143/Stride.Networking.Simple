# Stride.Networking.Simple
This is a small lib which I wrote because it came to my mind and bugged me for the whole day.

Basically you want to do a simple connection client-server, but don't care for all that pesky networking stuff.
Well, you're in luck - slap that lib onto your project and do the following:

### Client
First add the `NetworkClientSystem` to your game - if you want to connect to the server straight away I have an extension for you:

```csharp
using (var game = new Game())
{
    // Instantiate a connection to the server
    game.AddNetworkClient(out var networkClient);
    networkClient.Open("localhost", 25565);

    game.Run();
}
```

If you want to add it later (after game systems have been initialized), you need to call `Open` before adding it to the game systems collection.

```csharp
var networkClient = new NetworkClientSystem(game.Services);
networkClient.Open("localhost", 25565);

game.Services.AddService(networkClient);
game.GameSystems.Add(networkClient);
```

Ok, that's it for connections, now let's write some scripts. The general idea is:

* We define a _request_ for a handler using an enum
* Client has a custom script which extends `NetworkScript`
* Server has a custom script which extends `NetworkScript`
* We registered server script with the _request_
* Client runs first and sends the _request_
* They both then use `Send`/`Receive` methods to communicate
* Both scripts will use the Guid `ScriptId` to identify that they are supposed to talk to each other

```csharp
/// Enum used to link <see cref="PingScript"/> and <see cref="PongScript"/>.
/// Put it in a project shared between client and server.
public enum PingRequest
{
    Start
}

public class PingScript : NetworkScript
{
    protected override async Task Execute()
    {
        // This will tell the server to instantiate a new PongScript
        await StartNewServerHandler(PingRequest.Start);

        while (!IsCancelled)
        {
            await Send("Ping");
            var response = await Receive<string>();
            Log.Info("Sent Ping; Received " + response);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
```

### Server
Right now the idea is to just have a standalone server - not sure how it would look if there was a fully headless Stride (which right now is not).

Running the server is pretty straight forward:

```csharp
var services = new ServiceRegistry();
services.AddNetworkServer(out var networkServer)
    .AddNetworkHandler(PingRequest.Start, (srv, conn) => new PongScript(srv, conn));

await networkServer.OpenAsync(25565);
Console.WriteLine("Server started. Press any key to stop...");
Console.ReadKey();
```

Notice how we used the `PingRequest` enum to register the `PongScript` factory. 
And then for the script:

```csharp
// Server script - needs to have the constructor with those two parameters to correctly use it
public class PongScript : NetworkScript
{
    public PongScript(IServiceRegistry serviceRegistry, NetworkConnection connection)
        : base(serviceRegistry, connection)
    {
    }

    protected override async Task Execute()
    {
        while (!IsCancelled)
        {
            var response = await Receive<string>();
            await Send("Pong");
            Log.Info($"Received {response}; Sent Pong");
        }
    }
}
```

There's a second example script in the repo.

Enjoy!
