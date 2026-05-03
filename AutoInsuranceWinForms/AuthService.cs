using System.Configuration;

namespace AutoInsuranceWinForms
{
    public static class AuthService
    {
        public static UserAccount Authenticate(string email, string password)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            password = (password ?? string.Empty).Trim();
            if (email.Length == 0) return null;
            if (password.Length == 0) return null;

            if ((email == Read("SeniorAgentEmail") && password == ReadRaw("SeniorAgentPassword"))
                || (email == Read("AdminEmail") && password == ReadRaw("AdminPassword")))
                return new UserAccount { Email = email, FullName = "Старший агент", Role = UserRole.SeniorAgent };
            if (email == Read("HeadEmail") && password == ReadRaw("HeadPassword"))
                return new UserAccount { Email = email, FullName = "Руководитель отдела", Role = UserRole.DepartmentHead };
            if (email == Read("ManagerEmail") && password == ReadRaw("ManagerPassword"))
                return new UserAccount { Email = email, FullName = "Менеджер по страхованию", Role = UserRole.Manager };
            if ((email == Read("InsuranceAgentEmail") && password == ReadRaw("InsuranceAgentPassword"))
                || (email == Read("AdjusterEmail") && password == ReadRaw("AdjusterPassword")))
                return new UserAccount { Email = email, FullName = "Страховой агент", Role = UserRole.InsuranceAgent };

            return null;
        }

        private static string Read(string key)
        {
            return (ConfigurationManager.AppSettings[key] ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string ReadRaw(string key)
        {
            return (ConfigurationManager.AppSettings[key] ?? string.Empty).Trim();
        }
    }
}
