﻿#region	License
//------------------------------------------------------------------------------------------------
// <Auto-generated>
//     <Author> Top Nguyen (http://topnguyen.net) </Author>
//     <Project> TopCore.WebAPI.Service.Facade </Project>
//     <File> 
//         <Name> UserService.cs </Name>
//         <Created> 29 03 2017 11:55:30 PM </Created>
//         <Key> 6B0A8F26-EBEE-4D43-8F5B-6A851550F37D </Key>
//     </File>
//     <Summary>
//         UserService
//     </Summary>
// </Auto-generated>
//------------------------------------------------------------------------------------------------
#endregion License

using TopCore.WebAPI.Business;
using TopCore.Framework.DependencyInjection.Attributes;

namespace TopCore.WebAPI.Service.Facade
{
    [PerRequestDependency(ServiceType = typeof(IUserService))]
    public class UserService : IUserService
    {
        private readonly IUserBusiness _userBusiness;

        public UserService(IUserBusiness userBusiness)
        {
            _userBusiness = userBusiness;
        }

        public void Add(string userName)
        {
            _userBusiness.Add(userName);
        }
    }
}