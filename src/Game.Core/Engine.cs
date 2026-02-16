namespace Game.Core
{
    /// <summary>
    /// 凯恩斯引擎核心
    /// </summary>
    public class Engine
    {
        public string Version { get; } = "0.1.0";
        
        public void Initialize()
        {
            System.Console.WriteLine($"KeynesEngine v{Version} initialized");
        }
        
        public void Start()
        {
            System.Console.WriteLine("Engine started successfully!");
        }
    }
}
