using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO;
using Domain.Repositories;

namespace Application.UseCases
{
    public class RegisterUseCase : IRegisterUseCase
    {
        public readonly IRegisterRepository _repository;

        public RegisterUseCase(IRegisterRepository repository)
        {
            _repository = repository;
        }
        public void Register(RegisterDTO register)
        {
            var domainRegister = new Domain.Models.Register() 
            {
                DocumentNumber = register.DocumentNumber,
                DocumentType = register.DocumentType,
                Name = register.Name,
                SeatsTaken = register.SeatsTaken
            };

            _repository.SaveRegister(domainRegister);
        }
    }
}
