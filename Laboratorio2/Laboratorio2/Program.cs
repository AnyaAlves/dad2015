// Program.cs
using System;


namespace Laboratorio2
{

    /// <summary>
    ///     Classe da main
    /// </summary>
    class Application
    {
        /// <summary>
        ///  A tua main :v
        /// </summary>
        public static void Main()
        {
            MyDelegate.Delegate exercicio1 = new MyDelegate.Delegate();

            exercicio1.Original();

            exercicio1.Questao1();
            exercicio1.Questao2();
            exercicio1.Questao3();
            exercicio1.Questao4();
            exercicio1.Questao5();
            exercicio1.Questao6();

            MyThread.Threads.Main();
        }
    }
}