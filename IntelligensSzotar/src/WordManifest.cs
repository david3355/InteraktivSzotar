using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelligensSzotar
{
    public class WordMeta
    {
        public WordMeta(string Word, string Language, List<string> Meanings)
        {
            Szó = Word;
            Nyelv = Language;
            StringBuilder stb = new StringBuilder();
            for (int i = 0; i < Meanings.Count; i++)
            {
                stb.Append(Meanings[i]);
                if (i < Meanings.Count - 1)
                    stb.Append(", ");
            }
            Jelentések = stb.ToString();
        }

        public string Szó {get;set;}
        public string Nyelv {get;set;}
        public string Jelentések {get;set;}
    }
}
