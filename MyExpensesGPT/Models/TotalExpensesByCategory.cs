using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExpensesGPT.Models
{
    internal class TotalExpensesByCategory
    {
        public int CategoryId { get; set; } = 0;
        public double TotalAmount { get; set; } = 0;
    }
}
