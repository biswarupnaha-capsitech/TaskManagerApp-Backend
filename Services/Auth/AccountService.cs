using Capsitech.Data.MongoDB;
using Projects.Common;
using Projects.Models;

namespace Projects.Services.Auth
{
    public class AccountService : ApiControllerBase
    {
        private readonly ApplicationUserDB _accountsCollection;
        //private readonly PasswordService _passwordService;

        public AccountService(DBConfiguration dbconfig) : base(dbconfig)
        {
            _accountsCollection = new ApplicationUserDB(_dbConfig, User);
        }

    }
}
