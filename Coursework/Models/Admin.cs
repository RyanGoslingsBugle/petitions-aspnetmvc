using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Coursework.Models
{
    public class Admin : User
    {
        public string role { get; set; }
    }
}