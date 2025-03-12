using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TKPM_final_project.Repository;

namespace TKPM_final_project.Service
{
    public class MockDao : IDao
    {

        public class MockCategory : IRepository<Object>
        {
            throw new NotImplementedException();
        }
        public IRepository<Object> data { get; set; } = new MockCategory();
    }
}
