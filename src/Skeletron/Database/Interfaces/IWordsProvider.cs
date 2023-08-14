using System.Collections.Generic;

namespace Skeletron.Database.Interfaces
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
