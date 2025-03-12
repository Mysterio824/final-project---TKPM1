using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKPM_final_project.Repository
{
    public interface IRepository<T>
    {
        List<T> GetAll();
        //T GetById(string id);

        //int Insert(T entity);
        //int Update(T entity);
        //int Delete(T entity);
    }
}
