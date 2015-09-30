// compose.cs
using System;


namespace Laboratorio2
{

    /// <summary>
    ///     Delegate do laboratório 2
    /// </summary>
    delegate void MyDelegate(string s);

    /// <summary>
    ///     Estude e execute o exemplo exercicio-1.cs. Modifique o exemplo como achar necessário de forma a responder às seguintes questões:
    ///     <list type="bullet">
    ///         <para>
    ///         <item>
    ///             <term>1.</term>
    ///             <description>Localize no código como é feita a adição, remoção e atribuição de métodos ao delegate e onde é feita a invocação do delegate.</description>
    ///         </item>
    ///         </para>
    ///         <para>
    ///         <item>
    ///             <term>2.</term>
    ///             <description>Por que ordem são executados os métodos registados nos delegates?</description>
    ///         </item>
    ///         </para>
    ///         <para>
    ///         <item>
    ///             <term>3.</term>
    ///             <description>O que acontece se igualar um delegate a null antes do invocar?</description>
    ///         </item>
    ///         </para>
    ///         <para>
    ///         <item>
    ///             <term>4.</term>
    ///             <description>É possível adicionar métodos estáticos a delegates?</description>
    ///         </item>
    ///         </para>
    ///         <para>
    ///         <item>
    ///             <term>5.</term>
    ///             <description>O que acontece aos métodos registados no delegate se um novo método é atribuído? São invocados?</description>
    ///         </item>
    ///         </para>
    ///         <para>
    ///         <item>
    ///             <term>6.</term>
    ///             <description>Verifique que é possível passar um delegate como argumento e ser invocado dentro de um método.</description>
    ///         </item>
    ///         </para>
    ///     </list>
    /// </summary>
    class Exercicio1
    {
        /// <summary>
        ///     Cumprimenta string :)
        /// </summary>
        /// <param name="s">String para ser impresso</param>
        public /*static*/ void Hello(string s)
        {
            Console.WriteLine("  Hello, {0}!", s);
        }

        /// <summary>
        ///     Despede-se de string :(
        /// </summary>
        /// <param name="s">String para ser impresso</param>
        public /*static*/ void Goodbye(string s)
        {
            Console.WriteLine("  Goodbye, {0}!", s);
        }

        /// <summary>
        ///     Processo original
        /// </summary>
        public void Original()
        {
            MyDelegate a, b, c, d;

            // Create the delegate object a that references
            // the method Hello:
            a = new MyDelegate(Hello);
            // Create the delegate object b that references
            // the method Goodbye:
            b = new MyDelegate(Goodbye);
            // The two delegates, a and b, are composed to form c:
            c = a + b;
            // Remove a from the composed delegate, leaving d,
            // which calls only the method Goodbye:
            d = c - a;

            Console.WriteLine("Invoking delegate a:");
            a("A");
            Console.WriteLine("Invoking delegate b:");
            b("B");
            Console.WriteLine("Invoking delegate c:");
            c("C");
            Console.WriteLine("Invoking delegate d:");
            d("D");
        }

        /// <summary>
        ///  Método Exemplo
        /// </summary>
        /// <param name="s">String para ser impresso</param>
        public void Exemplo(string s)
        {
            Console.WriteLine("  {0}", s);
        }

        /// <summary>
        ///  Outro método Exemplo
        /// </summary>
        /// <param name="s">String para ser impresso</param>
        public void OutroExemplo(string s)
        {
            Console.WriteLine("  Outra {0}", s);
        }

        /// <summary>
        ///  Método Estático
        /// </summary>
        /// <param name="s">String para ser impresso</param>
        public static void ExemploEstatico(string s)
        {
            Console.WriteLine("  {0}", s);
        }

        /// <summary>
        ///  Método com delegate com argumento
        /// </summary>
        /// <param name="myDelegate">Delegate para ser invocado</param>
        /// <param name="s">String para ser usado no delegate</param>
        public void Invoca(MyDelegate myDelegate, string s)
        {
            myDelegate(s);
        }

        /// <summary>
        ///     Questão 1:
        ///     <list type="bullet">
        ///         <para>
        ///         <item>
        ///             <term>Pergunta:</term>
        ///             <description>Localize no código como é feita a adição, remoção e atribuição de métodos ao delegate e onde é feita a invocação do delegate.</description>
        ///         </item>
        ///         </para>
        ///         <para>
        ///         <item>
        ///             <term>Resposta:</term>
        ///             <description>Ver código do método Questao1.</description>
        ///         </item>
        ///         </para>
        ///     </list>
        /// </summary>
        /// <seealso cref="MyDelegate"/>
        /// <seealso cref="Exemplo"/>
        public void Questao1()
        {

            MyDelegate atribuicao1, atribuicao2, adicao, subtracao;

            Console.WriteLine("Questão 1:");

            // Atribuição de método a delegate
            atribuicao1 = new MyDelegate(Exemplo);
            atribuicao2 = new MyDelegate(Exemplo);

            // Adição de dois métodos a delegate
            adicao = atribuicao1 + atribuicao2;

            // Remoção de um método do delegate
            subtracao = adicao - atribuicao2;

            // Invocação do simple delegate
            atribuicao1("Isto é uma invocaçao.");

        }

