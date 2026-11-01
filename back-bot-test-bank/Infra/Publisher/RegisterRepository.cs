using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repositories;

namespace Infra.Publisher
{
    public class RegisterRepository : IRegisterRepository
    {
        /*
         * Adapter Implementation of port output port
         */
        public void SaveRegister(Register register)
        {
            //Implemetation to save in DataBase
            throw new NotImplementedException();
        }
    }
}
