namespace MineCS.mccs
{
    public class Session
    {
        public string username;
        public string sessionId;

        public Session(string username, string sessionId)
        {
            this.username = username;
            this.sessionId = sessionId;
        }
    }
}