        /// <summary>
        ///     Questão 2:
        ///     <list type="bullet">
        ///         <para>
        ///         <item>
        ///             <term>Pergunta:</term>
        ///             <description>Por que ordem são executados os métodos registados nos delegates?</description>
        ///         </item>
        ///         </para>
        ///         <para>
        ///         <item>
        ///             <term>Resposta:</term>
        ///             <description>Pela ordem com que foram adicionados ao delegate. Ou seja, da cabeça até à cauda da lista do delegate</description>
        ///         </item>
        ///         </para>
        ///     </list>
        /// </summary>
        /// <seealso cref="MyDelegate"/>
        /// <seealso cref="Exemplo"/>
        /// <seealso cref="OutroExemplo"/>
        public void Questao2()
        {

            MyDelegate atribuicao1, atribuicao2, adicao;

            Console.WriteLine("Questão 2:");

            // Atribuicao de métodos a delegates
            atribuicao1 = new MyDelegate(Exemplo);
            atribuicao2 = new MyDelegate(OutroExemplo);

            // Adicao de dois métodos a delegate
            adicao = atribuicao1 + atribuicao2;

            // Invocation do combined delegate
            adicao("invocaçao.");

        }

        /// <summary>
        ///     Questão 3:
        ///     <list type="bullet">
        ///         <para>
        ///         <item>
        ///             <term>Pergunta:</term>
        ///             <description>O que acontece se igualar um delegate a null antes do invocar?</description>
        ///         </item>
        ///         </para>
        ///         <para>
        ///         <item>
        ///             <term>Resposta:</term>
        ///             <description>Quando o delegate é invocado, este lança uma extenção NullReferenceException porque a lista está vazia.</description>
        ///         </item>
        ///         </para>
        ///     </list>
        /// </summary>
        /// <exception cref="NullReferenceException"/>
        /// <seealso cref="MyDelegate"/>
        /// <seealso cref="Exemplo"/>
        /// <seealso cref="NullReferenceException"/>
        public void Questao3()
        {

            MyDelegate atribuicao;

            Console.WriteLine("Questão 3:");

            // Atribuicao de métodos a delegates
            atribuicao = new MyDelegate(Exemplo);

            atribuicao -= new MyDelegate(Exemplo);

            // Invocation do null delegate
            try
            {
                atribuicao("invocaçao");
            }
            catch (NullReferenceException e) {
                Console.WriteLine("  {0}", e.Message);
            }

        }

        /// <summary>
        ///     Questão 4:
        ///     <list type="bullet">
        ///         <para>
        ///         <item>
        ///             <term>Pergunta:</term>
        ///             <description>É possível adicionar métodos estáticos a delegates?</description>
        ///         </item>
        ///         </para>
        ///         <para>
        ///         <item>
        ///             <term>Resposta:</term>
        ///             <description>Sim.</description>
        ///         </item>
        ///         </para>
        ///     </list>
        /// </summary>
        /// <seealso cref="MyDelegate"/>
        /// <seealso cref="ExemploEstatico"/>
        public void Questao4()
        {

            MyDelegate atribuicao;

            Console.WriteLine("Questão 4:");

            // Atribuicao de métodos a delegates
            atribuicao = new MyDelegate(ExemploEstatico);

            // Invocation do delegate com método estático
            atribuicao("Invocaçao de método estático.");

        }

        /// <summary>
        ///     Questão 5:
        ///     <list type="bullet">
        ///         <para>
        ///         <item>
        ///             <term>Pergunta:</term>
        ///             <description>O que acontece aos métodos registados no delegate se um novo método é atribuído? São invocados?</description>
        ///         </item>
        ///         </para>
        ///         <para>
        ///         <item>
        ///             <term>Resposta:</term>
        ///             <description>O delegate aponta para outra lista com o novo método. Dessa forma, os velhos métodos não são invocados.</description>
        ///         </item>
        ///         </para>
        ///     </list>
        /// </summary>
        /// <seealso cref="MyDelegate"/>
        /// <seealso cref="Exemplo"/>
        /// <seealso cref="OutroExemplo"/>
        public void Questao5()
        {

            MyDelegate atribuicao;

            Console.WriteLine("Questão 5:");

            // Atribuicao de métodos a delegates
            atribuicao = new MyDelegate(Exemplo);
            atribuicao = new MyDelegate(OutroExemplo);

            // Invocation do delegate com método estático
            atribuicao("invocação.");

        }

        /// <summary>
        ///     Questão 6:
        ///     <list type="bullet">
        ///         <para>
        ///         <item>
        ///             <term>Pergunta:</term>
        ///             <description>Verifique que é possível passar um delegate como argumento e ser invocado dentro de um método.</description>
        ///         </item>
        ///         </para>
        ///         <para>
        ///         <item>
        ///             <term>Resposta:</term>
        ///             <description>Q.Q.D.</description>
        ///         </item>
        ///         </para>
        ///     </list>
        /// </summary>
        /// <seealso cref="MyDelegate"/>
        /// <seealso cref="Exemplo"/>
        /// <seealso cref="OutroExemplo"/>
        public void Questao6()
        {

            MyDelegate atribuicao;

            Console.WriteLine("Questão 6:");

            // Atribuicao de métodos a delegates
            atribuicao = new MyDelegate(Exemplo);

            // Invocation do delegate com método estático
            Invoca(atribuicao, "invocação.");

        }

    }

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
            Exercicio1 exercicio1 = new Exercicio1();

            exercicio1.Original();

            exercicio1.Questao1();
            exercicio1.Questao2();
            exercicio1.Questao3();
            exercicio1.Questao4();
            exercicio1.Questao5();
            exercicio1.Questao6();

            Console.Read();

        }
    }
}