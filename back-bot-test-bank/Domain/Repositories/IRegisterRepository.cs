using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repositories
{
    /*
     * Here we define the interfaz is called also output port
     * 
     * on Infra.Messaging we are going to implement thsi interface is called output adapter
     */
    public interface IRegisterRepository
    {
        void SaveRegister(Register register);
    }
}
