using KPMG3CS.DomainModel.DbContexts;
using KPMG3CS.DomainModel.Enums;
using KPMG3CS.DomainModel.Models.AdminPanel;
using KPMG3CS.DomainModel.Models.Masters;
using KPMG3CS.Utility.Constants;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KPMG3CS.Repository.SeedData   
{
    public static class SeedData
    {
        /// <summary>
        /// Seed Initial data to database
        /// </summary>
        /// <param name="_dbContext">Instance of database constext</param>
        public static void Initialize(KpmgDbContext _dbContext)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    #region Tenant
                    int existingTenant = _dbContext.Tenant.Count();

                    Tenant tenant = new Tenant();
                    tenant.Id = new Guid();
                    tenant.CreatedDateTime = DateTime.UtcNow;
                    tenant.Name = "DemoTenant";
                    tenant.StartDate = DateTime.UtcNow;
                    tenant.EndDate = DateTime.UtcNow;

                    if (existingTenant == 0)
                    {
                        _dbContext.Tenant.Add(tenant);
                        _dbContext.SaveChanges();

                    }
                    #endregion

                    Guid tenantId = _dbContext.Tenant.First().Id;

                    #region Company
                    int existingCompany = _dbContext.Company.Count();

                    Company company = new Company();
                    company.TenantId = tenantId;
                    company.Id = new Guid();
                    company.Name = "DemoCompany";
                    company.CreatedDateTime = DateTime.UtcNow;

                    if (existingCompany == 0)
                    {
                        _dbContext.Company.Add(company);
                        _dbContext.SaveChanges();
                    }
                    #endregion

                    #region System Role
                    int existingRoleCount = _dbContext.SystemRole.Count();

                    List<SystemRole> systemRoleList = new List<SystemRole>()
                    {
                        new SystemRole
                        {
                            TenantId=tenantId,
                            Name = StringConstant.KPMGAdminText,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new SystemRole
                        {
                            TenantId=tenantId,
                            Name =StringConstant.TenantAdminText,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new SystemRole
                        {
                            TenantId=tenantId,
                            Name =StringConstant.TenantUserText,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new SystemRole
                        {
                            TenantId=tenantId,
                            Name = StringConstant.ContractorText,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new SystemRole
                        {
                            TenantId=tenantId,
                            Name = StringConstant.WorkforceText,
                            CreatedDateTime = DateTime.UtcNow,
                        }
                    };

                    if (existingRoleCount == 0)
                    {
                        _dbContext.SystemRole.AddRange(systemRoleList);
                        _dbContext.SaveChanges();

                    }
                    #endregion

                    #region User 
                    int existingUserCount = _dbContext.ApplicationUser.Count();

                    SystemRole systemRole = _dbContext.SystemRole.FirstOrDefault(x => !x.IsDeleted && x.Name == StringConstant.KPMGAdminText);
                    Guid companyId = _dbContext.Company.First().Id;
                    ApplicationUser userDetail = new ApplicationUser();
                    userDetail.TenantId = tenantId;
                    userDetail.CompanyId = companyId;
                    userDetail.Name = StringConstant.KPMGAdminText;
                    userDetail.Email = "KpmgAdmin@gmail.com";
                    userDetail.Password = "Passw@rd";
                    userDetail.RoleId = systemRole.Id;
                    userDetail.UserType = UserType.KPMGAdmin;
                    userDetail.Status = UserStatus.Inital;
                    userDetail.IsDeleted = false;
                    userDetail.CreatedDateTime = DateTime.UtcNow;

                    //If no db data present then add all
                    if (existingUserCount == 0)
                    {
                        _dbContext.ApplicationUser.Add(userDetail);
                        _dbContext.SaveChanges();
                    }
                    #endregion

                    #region Access Permission 
                    int existingRoleWisePermissionSettingsCount = _dbContext.RoleWisePermissionSettings.Count();

                    List<SystemRole> systemRolesList = _dbContext.SystemRole.Where(x => !x.IsDeleted).ToList();
                    Guid systemRoleId = systemRolesList.First(a => a.Name == StringConstant.KPMGAdminText).Id;

                    List<RoleWisePermissionSettings> kpmgAdminPermissionList = SetKPMGAdminRoleWisePermissionSettings(systemRoleId);
                    List<RoleWisePermissionSettings> tanentAdminPermissionList = SetTanentAdminRoleWisePermissionSettings(systemRoleId);
                    List<RoleWisePermissionSettings> tanentUserPermissionList = SetTanentUserRoleWisePermissionSettings(systemRoleId);
                    List<RoleWisePermissionSettings> contractorPermissionList = SetContractorRoleWisePermissionSettings(systemRoleId);
                    List<RoleWisePermissionSettings> workForceRoleWisePermissionSettingsList = SetWorkForceRoleWisePermissionSettings(systemRoleId);

                    //If no db data present then add all
                    if (existingRoleWisePermissionSettingsCount == 0)
                    {
                        _dbContext.RoleWisePermissionSettings.AddRange(kpmgAdminPermissionList);
                        _dbContext.RoleWisePermissionSettings.AddRange(tanentAdminPermissionList);
                        _dbContext.RoleWisePermissionSettings.AddRange(tanentUserPermissionList);
                        _dbContext.RoleWisePermissionSettings.AddRange(contractorPermissionList);
                        _dbContext.RoleWisePermissionSettings.AddRange(workForceRoleWisePermissionSettingsList);
                        _dbContext.SaveChanges();
                    }
                    #endregion

                    _dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #region Private Methods
        /// <summary>
        /// Set KPMG admin access permission
        /// </summary>
        /// <param name="roleId">system role id</param>
        /// <returns>List of access permissions</returns>
        private static List<RoleWisePermissionSettings> SetKPMGAdminRoleWisePermissionSettings(Guid roleId)
        {
            List<RoleWisePermissionSettings> RoleWisePermissionSettingss = new List<RoleWisePermissionSettings>()
            {
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ParentContractorText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.CountryText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ProvinceTerritoriesText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.LocalJurisdictionText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.DepartmentText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.DesignationText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ShiftText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.HolidayText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.DeviceText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.MinimumWagesText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.WorkPayPolicyText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.PaymentModeText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                //Module name
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.BudgetingSourcingText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ContractorRegistrationText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.WorkOrderText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ContingentWorkForceText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.AdvancesText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.LeaveManagementText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ComplianceManagementText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.FinesPenaltiesText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ConstructionActivityText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ReportingAccidentsText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.HealthRegisterText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.DailyAttendanceText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.TaxDeductedSourceText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.BonusPayableText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },

				//Report modules
				new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.MonthlyAttendanceText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.RegisterText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.WageRegisterText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.PaySlipText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ContractorInvoicePaymentText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.OtherReportsText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
				//Statutory reports
				new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.FormCText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.BOCWAForm1Text,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.BOCWAForm2Text,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.BOCWAForm4Text,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.BOCWAForm28Text,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.BOCWAForm29Text,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.BOCWAForm77Text,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.ESICFormText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.LabourWelfareFormText,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName=StringConstant.CodeOfWagesForm1Text,
                    RoleId=roleId,
                    Access= true,
                    View= true,
                    Edit= true,
                    Delete= true,
                    Import= true,
                    Export= true,
                    CreatedDateTime = DateTime.UtcNow,
                }
            };
            return RoleWisePermissionSettingss;
        }

        /// <summary>
        /// Set Tanent admin access permission
        /// </summary>
        /// <param name="roleId">system role id</param>
        /// <returns>List of access permissions</returns>
        private static List<RoleWisePermissionSettings> SetTanentAdminRoleWisePermissionSettings(Guid roleId)
        {
            List<RoleWisePermissionSettings> RoleWisePermissionSettingss = new List<RoleWisePermissionSettings>()
                    {
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ParentContractorText,
                RoleId = roleId,
                Access = true,
                View = true,
                Edit = true,
                Delete = true,
                Import = true,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.CountryText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ProvinceTerritoriesText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.LocalJurisdictionText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.DepartmentText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.DesignationText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ShiftText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.HolidayText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.DeviceText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.MinimumWagesText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.WorkPayPolicyText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.PaymentModeText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        //Module name
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.BudgetingSourcingText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ContractorRegistrationText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.WorkOrderText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ContingentWorkForceText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.AdvancesText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.LeaveManagementText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ComplianceManagementText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.FinesPenaltiesText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ConstructionActivityText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ReportingAccidentsText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.HealthRegisterText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.DailyAttendanceText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.TaxDeductedSourceText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.BonusPayableText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },

					    //Report modules
					    new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.MonthlyAttendanceText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.RegisterText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.WageRegisterText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.PaySlipText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ContractorInvoicePaymentText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.OtherReportsText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
					    //Statutory reports
					    new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.FormCText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.BOCWAForm1Text,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.BOCWAForm2Text,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.BOCWAForm4Text,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.BOCWAForm28Text,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.BOCWAForm29Text,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.BOCWAForm77Text,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.ESICFormText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.LabourWelfareFormText,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        },
                        new RoleWisePermissionSettings
                        {
                            ModuleName = StringConstant.CodeOfWagesForm1Text,
                            RoleId = roleId,
                            Access = true,
                            View = true,
                            Edit = true,
                            Delete = true,
                            Import = true,
                            Export = true,
                            CreatedDateTime = DateTime.UtcNow,
                        }
            };
            return RoleWisePermissionSettingss;
        }

        /// <summary>
        /// Set tanent user access permission
        /// </summary>
        /// <param name="roleId">system role id</param>
        /// <returns>List of access permissions</returns>
        private static List<RoleWisePermissionSettings> SetTanentUserRoleWisePermissionSettings(Guid roleId)
        {
            List<RoleWisePermissionSettings> RoleWisePermissionSettingss = new List<RoleWisePermissionSettings>()
                    {
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ParentContractorText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.CountryText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ProvinceTerritoriesText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.LocalJurisdictionText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.DepartmentText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.DesignationText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ShiftText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.HolidayText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.DeviceText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.MinimumWagesText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.WorkPayPolicyText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.PaymentModeText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = false,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
	                        //Module name
	                        new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.BudgetingSourcingText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ContractorRegistrationText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.WorkOrderText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ContingentWorkForceText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.AdvancesText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.LeaveManagementText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ComplianceManagementText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.FinesPenaltiesText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ConstructionActivityText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ReportingAccidentsText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.HealthRegisterText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.DailyAttendanceText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.TaxDeductedSourceText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.BonusPayableText,
                                RoleId = roleId,
                                Access = true,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = false,
                                CreatedDateTime = DateTime.UtcNow,
                            },

	                        //Report modules
	                        new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.MonthlyAttendanceText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.RegisterText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.WageRegisterText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.PaySlipText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ContractorInvoicePaymentText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.OtherReportsText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
	                        //Statutory reports
	                        new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.FormCText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.BOCWAForm1Text,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.BOCWAForm2Text,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.BOCWAForm4Text,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.BOCWAForm28Text,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.BOCWAForm29Text,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.BOCWAForm77Text,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.ESICFormText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.LabourWelfareFormText,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            },
                            new RoleWisePermissionSettings
                            {
                                ModuleName = StringConstant.CodeOfWagesForm1Text,
                                RoleId = roleId,
                                Access = false,
                                View = true,
                                Edit = true,
                                Delete = false,
                                Import = false,
                                Export = true,
                                CreatedDateTime = DateTime.UtcNow,
                            }
            };
            return RoleWisePermissionSettingss;
        }

        /// <summary>
        /// Set contractor access permission
        /// </summary>
        /// <param name="roleId">system role id</param>
        /// <returns>List of access permissions</returns>
        private static List<RoleWisePermissionSettings> SetContractorRoleWisePermissionSettings(Guid roleId)
        {
            List<RoleWisePermissionSettings> RoleWisePermissionSettingss = new List<RoleWisePermissionSettings>()
                    {
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ParentContractorText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.CountryText,
                RoleId = roleId,
                Access = false,
                View = false,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ProvinceTerritoriesText,
                RoleId = roleId,
                Access = false,
                View = false,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.LocalJurisdictionText,
                RoleId = roleId,
               Access = false,
                View = false,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.DepartmentText,
                RoleId = roleId,
                Access = false,
                View = false,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.DesignationText,
                RoleId = roleId,
                Access = false,
                View = false,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ShiftText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.HolidayText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.DeviceText,
                RoleId = roleId,
               Access = false,
                View = false,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.MinimumWagesText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.WorkPayPolicyText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.PaymentModeText,
                RoleId = roleId,
                Access = false,
                View = false,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
	        //Module name
	        new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.BudgetingSourcingText,
                RoleId = roleId,
                Access = false,
                View = false,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ContractorRegistrationText,
                RoleId = roleId,
                Access = true,
                View = true,
                Edit = true,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.WorkOrderText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ContingentWorkForceText,
                RoleId = roleId,
                Access = true,
                View = true,
                Edit = true,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.AdvancesText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.LeaveManagementText,
                RoleId = roleId,
                Access = true,
                View = true,
                Edit = true,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ComplianceManagementText,
                RoleId = roleId,
                Access = true,
                View = true,
                Edit = true,
               Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.FinesPenaltiesText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ConstructionActivityText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                 Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ReportingAccidentsText,
                RoleId = roleId,
                Access = true,
                View = true,
                Edit = true,
                Delete = true,
                Import = true,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.HealthRegisterText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.DailyAttendanceText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.TaxDeductedSourceText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.BonusPayableText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = false,
                CreatedDateTime = DateTime.UtcNow,
            },

	        //Report modules
	        new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.MonthlyAttendanceText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.RegisterText,
                RoleId = roleId,
               Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.WageRegisterText,
                RoleId = roleId,
               Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.PaySlipText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ContractorInvoicePaymentText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.OtherReportsText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
	        //Statutory reports
	        new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.FormCText,
                RoleId = roleId,
                 Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.BOCWAForm1Text,
                RoleId = roleId,
              Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.BOCWAForm2Text,
                RoleId = roleId,
               Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.BOCWAForm4Text,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.BOCWAForm28Text,
                RoleId = roleId,
              Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.BOCWAForm29Text,
                RoleId = roleId,
              Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,

                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.BOCWAForm77Text,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.ESICFormText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.LabourWelfareFormText,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            },
            new RoleWisePermissionSettings
            {
                ModuleName = StringConstant.CodeOfWagesForm1Text,
                RoleId = roleId,
                Access = false,
                View = true,
                Edit = false,
                Delete = false,
                Import = false,
                Export = true,
                CreatedDateTime = DateTime.UtcNow,
            }
            };
            return RoleWisePermissionSettingss;
        }

        /// <summary>
        /// Set work force access permission
        /// </summary>
        /// <param name="roleId">system role id</param>
        /// <returns>List of access permissions</returns>
        private static List<RoleWisePermissionSettings> SetWorkForceRoleWisePermissionSettings(Guid roleId)
        {
            List<RoleWisePermissionSettings> RoleWisePermissionSettingss = new List<RoleWisePermissionSettings>()
            {
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.WorkforceText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.CountryText,
                    RoleId = roleId,
                    Access = false,
                    View = false,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ProvinceTerritoriesText,
                    RoleId = roleId,
                   Access = false,
                    View = false,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.LocalJurisdictionText,
                    RoleId = roleId,
                   Access = false,
                    View = false,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.DepartmentText,
                    RoleId = roleId,
                   Access = false,
                    View = false,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.DesignationText,
                    RoleId = roleId,
                    Access = false,
                    View = false,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ShiftText,
                    RoleId = roleId,
                   Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.HolidayText,
                    RoleId = roleId,
                   Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.DeviceText,
                    RoleId = roleId,
                   Access = false,
                    View = false,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.MinimumWagesText,
                    RoleId = roleId,
                   Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.WorkPayPolicyText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.PaymentModeText,
                    RoleId = roleId,
                   Access = false,
                    View = false,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
	            //Module name
	            new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.BudgetingSourcingText,
                    RoleId = roleId,
                     Access = false,
                    View = false,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ContractorRegistrationText,
                    RoleId = roleId,
                     Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.WorkOrderText,
                    RoleId = roleId,
                  Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ContingentWorkForceText,
                    RoleId = roleId,
                    Access = true,
                    View = true,
                    Edit = true,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.AdvancesText,
                    RoleId = roleId,
                     Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.LeaveManagementText,
                    RoleId = roleId,
                    Access = true,
                    View = true,
                    Edit = true,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ComplianceManagementText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.FinesPenaltiesText,
                    RoleId = roleId,
                     Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ConstructionActivityText,
                    RoleId = roleId,
                      Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ReportingAccidentsText,
                    RoleId = roleId,
                    Access = true,
                    View = true,
                    Edit = true,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.HealthRegisterText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.DailyAttendanceText,
                    RoleId = roleId,
                     Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.TaxDeductedSourceText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.BonusPayableText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = false,
                    CreatedDateTime = DateTime.UtcNow,
                },

	            //Report modules
	            new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.MonthlyAttendanceText,
                    RoleId = roleId,
                  Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.RegisterText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.WageRegisterText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.PaySlipText,
                    RoleId = roleId,
                 Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ContractorInvoicePaymentText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.OtherReportsText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
	            //Statutory reports
	            new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.FormCText,
                    RoleId = roleId,
                   Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.BOCWAForm1Text,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.BOCWAForm2Text,
                    RoleId = roleId,
                   Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.BOCWAForm4Text,
                    RoleId = roleId,
                   Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.BOCWAForm28Text,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.BOCWAForm29Text,
                    RoleId = roleId,
                  Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.BOCWAForm77Text,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.ESICFormText,
                    RoleId = roleId,
                     Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.LabourWelfareFormText,
                    RoleId = roleId,
                    Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                },
                new RoleWisePermissionSettings
                {
                    ModuleName = StringConstant.CodeOfWagesForm1Text,
                    RoleId = roleId,
                     Access = false,
                    View = true,
                    Edit = false,
                    Delete = false,
                    Import = false,
                    Export = true,
                    CreatedDateTime = DateTime.UtcNow,
                }
            };
            return RoleWisePermissionSettingss;
        }

        #endregion
    }
}
