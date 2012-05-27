using System;

namespace HappyFunBlob
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (HappyFunBlobGame game = new HappyFunBlobGame())
            {
                game.Run();
            }
        }
    }
}

