using System;
using System.Collections.Generic;

namespace SnackSeeker
{
    public partial class Preferences
    {
        public int PreferenceId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public double? Rating { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
