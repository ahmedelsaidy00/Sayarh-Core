using Sayarah.Application.Chips.Dto;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Sayarah.Api.Models
{
    public class StringOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }



    public class ImageSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }




    public class CreateDriverCodeInput
    {
        public long? DriverId { get; set; }
        public long VechileId { get; set; }
        public string DriverCode { get; set; }
    }
    public class CreateDriverCodeOutput
    {
        public string DriverCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

    }
    public class CreateClientInput
    {
        public string Lang { get; set; }
        public string Name { get; set; }
        //[Required]
        public string UserName { get; set; }
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
        public Gender? Gender { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        public DeviceType DeviceType { get; set; }
        public string RegistrationToken { get; set; }
        public string CommercialRegistrationNo { get; set; }
        public string CommercialRegistrationImage { get; set; }


        public string IqamaNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string DateOfBirthString { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool Saudi { get; set; }
        public bool IgnoreYakeen { get; set; }

    }



    public class CreateStoreClientInput
    {
        public string Lang { get; set; }
        public string Name { get; set; }
        //[Required]
        public string UserName { get; set; }
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
        public Gender? Gender { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        public DeviceType DeviceType { get; set; }
        public string RegistrationToken { get; set; }
        public string CommercialRegistrationNo { get; set; }
        public string CommercialRegistrationImage { get; set; }

        public int? CityId { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Whatsapp { get; set; }
        public string Instagram { get; set; }
        public string IqamaNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string DateOfBirthString { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool Saudi { get; set; }
        public bool IgnoreYakeen { get; set; }

    }


    public class CheckFormInput
    {
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class CheckFormOutput
    {
        public string EmailError { get; set; }
        public string PhoneNumberError { get; set; }
    }



    public class LoginInput
    {
        public string Lang { get; set; }
        [Required]
        public string UserName { get; set; }

       
        public string AppVersion { get; set; }

        [Required]
        public string Password { get; set; }
        public string RegistrationToken { get; set; }
        public DeviceType DeviceType { get; set; }

        public UserTypes? UserType { get; set; }
    }



    public class LoginOutput
    {
        public string AccessToken { get; set; }
        public string ConfirmationCode { get; set; }
        public string Code { get; set; }
        public long? UserId { get; set; }
        public long? CompanyId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
        public UserTypes UserType { get; set; }
        public long? BranchId { get; set; }
        public long? ProviderId { get; set; }
        public long? WorkerId { get; set; }
        public long? DriverId { get; set; }
        public string Avatar { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }

        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }

        public List<ApiFuelPumpDto> FuelPumps { get; set; }
        public bool ForceUpdate { get; set; }

    }


    public class UserPendingAdvertismentsOutput
    {

        public bool HasPendingAd { get; set; }
        public bool AdOwner { get; set; }
        public long? Id { get; set; }
        public string NotifyMsg { get; set; }
    }


    public class ConfirmRegisterationInput
    {
        public string Lang { get; set; }
        public long UserId { get; set; }
        public string ConfirmationCode { get; set; }
        public string RegistrationToken { get; set; }
        public DeviceType DeviceType { get; set; }
    }

    public class ResendConfirmationCodeInput
    {
        public string Lang { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
    }


    public class RegistrationTokenInput
    {
        public string RegistrationToken { get; set; }
        public DeviceType DeviceType { get; set; }
    }


    public class ChangePasswordInput
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        public string Lang { get; set; }
    }

    public class CreatePasswordINInput
    {
        public string Password { get; set; }
        public long UserId { get; set; }
        public string PhoneNumber { get; set; }
    }


    public class AboutOutput
    {
        public string Description { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ContactsOutput
    {
        public string Whatsapp { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Telegram { get; set; }
        public string Youtube { get; set; }
        public string Snapchat { get; set; }
        public string Linkedin { get; set; }
        public string Instagram { get; set; }
        public string Tiktok { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class CreateContactMessageOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }


    public class HeaderCountsOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NotificationCount { get; set; }
        public int CartCount { get; set; }
        public decimal CartAmount { get; set; }
        public UserTypes? UserType { get; set; }
    }
    public class GetDriverProfileOutput
    {
        public ApiDriverDto Driver { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class UpdateDriverProfileOutput
    {
        public ApiDriverDto Driver { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }


    public class UpdateProfilePicOutput
    {
        public string AvatarPath { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }


    public class GetWorkerProfileOutput
    {
        public ApiWorkerDto Worker { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class UpdateWorkerProfileOutput
    {
        public ApiWorkerDto Worker { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }


    #region Driver

    public class GetProvidersOutput
    {
        public List<ApiProviderDto> Providers { get; set; }
        public int TotalCount { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class GetAllDriverVechilesOutput
    {
        public List<SmallDriverVeichleDto> DriverVechiles { get; set; }
        public int TotalCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    #endregion

    #region Worker

    //public class GetProvidersOutput
    //{
    //    public List<ApiProviderDto> Providers { get; set; }
    //    public int TotalCount { get; set; }
    //    public string Message { get; set; }
    //    public bool Success { get; set; }
    //} 

    public class UpdateCarSimOutput
    {
        public VeichleDto Veichle { get; set; }
        public ApiVeichlePic VeichlePic { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    #endregion


    public class GetProviderDetailsOutput
    {
        public ApiProviderDto Provider { get; set; }
        public List<ApiFuelPumpDto> FuelPumps { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class GetTransactionsOutput
    {
        public List<TransactionDto> Transactions { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class MappedGetTransactionsOutput
    {
        public List<MappedTransactionDto> Transactions { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class MappedGetTransactionsOutputNotCompleted
    {
        public List<GetVeichleDetailsOutput> Transactions { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class CancelTransOutInput
    {
        public long Id { get; set; }
        public long BranchId { get; set; }
        public CancelNote? CancelNote { get; set; }
        public virtual string CancelReason { get; set; }
    }



    public class GetVeichleDetailsOutput
    {
        public ApiVeichleDto Veichle { get; set; }
        public List<ApiFuelTransOutDto> Transactions { get; set; }
        public decimal FuelPrice { get; set; }

        public DateTime? dateTime { get; set; }
        public long? fuelIntTransId { get; set; }
        
        public decimal VeichleBalanceInLitre { get; set; }
        public decimal VeichleBalanceInSar { get; set; }

        public decimal BranchWalletAmount { get; set; }

        public decimal BranchWalletAmountInLitre
        {
            get
            {
                decimal _litres = FuelPrice > 0 ? BranchWalletAmount / FuelPrice : 0;
                return _litres;
            }
        }

        public decimal BranchFuelAmount { get; set; }
        public decimal BranchCleanAmount { get; set; }
        public decimal BranchMaintainAmount { get; set; }
        public decimal BranchReserved { get; set; }

        public virtual decimal? Reserved { get; set; }
        public virtual string QrCode { get; set; }
        public decimal BranchFuelAmountInLitre
        {
            get
            {
                decimal _litres = FuelPrice > 0 ? BranchFuelAmount / FuelPrice : 0;
                return _litres;
            }
        }

        public decimal BalanceInLitre
        {
            get
            {

                decimal _litres = VeichleBalanceInLitre;

                if (GroupType == GroupType.Litre)
                    _litres = VeichleBalanceInLitre;
                else if (GroupType == GroupType.Period)
                {
                    if (PeriodConsumptionType == PeriodConsumptionType.Money)
                        _litres = FuelPrice > 0 ? VeichleBalanceInSar / FuelPrice : 0;
                    else if (PeriodConsumptionType == PeriodConsumptionType.Litre)
                        _litres = VeichleBalanceInLitre;
                }
                else if (GroupType == GroupType.Open)
                {

                    decimal _amount = BranchFuelAmount > MaximumRechargeAmount == true ? MaximumRechargeAmount : BranchFuelAmount;
                    _litres = FuelPrice > 0 ? _amount / FuelPrice : 0;

                    //_litres = FuelPrice > 0 ? BranchFuelAmount / FuelPrice : 0;

                }

                if (FuelPrice > 0)
                    return _litres > ((BranchWalletAmount ) / FuelPrice) ? ((BranchWalletAmount ) / FuelPrice) : _litres;
                else
                    return 0;
            }
        }


        public decimal BalanceInSar
        {
            get
            {
                decimal _amount = VeichleBalanceInSar;

                if (GroupType == GroupType.Litre)
                {
                    //_amount = VeichleBalanceInLitre * FuelPrice;
                    _amount = BranchFuelAmount;

                }
                else if (GroupType == GroupType.Period)
                {
                    if (PeriodConsumptionType == PeriodConsumptionType.Money)
                        _amount = VeichleBalanceInSar;
                    else if (PeriodConsumptionType == PeriodConsumptionType.Litre)
                        _amount = VeichleBalanceInLitre * FuelPrice;
                }
                else if (GroupType == GroupType.Open)
                {
                    _amount = BranchFuelAmount > MaximumRechargeAmount == true ? MaximumRechargeAmount : BranchFuelAmount;
                }
                return _amount;
            }
        }

        public decimal MaintainBalanceAmount { get { return Veichle != null ? Veichle.Maintain_Balance : 0; } }
        public decimal WashBalanceAmount { get { return Veichle != null ? Veichle.Wash_Balance : 0; } }

        public GroupType GroupType { get; set; }
        public PeriodConsumptionType PeriodConsumptionType { get; set; }
        public decimal MaximumRechargeAmount { get; set; }
        public bool CounterPicIsRequired { get; set; }

        public bool Success { get; set; }
        public bool NotFound { get; set; }
        public bool NotFoundSim { get; set; }
        public string Message { get; set; }
        public string FuelColor { get; set; }
    }




    public class GetClientSearchOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalCount { get; set; }

    }


    public class CreateTransactionOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal Reserved { get; set; }
        public long Id { get; set; }
    }




    public class GetVeichleOutput
    {
        public ApiVeichleDto Veichle { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class GetCompaniesOutput
    {
        public List<CompanyNameDto> Companies { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class GetCompanyOutput
    {
        public CompanyDto Companies { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class GetBranchsOutput
    {
        public List<BranchNameDto> Branchs { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class _LinkByChipsEmployeeOutput
    {
        public LinkByChipsEmployeeOutput Chips { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

}
