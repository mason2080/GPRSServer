
namespace GameServer
{
    /// <summary>
    /// 用于保存已经坐到游戏桌座位上玩家的情况
    /// </summary>
    class Player
    {
        public User user;       //User类的实例
        public bool started;    //是否已经开始
        public int grade;       //成绩

        public bool someone;    //是否有人坐下
        public Player() 
        {
            someone = false;
            started = false;
            grade = 0;
            user = null;
        }
    }
}
