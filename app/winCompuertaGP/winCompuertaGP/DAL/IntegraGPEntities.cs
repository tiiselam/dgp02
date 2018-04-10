using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace winCompuertaGP.DAL
{
    public partial class INTEGRAGPEntities : DbContext
    {
        public INTEGRAGPEntities(String connectionString) : base(connectionString)
        {

        }

    }
}
