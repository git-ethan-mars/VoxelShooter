using Data;
using Mirror;
using UnityEngine;

namespace GamePlay
{
    public class KillList
    {
        public readonly SyncList<KillData> Kills = new SyncList<KillData>();

        public void AddKill(KillData kill)
        {
            if (kill is null)
            {
                throw new System.ArgumentNullException(nameof(kill));
            }
            
            Kills.Add(kill);
            Debug.Log(kill);
        }
    }
}