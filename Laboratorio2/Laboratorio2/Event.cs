using System;


namespace Laboratorio2
{
    /// <summary>
    /// este delegate e' a base para o event Move do slider
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    delegate void MoveEventHandler(object source, MoveEventArgs e);

    // esta  classe contem os argumentos do evento move do slider
    public class MoveEventArgs : EventArgs
    {
        private int value;
        public bool valid = false;

        public MoveEventArgs(int i)
        {
            this.value = i;
        }

        public int Value
        {
            get
            {
                return this.value;
            }
        }
    }

    class Slider
    {
        private int position;

        public event MoveEventHandler Move;

        public int Position
        {
            get
            {
                return this.position;
            }
            // e' este bloco que e' executado quando se move o slider
            set
            {
                if (Move != null)
                {
                    MoveEventArgs args = new MoveEventArgs(value);
                    Move(this, args);
                    if (args.valid)
                    {
                        position = args.Value;
                    }
                }
            }
        }

    }


    class Form
    {
        static void Original()
        {
            Slider slider = new Slider();

            // TODO: register with the Move event
            slider.Move += new MoveEventHandler(slider_Move);

            // estas sao as duas alteracoes simuladas no slider
            slider.Position = 20;
            slider.Position = 60;
        }

        // este é o método que deve ser chamado quando o slider e' movido
        public static void slider_Move(object source, MoveEventArgs e)
        {
            if (e.Value >  50)
            {
                e.valid = true;
            }
        }
    }
}