using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.EAVModels
{
    public class FileRepository
    {
        private EAVEntities entities = new EAVEntities();

        public result_files AddResultFile(result_files file)
        {
            entities.result_files.Add(file);
            entities.SaveChanges();
            return file;
        }
    }
}
