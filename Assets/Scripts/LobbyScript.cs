using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyScript : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset lobbyListItemTemplate;

    private ScrollView lobbyList;
    private Lobby hostedLobby;
    private Lobby joinedLobby;
    private Button createLobbyButton;
    private Button refreshLobbiesButton;

    // Ping the lobby every 15 seconds to avoid it being deleted from inactivity
    private const float MaxHeartbeatTime = 15f;
    private float heartbeatTimeRemaining = MaxHeartbeatTime;

    // Check the lobby every 3 seconds to check for updates
    private const float MaxLobbyUpdateTime = 3f;
    private float lobbyUpdateTimeRemaining = MaxLobbyUpdateTime;

    private async void Start() {
        Debug.developerConsoleVisible = true;

        lobbyList = GetComponent<UIDocument>().rootVisualElement.Q<ScrollView>("LobbyList");
        createLobbyButton = GetComponent<UIDocument>().rootVisualElement.Q<Button>("CreateLobbyButton");
        refreshLobbiesButton = GetComponent<UIDocument>().rootVisualElement.Q<Button>("RefreshLobbiesButton");

        createLobbyButton.clicked += CreateLobby;
        refreshLobbiesButton.clicked += ListLobbies;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update() {
        HandleLobbyHeartbeat();
        HandleLobbyUpdates();
    }

    private async void HandleLobbyHeartbeat() {
        if (hostedLobby != null) {
            heartbeatTimeRemaining -= Time.deltaTime;
            if (heartbeatTimeRemaining < 0.0f) {
                Debug.Log($"Pinging heartbeat for lobby {hostedLobby.Id}");
                heartbeatTimeRemaining = MaxHeartbeatTime;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostedLobby.Id);
            }
        }
    }

    private async void HandleLobbyUpdates() {
        if (joinedLobby != null) {
            lobbyUpdateTimeRemaining -= Time.deltaTime;
            if (lobbyUpdateTimeRemaining < 0.0f) {
                Debug.Log($"Checking for updates on lobby {joinedLobby.Id}");
                lobbyUpdateTimeRemaining = MaxLobbyUpdateTime;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                // TODO: Update lobby based on any changes
            }
        }
    }

    private async void CreateLobby() {
        if (joinedLobby != null) {
            await LeaveLobby();
        }

        try {
            hostedLobby = await LobbyService.Instance.CreateLobbyAsync("TestLobby", 4);
            joinedLobby = hostedLobby;
            Debug.Log($"Created lobby {hostedLobby.Id}");
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void JoinLobby(string lobbyId) {
        if (joinedLobby != null) {
            await LeaveLobby();
        }

        try {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            Debug.Log($"Joined lobby {lobbyId}");
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void ListLobbies() {
        try {
            var lobbiesQuery = await LobbyService.Instance.QueryLobbiesAsync();
            lobbyList.Clear();
            foreach (var lobby in lobbiesQuery.Results) {
                Debug.Log($"Found lobby {lobby.Id}");
                lobbyList.Add(CreateLobbyListItem(lobby));
            }
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async Task LeaveLobby() {
        try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            Debug.Log($"Player {AuthenticationService.Instance.PlayerId} left lobby {joinedLobby.Id}");
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        } finally {
            hostedLobby = null;
            joinedLobby = null;
        }
    }

    private VisualElement CreateLobbyListItem(Lobby lobby) {
        var lobbyListItem = lobbyListItemTemplate.Instantiate();
        var lobbyId = lobby.Id;
        lobbyListItem.Q<Label>("LobbyIdLabel").text = lobbyId[..6];
        lobbyListItem.Q<Label>("PlayerCountLabel").text = $"Players: {lobby.Players.Count}/{lobby.MaxPlayers}";
        lobbyListItem.Q<Button>("JoinLobbyButton").clicked += () => JoinLobby(lobbyId);
        return lobbyListItem;
    }
}
