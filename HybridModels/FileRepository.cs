using MHGR.HybridModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.HybridModels
{
    public class FileRepository
    {
        private HybridEntities entities = new HybridEntities();


        public result_files AddResultFile(result_files file)
        {
            entities.result_files.Add(file);
            entities.SaveChanges();
            return file;
        }
    }
}
