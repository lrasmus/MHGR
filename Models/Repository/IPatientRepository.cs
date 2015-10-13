using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Repository
{
    public interface IPatientRepository
    {
        List<Patient> Search(string search, int? limit);
        Patient Get(int id);
    }
}
