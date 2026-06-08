using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    internal class Desordenador
    {
        const int DEFAULT_SIZE = 100;
        private int[] vector;
        public int[] Vector
        {
            get { return vector; }
        }
        public Desordenador()
        {
            vector = new int[DEFAULT_SIZE];
        }
        public Desordenador(int size)
        {
            vector = new int[size];
        }
        public void Fill()
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = i + 1;
            }
        }
        public void Shuffle()
        {
            Random random = new Random();
            for (int i = 0; i < vector.Length; i++)
            {
                int j = random.Next(i, vector.Length);
                int temp = vector[i];
                vector[i] = vector[j];
                vector[j] = temp;
            }
        }
    }
}
