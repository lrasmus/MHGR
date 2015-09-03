using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter
{
    public abstract class BaseLoader
    {
        public abstract void LoadData(string filePath);

        /// <summary>
        /// Utility function to verify an expected count matches an actual one
        /// </summary>
        /// <param name="expectedCount"></param>
        /// <param name="actualCount"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        protected static bool CheckEntityCounts(int expectedCount, int actualCount, string entityName)
        {
            if (actualCount != expectedCount)
            {
                Console.WriteLine("Expected {0} {1}, but counted {2}", expectedCount, entityName, actualCount);
                return false;
            }

            return true;
        }

        public string GetFileHash(string filePath)
        {
            string hash = null;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
            return hash;
        }
    }
}
