using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.EAVModels
{
    public class AttributeRepository
    {
        private EAVEntities entities = new EAVEntities();

        public attribute FindUniqueAttributeByCode(string code, string codeSystem)
        {
            var results = entities.attributes.Where(x => x.code == code && x.code_system == codeSystem).ToArray();
            if (results != null && results.Length == 1)
            {
                return results[0];
            }

            return null;
        }

        public attribute FindUniqueAttributeByName(string name)
        {
            var results = entities.attributes.Where(x => x.name == name).ToArray();
            if (results != null && results.Length == 1)
            {
                return results[0];
            }

            return null;
        }
    }
}
