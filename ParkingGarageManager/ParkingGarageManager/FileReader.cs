using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingGarageManager
{
    public class FileReader
    {
        public string FileName { get; private set; }

        public FileReader(string fileName)
        {
            this.FileName = fileName;
        }

        public string[] ReadFile()
        {
            return File.ReadAllLines(this.FileName);
        }
    }
}