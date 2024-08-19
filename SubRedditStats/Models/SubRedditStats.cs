using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRedditStats.Models
{
    public class SubRadditStats
    {
        public string Author { get; set; }
        
        public int PostCount { get; set; }

        public Post Post { get; set; }
    }
}
