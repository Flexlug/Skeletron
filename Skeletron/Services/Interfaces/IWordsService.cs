using System.Collections.Generic;

namespace WAV_Bot_DSharp.Services.Interfaces
{
    public interface IWordsService
    {
        public bool CheckWord(string word);
        public void AddWord(string word);
        public void DeleteWord(string word);
        public void ClearWords();
        public List<string> GetWords();
    }
}
