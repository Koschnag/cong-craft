using CongCraft.Engine.Core;

namespace CongCraft.Game;

class Program
{
    static void Main(string[] args)
    {
        DevLog.Init();

        try
        {
            GameSetup.Run();
        }
        catch (Exception ex)
        {
            DevLog.Error("Fatal crash", ex);
            Console.Error.WriteLine($"\nCongCraft crashed. See log: {DevLog.LogFilePath}");
            Console.Error.WriteLine(ex);
        }
        finally
        {
            DevLog.Shutdown();
        }
    }
}
