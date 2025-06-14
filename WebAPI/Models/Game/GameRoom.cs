namespace OmokServer.Models.Game
{

    public enum GameStatus
    {
        InProgress,
        Finished
    }
    public record Player(int UserId, string Username);

    public class GameRoom
    {
        public string RoomId { get; }
        public Player Player1 { get; }
        public Player Player2 { get; }
        public GameStatus Status { get; private set; }
        public int TurnOwnerPlayerId { get; private set; }

        private readonly int[,] _board = new int[19, 19];
        private readonly int _boardHeight;
        private readonly int _boardWidth;
        public GameRoom(Player player1, Player player2)
        {
            RoomId = Guid.NewGuid().ToString("N");
            Player1 = player1;
            Player2 = player2;
            Status = GameStatus.InProgress;
            TurnOwnerPlayerId = player1.UserId;
            _boardHeight = _board.GetLength(0);
            _boardWidth = _board.GetLength(1);
        }

        public bool PlaceStone(int playerId, int x, int y)
        {
            if(playerId != TurnOwnerPlayerId)
            {
                return false;
            }
            if(x < 0 || y < 0 || y >= _boardHeight || x >= _boardWidth)
            {
                return false; 
            }
            if (_board[y, x] != 0)
            {
                return false;
            }
            int playerNumber = (playerId == Player1.UserId) ? 1 : 2;
            _board[y, x] = playerNumber;
            if (CheckIfFinished(playerNumber, x, y))
            {
                Status = GameStatus.Finished;
            }
            else
            {
                TurnOwnerPlayerId = (TurnOwnerPlayerId == Player1.UserId) ? Player2.UserId : Player1.UserId;
            }
            return true;
        }
        private bool CheckIfFinished(int playerNumber, int x, int y)
        {
            var dirs = new (int dy, int dx)[] { (1, 0), (0, 1), (1, 1), (1, -1) };
            foreach (var dir in dirs)
            {
                int cnt = 1;
                int negY = y - dir.dy;
                int negX = x - dir.dx;
                int posY = y + dir.dy;
                int posX = x + dir.dx;

                while (negY >= 0 && negY < _boardHeight && negX >= 0 && negX < _boardWidth && _board[negY, negX] == playerNumber)
                {
                    cnt++;
                    negY -= dir.dy;
                    negX -= dir.dx;
                }

                while (posY >= 0 && posY < _boardHeight && posX >= 0 && posX < _boardWidth && _board[posY, posX] == playerNumber)
                {
                    cnt++;
                    posY += dir.dy;
                    posX += dir.dx;
                }

                if (cnt == 5)
                {
                    return true;
                }
            }
            return false;
        }
    }
}