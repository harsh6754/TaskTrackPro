using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IRegisterLoginInterface
    {
        Task<int> Register(t_Register register);
        Task<t_Register> Login (t_Login login);

        Task<List<t_Country>> GetCountries();   
        Task<List<t_State>> GetStates(int countryId);
        Task<List<t_District>> GetDistricts(int stateId);
        Task<List<t_City>> GetCities(int districtId);

        Task<int> ChangePassword(t_ChangePassword changePassword);

        Task<t_UserUpdateProfile> UpdateProfile(t_UserUpdateProfile userUpdateProfile);
    }

}