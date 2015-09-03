using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.EAVModels
{
    public class SourceRepository
    {
        private EAVEntities entities = new EAVEntities();

        public result_sources AddSource(string name, string description)
        {
            var source = (from src in entities.result_sources
                          where src.name == name
                          select src).FirstOrDefault();
            if (source == null)
            {
                source = new result_sources()
                {
                    name = name,
                    description = description,
                };
                entities.result_sources.Add(source);
                entities.SaveChanges();
            }

            return source;
        }
    }
}
