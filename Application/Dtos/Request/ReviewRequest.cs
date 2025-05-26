using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class ReviewRequest
    {
        public int Rating { get; set; }            // 1–5
        public string? Comment { get; set; }       
    }
}
