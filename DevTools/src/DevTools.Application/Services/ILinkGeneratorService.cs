using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.Application.Services
{
    public interface ILinkGeneratorService
    {
        string GenerateEmailVerificationLink(string token);
        string GeneratePasswordResetLink(string token);
    }

}
