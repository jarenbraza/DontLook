//-----------------------------------------------------------------------------
// <auto-generated>
//     This file was generated by the C# SDK Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Services.Lobbies.Http;



namespace Unity.Services.Lobbies.Models
{
    /// <summary>
    /// The body of a Bulk Update request.
    /// </summary>
    [Preserve]
    [DataContract(Name = "BulkUpdateRequest")]
    internal class BulkUpdateRequest
    {
        /// <summary>
        /// The body of a Bulk Update request.
        /// </summary>
        /// <param name="lobbyUpdate">lobbyUpdate param</param>
        /// <param name="playerUpdates">A mapping from player IDs to player update requests.</param>
        /// <param name="playersToAdd">An array of players to add to the lobby.</param>
        /// <param name="playersToRemove">An array of player IDs to remove from the lobby.</param>
        /// <param name="ignoreIneffectualUpdates">Whether or not to silently ignore ineffectual updates (i.e. removing or updating players who are not in the lobby) instead of failing.</param>
        [Preserve]
        public BulkUpdateRequest(UpdateRequest lobbyUpdate = default, Dictionary<string, PlayerUpdateRequest> playerUpdates = default, List<Player> playersToAdd = default, List<string> playersToRemove = default, bool? ignoreIneffectualUpdates = false)
        {
            LobbyUpdate = lobbyUpdate;
            PlayerUpdates = playerUpdates;
            PlayersToAdd = playersToAdd;
            PlayersToRemove = playersToRemove;
            IgnoreIneffectualUpdates = ignoreIneffectualUpdates;
        }

        /// <summary>
        /// Parameter lobbyUpdate of BulkUpdateRequest
        /// </summary>
        [Preserve][JsonConverter(typeof(JsonObjectConverter))]
        [DataMember(Name = "lobbyUpdate", EmitDefaultValue = false)]
        public UpdateRequest LobbyUpdate{ get; }

        /// <summary>
        /// A mapping from player IDs to player update requests.
        /// </summary>
        [Preserve][JsonConverter(typeof(JsonObjectCollectionConverter))]
        [DataMember(Name = "playerUpdates", EmitDefaultValue = false)]
        public Dictionary<string, PlayerUpdateRequest> PlayerUpdates{ get; }

        /// <summary>
        /// An array of players to add to the lobby.
        /// </summary>
        [Preserve]
        [DataMember(Name = "playersToAdd", EmitDefaultValue = false)]
        public List<Player> PlayersToAdd{ get; }

        /// <summary>
        /// An array of player IDs to remove from the lobby.
        /// </summary>
        [Preserve]
        [DataMember(Name = "playersToRemove", EmitDefaultValue = false)]
        public List<string> PlayersToRemove{ get; }

        /// <summary>
        /// Whether or not to silently ignore ineffectual updates (i.e. removing or updating players who are not in the lobby) instead of failing.
        /// </summary>
        [Preserve]
        [DataMember(Name = "ignoreIneffectualUpdates", EmitDefaultValue = true)]
        public bool? IgnoreIneffectualUpdates{ get; }

        /// <summary>
        /// Formats a BulkUpdateRequest into a string of key-value pairs for use as a path parameter.
        /// </summary>
        /// <returns>Returns a string representation of the key-value pairs.</returns>
        internal string SerializeAsPathParam()
        {
            var serializedModel = "";

            if (LobbyUpdate != null)
            {
                serializedModel += "lobbyUpdate," + LobbyUpdate.ToString() + ",";
            }
            if (PlayerUpdates != null)
            {
                serializedModel += "playerUpdates," + PlayerUpdates.ToString() + ",";
            }
            if (PlayersToAdd != null)
            {
                serializedModel += "playersToAdd," + PlayersToAdd.ToString() + ",";
            }
            if (PlayersToRemove != null)
            {
                serializedModel += "playersToRemove," + PlayersToRemove.ToString() + ",";
            }
            if (IgnoreIneffectualUpdates != null)
            {
                serializedModel += "ignoreIneffectualUpdates," + IgnoreIneffectualUpdates.ToString();
            }
            return serializedModel;
        }

        /// <summary>
        /// Returns a BulkUpdateRequest as a dictionary of key-value pairs for use as a query parameter.
        /// </summary>
        /// <returns>Returns a dictionary of string key-value pairs.</returns>
        internal Dictionary<string, string> GetAsQueryParam()
        {
            var dictionary = new Dictionary<string, string>();

            if (PlayersToRemove != null)
            {
                var playersToRemoveStringValue = PlayersToRemove.ToString();
                dictionary.Add("playersToRemove", playersToRemoveStringValue);
            }

            if (IgnoreIneffectualUpdates != null)
            {
                var ignoreIneffectualUpdatesStringValue = IgnoreIneffectualUpdates.ToString();
                dictionary.Add("ignoreIneffectualUpdates", ignoreIneffectualUpdatesStringValue);
            }

            return dictionary;
        }
    }
}
