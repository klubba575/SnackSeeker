using System;
using System.Collections.Generic;

namespace SnackSeeker
{
    public partial class Search
    {
        public int SearchId { get; set; }
        public string UserId { get; set; }
        public string CategoryAlias { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
