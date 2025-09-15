using prjetax.Models;

namespace prjetax.Helpers
{
    public static class EnterpriseMapper
    {
        public static EnterpriseDemo ToEntity(this ExcelEnterpriseRow src)
        {
            return new EnterpriseDemo
            {
                TaxAgencyCode = src.TaxAgencyCode,
                TaxAgencyName = src.TaxAgencyName,
                TaxPayer = src.TaxPayer,
                TaxCode = src.TaxCode,
                TaxPayerName = src.TaxPayerName,
                ManagementDept = src.ManagementDept,
                ManagerName = src.ManagerName,
                MainBusinessCode = src.MainBusinessCode,
                MainBusinessName = src.MainBusinessName,
                BusinessDescription = src.BusinessDescription,
                DirectorName = src.DirectorName,
                DirectorPhone = src.DirectorPhone,
                ChiefAccountant = src.ChiefAccountant,
                ChiefAccountantPhone = src.ChiefAccountantPhone,
                DocumentNumber = src.DocumentNumber,
                DocumentDate = src.DocumentDate,
                DocumentType = src.DocumentType,
                Email = src.Email,
                ManagerId = src.ManagerId,
                Status = src.Status,
                Notes = src.Notes
            };
        }
    }
}
