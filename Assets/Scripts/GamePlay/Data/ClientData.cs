namespace GamePlay.Data
{
    public class ClientData
    {
        public readonly string NickName;
        public GameClass GameClass { get; set; } = GameClass.None;
        public bool IsAlive { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public ClientData(string nickName)
        {
            NickName = nickName;
        }
    }
}