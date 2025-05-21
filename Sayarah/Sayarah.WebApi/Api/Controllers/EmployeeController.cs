using Abp.WebApi.Controllers;
using Sayarah.Api.Models;
using Sayarah.Helpers;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Generic;
using Sayarah.Companies;
using Sayarah.Companies.Dto;
using Sayarah.Chips.Dto;
using Sayarah.Chips;
using Sayarah.Helpers.Enums;
using Abp.Application.Services.Dto;
using Sayarah.Users;

namespace Sayarah.Api.Controllers
{
    public class EmployeeController : AbpApiController
    {
        private readonly IBranchAppService _branchAppService;
        private readonly ICompanyAppService _companyAppService;
        private readonly IChipNumberAppService _chipNumberAppService;
        private readonly IUserAppService _userAppService;
        public EmployeeController(

        IBranchAppService branchAppService,
       ICompanyAppService companyAppService,
       IChipNumberAppService chipNumberAppService,
       IUserAppService userAppService
                               )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;

            _branchAppService = branchAppService;
            _companyAppService = companyAppService;
            _chipNumberAppService = chipNumberAppService;
            _userAppService = userAppService;
        }

        CultureInfo new_lang = new CultureInfo("ar");

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> GetAllCompanies(GetCompaniesInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                   return Unauthorized();

                input.MaxCount = true;
                var companies = await _companyAppService.GetAllCompanies(input);
                return Ok(new GetCompaniesOutput { Success = true, Companies = companies });
            }
            catch (Exception ex)
            {
                return Ok(new GetCompaniesOutput { Message = ex.Message, Success = false, Companies = new List<CompanyNameDto>() });
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> GetAllCompaniesByRole()
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    return Unauthorized();
                var user = await _userAppService.GetById(new EntityDto<long> { Id = AbpSession.UserId.Value });
                if(user.UserType == UserTypes.AdminEmployee)
                {

                    var companies = await _companyAppService.GetAllCompaniesAdminEmployee();

                    return Ok(new GetCompaniesOutput { Success = true, Companies = companies });

                }
                else if(user.UserType == UserTypes.Company)
                {

                    var companies = await _companyAppService.GetByUserIdForCompanyUser(new EntityDto<long> { Id = AbpSession.UserId.Value });

                    return Ok(new GetCompaniesOutput { Success = true, Companies = companies });
                }
                
                    return Ok(new GetCompaniesOutput { Message = L("MobileApi.Messages.UserTypeNotValid"), Success = false, Companies = new List<CompanyNameDto>() });
                

            }
            catch (Exception ex)
            {
                return Ok(new GetCompaniesOutput { Message = ex.Message, Success = false, Companies = new List<CompanyNameDto>() });
            }
        }

            
        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> GetAllBranchs(GetBranchesInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    return Unauthorized();
                input.MaxCount = true;
                var branchs = await _branchAppService.GetAllBranchs(input);
                return Ok(new GetBranchsOutput { Success = true, Branchs = branchs });
            }
            catch (Exception ex)
            {
                return Ok(new GetBranchsOutput { Message = ex.Message, Success = false, Branchs = new List<BranchNameDto>() });
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> LinkByChipsEmployee(LinkByChipsEmployee input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    return Unauthorized();
                var chips = await _chipNumberAppService.LinkByChipsEmployee(input);
                return Ok(new _LinkByChipsEmployeeOutput { Success = true, Chips = chips });
            }
            catch (Exception ex)
            {
                return Ok(new _LinkByChipsEmployeeOutput { Message = ex.Message, Success = false, Chips = new LinkByChipsEmployeeOutput() });
            }
        }

    }
}