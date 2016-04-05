using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRMSupportForce.Models
{
    // Drop Down Support
    public class MaxRecordType
    {
        public MaxRecordType(int maxrecords, String description)
        {
            MaxRecords = maxrecords;
            Description = description;
        }

        public int MaxRecords { get; set; }
        public String Description { get; set; }
    }

    public class OrderByColumns
    {
        public OrderByColumns(String clause, String description)
        {
            Clause = clause;
            Description = description;
        }

        public String Clause { get; set; }
        public String Description { get; set; }
    }

}
