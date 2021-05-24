using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomKVisibility : SpatialHashingInterestManagement
{

    #region ======= CLASS & UNITY INSPECTOR FIELDS

    [Tooltip("Visibility will be limit by room Concept")] [SerializeField]
    private bool roomBasedVisibility;

    private double lastRebTime;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        H.klog($" This is New My Kustom , Network match checker, using global interest manager");
        checkMethod = CheckMethod.XY_FOR_2D;
        roomBasedVisibility = true;
    }

    // Update is called once per frame
    void Update()
    {
        //H.klog($"Testroom K running");
        if(!NetworkServer.active)   return;
        
        // clear all the grid, without poppulation
        foreach (HashSet<NetworkConnection> tset in grid.Values)
        {
            tset.Clear();
        }

        foreach (NetworkConnectionToClient client in NetworkServer.connections.Values)
        {
            if (client.isAuthenticated && client.identity != null)
            {
                Vector2Int pos = PosOnGrid(client.identity.transform.position);
                AddtoGrid(pos, client);
            }
        }

        // rebuild all spawned entities' observers every 'interval'
        // this will call OnRebuildObservers which then returns the
        // observers at grid[position] for each entity.
        if (NetworkTime.time >= lastRebTime + rebuildInterval)
        {
            RebuildAll();
            lastRebTime = NetworkTime.time;
        }
    }

    // key: postion, value: list of conn at that position
    private Dictionary<Vector2Int, HashSet<NetworkConnection>> grid =
        new Dictionary<Vector2Int, HashSet<NetworkConnection>>();
    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
    {
        if ( (identity.GetComponent<PlayerMatchId>() == null) || (identity.GetComponent<PlayerMatchId>().matchGuid == null) )
        {
            H.klog2($"Warning, Required Match id Component", name, "ffc65c");
            return false;
        }
        
        Guid guid = (identity.GetComponent<PlayerMatchId>().matchGuid);
        Guid connguid = ( newObserver.identity.GetComponent<PlayerMatchId>().matchGuid);
        
        if (guid == null || connguid == null)
        {
            H.klog2($"Error, Not a guid structure ", name, "red");
            return false;
        }

        if (guid == connguid)
        {
            H.klog2($"Found netid {identity.netId}  have same match id with netid {newObserver.identity.netId}", name, "00e0e0");
            return base.OnCheckObserver(identity, newObserver);
        }
        else
            return false;
    }
    
    public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize)
    {
        //H.klog($"My K Rebuild is called");
        // add everyone in 9 neighbour grid
        // -> pass observers to GetWithNeighbours directly to avoid allocations
        //    and expensive .UnionWith computations.
        Vector2Int current =PosOnGrid(identity.transform.position);
        newObservers.Clear();

        foreach (Vector2Int offset in neighbourOffsets)
        {
            if (grid.TryGetValue(current + offset, out HashSet<NetworkConnection> tmp))
            {
                foreach (NetworkConnection conn in tmp)
                {
                    if (roomBasedVisibility)
                    {
                        // todo: modify this so it will support room based visibity
                        // add to newobserver if it has same match id
                        if (conn.identity.GetComponent<PlayerMatchId>() != null &&
                            identity.GetComponent<PlayerMatchId>() != null)
                        {
                            if (conn.identity.GetComponent<PlayerMatchId>().matchGuid ==
                                identity.GetComponent<PlayerMatchId>().matchGuid)
                            {
                                //H.klog($"Same MatchID, Conn id to add to new observer-- {conn.connectionId}");
                                newObservers.Add(conn);
                            }
                        }
                        
                    }
                    else
                    {
                        //H.klog($"Non-Room based Visibility");
                        newObservers.Add(conn);
                    }
                }
            }
        }
    }


    #region ==============  mini things for used

    private Vector2Int[] neighbourOffsets =
    {
        Vector2Int.up, Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right, Vector2Int.left
        , Vector2Int.zero, Vector2Int.right, Vector2Int.down, Vector2Int.down + Vector2Int.left
        , Vector2Int.down + Vector2Int.right
    };


    private Vector2Int PosOnGrid(Vector3 inpos)
    {
        Vector2Int res;
        if (checkMethod == CheckMethod.XZ_FOR_3D)
        {
            res = Vector2Int.RoundToInt(new Vector2(inpos.x, inpos.z) / resolution);
        }
        else
        {
            res = Vector2Int.RoundToInt(new Vector2(inpos.x, inpos.y) / resolution);
        }

        return res;
    }

    private void AddtoGrid(Vector2Int pos, NetworkConnection val)
    {
        if (!grid.ContainsKey(pos))
        {
            grid[pos] = new HashSet<NetworkConnection>();
        }

        grid[pos].Add(val);
    }
    #endregion
}
