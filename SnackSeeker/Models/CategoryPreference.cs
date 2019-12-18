using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnackSeeker.Models
{
	public class CategoryPreference
	{
		public List<Preferences> PreferenceModel { get; set; }
		public List<String> HighCategory { get; set; }

		public CategoryPreference(List<Preferences> preferenceModel, List<string> highCategory)
		{
			PreferenceModel = preferenceModel;
			HighCategory = highCategory;
		}

	}
}
