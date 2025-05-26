using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Sayarah.Api.Models;
using Sayarah.Application.Chips;
using Sayarah.Application.Chips.Dto;
using Sayarah.Application.Companies;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Users;
using Sayarah.Core.Helpers;
using System.Globalization;

namespace Sayarah.Api.Controllers
{
    [ApiController]

    public class EmployeeController : AbpController
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
        public async Task<IActionResult> GetAllCompanies(GetCompaniesInput input)
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
        public async Task<IActionResult> GetAllCompaniesByRole()
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
        public async Task<IActionResult> GetAllBranchs(GetBranchesInput input)
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
        public async Task<IActionResult> LinkByChipsEmployee(LinkByChipsEmployee input)
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