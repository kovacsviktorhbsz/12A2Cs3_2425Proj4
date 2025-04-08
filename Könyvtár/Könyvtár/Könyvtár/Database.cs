using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Könyvtár
{
    public class Database
    {
        public List<string> TableNames;
        public Dictionary<string,Table> Tables = new Dictionary<string, Table>();
        public string Name;
        public string connection;

        public Database(string name) 
        {
            Name = name;
            connection = $"server=localhost;port=3306;uid=root;pwd=;database={this.Name}";
        }
    }
}
