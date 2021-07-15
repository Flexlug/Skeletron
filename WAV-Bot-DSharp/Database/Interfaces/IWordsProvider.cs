using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Database.Interfaces
{
    public interface IWordsProvider
    {
        public bool CheckWord(string word);
        public void AddWord(string word);
        public void DeleteWord(string word);
        public void ClearWords();
        public List<string> GetWords();
    }
}
