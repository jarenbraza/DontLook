using System.Runtime.CompilerServices;

using UnityEngine.Scripting;

// prevent Il2CPP code stripping
[assembly: AlwaysLinkAssembly]

[assembly: InternalsVisibleTo("Unity.Services.Friends.IntegrationTests")]
[assembly: InternalsVisibleTo("Unity.Services.Lobby.IntegrationTests")]
[assembly: InternalsVisibleTo("Unity.Services.Multiplayer.IntegrationTests")]
[assembly: InternalsVisibleTo("Unity.Services.Wire.Tests")]
[assembly: InternalsVisibleTo("Unity.Services.Wire.EditorTests")]
[assembly: InternalsVisibleTo("Unity.Services.Wire.IntegrationTests")]
[assembly: InternalsVisibleTo("Unity.Services.Multiplayer.IntegrationTests")]
