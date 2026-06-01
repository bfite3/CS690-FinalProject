using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class FileManager
    {
        public string FileName { get; private set; }
        public FileReader FileReader { get; private set; }
        public FileSaver FileSaver { get; private set; }
        public string[] FileContents { get; private set; }
         public FileManager(string fileName)
        {
            this.FileName = fileName;
            this.FileReader = new FileReader(fileName);
            this.FileSaver = new FileSaver(fileName);
            this.FileContents = [];
        }

        public void LoadData()
        {
            this.FileContents = this.FileReader.ReadFile();
        }

        public string[] ReturnData()
        {
            this.LoadData();
            return this.FileContents;
        }

        public void SaveData(IEnumerable<string> lines)
        {
            this.FileSaver.SaveData(lines);
        }
    }
}