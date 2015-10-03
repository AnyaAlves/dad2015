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
            Delegate exercicio1 = new Delegate();
            Threads exercicio3 = new Threads();

            exercicio1.Original();

            exercicio1.Questao1();
            exercicio1.Questao2();
            exercicio1.Questao3();
            exercicio1.Questao4();
            exercicio1.Questao5();
            exercicio1.Questao6();

            exercicio3.Original();

        }
    }
}