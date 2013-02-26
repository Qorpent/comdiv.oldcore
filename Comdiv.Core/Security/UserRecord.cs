using Comdiv.Extensions;

namespace Comdiv.Security {
    public class UserRecord : IUserRecord {
        public UserRecord() {
        }

        public string DomainId { get; set; }
        public UserRecord(string username) {
            var n = new UserName(username);
            this.Login = username;
            this.Name = n.Name;
            this.Domain = n.Domain;
            this.Occupation = "неизвестно";
            this.Contact = "неизвестно";
            this.Tag = "/stub:/";
        }
        private const string fiopattern = @"^(\w+)\s+(\w+)\s+(\w+)$";
        public string GetFio()
        {
            if (Name.noContent()) return Login;
            if (Name.like(fiopattern))
            {
                return Name.replace(fiopattern, m => "{0} {1}.{2}."._format(m.Groups[1].Value,
                                                                            m.Groups[2].Value.Substring(0, 1).ToUpper(),
                                                                            m.Groups[3].Value.Substring(0, 1).ToUpper()));
            }
            return Name;
        }

        public string Login { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Occupation { get; set; }
        public string Contact { get; set; }
        public string Tag { get; set; }
    }
}