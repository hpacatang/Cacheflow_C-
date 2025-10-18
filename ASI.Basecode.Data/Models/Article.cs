using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public int? Views { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Body { get; set; }
    }
}
