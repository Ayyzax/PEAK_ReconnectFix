using System.Linq;
using System.Collections;
using BepInEx.Logging;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Multiplayer.Playmode;
using Zorro.Core;

namespace Ayzax.ReconnectFix;

public class ReconnectFixer : MonoBehaviourPunCallbacks
{
    ManualLogSource _logger;

    public void InitLogger(ManualLogSource logger)
    {
        _logger = logger;
    }

    public override void OnJoinedRoom()
    {
        StartCoroutine(SpawnLocalPlayerFixed());
    }

    IEnumerator SpawnLocalPlayerFixed()
    {
        yield return null;

        // Only perform fix if in a broken state
        if (Character.localCharacter != null)
        {
            yield break;
        }

        _logger.LogInfo("No player character found, performing reconnect fix!");

        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        int index = PhotonNetwork.LocalPlayer.ActorNumber % SpawnPoint.allSpawnPoints.Count;
        SpawnPoint spawnPoint = SpawnPoint.allSpawnPoints.FirstOrDefault((SpawnPoint s) => s.index == index);
        if (spawnPoint == null)
        {
            spawnPoint = SpawnPoint.allSpawnPoints[0];
        }
        if (spawnPoint != null)
        {
            position = spawnPoint.transform.position;
            rotation = spawnPoint.transform.rotation;
            _logger.LogInfo($"Setting player{PhotonNetwork.LocalPlayer.ActorNumber} to spawn point {spawnPoint.index}");
        }
        else
        {
            _logger.LogError("No Spawn Point, make on in the scene!");
        }
        bool flag = SceneManager.GetActiveScene().name == "Airport";
        if (!GameHandler.TryGetStatus<SceneSwitchingStatus>(out var _) && !flag)
        {
            // ReconnectFix: This throws an error that prevents character from spawning
            //if (RoomProperties.me.IsReconnecting() && RoomProperties.me.GetReconnectPosition(out position))
            //{
            //}
        }
        else
        {
            GameHandler.ClearStatus<SceneSwitchingStatus>();
        }
        string[] source = CurrentPlayer.ReadOnlyTags();
        if (Singleton<MapHandler>.Instance != null && Singleton<MapHandler>.Instance.GetCurrentSegment() != Segment.Beach)
        {
            Segment currentSegment = Singleton<MapHandler>.Instance.GetCurrentSegment();
            position = Singleton<MapHandler>.Instance.segments[(uint)currentSegment].reconnectSpawnPos.position;
        }
        if (!source.Contains("NoCharacter"))
        {
            if (Character.localCharacter == null)
            {
                _logger.LogInfo("Spawning local character.");
                Character component = PhotonNetwork.Instantiate("Character", position, rotation, 0).GetComponent<Character>();
                component.data.spawnPoint = spawnPoint.transform;
                // ReconnectFix: Can't use this method as it is internal :/
                //if (spawnPoint.startPassedOut)
                //{
                //    component.StartPassedOutOnTheBeach();
                //}
            }
            else
            {
                _logger.LogInfo("Moving local character to warp point.");
                Character.localCharacter.photonView.RPC("WarpPlayerRPC", RpcTarget.All, position, false);
                Character.localCharacter.data.spawnPoint = spawnPoint.transform;
            }
        }
        if (Player.localPlayer == null)
        {
            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
        }
        if (!flag)
        {
            RoomProperties.me.Reconnect();
        }
        else
        {
            RoomProperties.me.Clear();
        }
    }
}
