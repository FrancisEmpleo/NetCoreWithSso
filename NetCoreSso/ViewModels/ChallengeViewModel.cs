using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreSso.ViewModels
{
    public class ChallengeViewModel
    {
        public string Scheme { get; set; }
        public string ReturnUrl { get; set; }
    }
}
