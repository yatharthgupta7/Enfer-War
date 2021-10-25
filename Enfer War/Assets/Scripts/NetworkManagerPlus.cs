using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerPlus : NetworkManager
{
    [SerializeField]
    List<Transform> m_spawnPositions = new List<Transform>();

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject player = (GameObject)Instantiate(playerPrefab, GetSpawnPosition(numPlayers), Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);

        player.GetComponent<PlayerLogic>().SetTeam((numPlayers - 1) % 2 == 0 ? Team.Blue : Team.Red);

        Debug.Log("Player spawned with Index: " + (numPlayers - 1));
    }

    Vector3 GetSpawnPosition(int spawnIndex)
    {
        return m_spawnPositions[spawnIndex].position;
    }
}
