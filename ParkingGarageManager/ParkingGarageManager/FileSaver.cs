using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class FileSaver
    {
        public string FileName { get; private set; }

        public FileSaver(string fileName)
        {
            this.FileName = fileName;
            if(!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }
        }

        public void SaveData(IEnumerable<string> lines)
        {
            File.WriteAllLines(this.FileName, lines);
        }


    }
}