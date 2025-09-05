namespace Client.Rede
{
    public class HandleGame
    {
        private readonly TcpClientWrapper client;

        public HandleGame(TcpClientWrapper client)
        {
            this.client = client;
        }

        public async Task SendPlayerActionAsync(bool left, bool right, bool up, bool down, bool shoot)
        {
            var msg = new
            {
                type = "PlayerAction",
                left,
                right,
                up,
                down,
                shoot
            };

            await client.SendAsync(msg);
        }
    }
}
