using System.Collections.Generic;
using Common;
using GamePlay.Data;

namespace GamePlay.Services
{
    public interface IStaticDataService : IService
    {
        void LoadItems();
        ItemConfigure GetItem(int id);
        void LoadInventories();
        List<ItemConfigure> GetInventory(GameClass gameClass);
        void LoadPlayerCharacteristics();
        PlayerCharacteristic GetPlayerCharacteristic(GameClass gameClass);
        void LoadLobbyBalance();
        LobbyBalance GetLobbyBalance();
        void LoadVoxelHealthBalance();
        VoxelHealthBalance GetVoxelHealthBalance();
        void LoadSounds();
        AudioData GetAudio(int soundId);
        int GetAudioIndex(AudioData audio);
        void LoadFallDamageConfiguration();
        FallDamageData GetFallDamageConfiguration();
        void LoadRectPaletteData();
        RectPaletteData GetRectPaletteData();
    }
}