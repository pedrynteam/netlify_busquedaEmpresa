using System;
using System.Collections.Generic;
using System.Text;

namespace MGTModel.Helpers.DataAnnotationsValidator
{
	public static class ObjectExtensions
	{
		public static object GetPropertyValue(this object o, string propertyName)
		{
			object objValue = string.Empty;

			var propertyInfo = o.GetType().GetProperty(propertyName);
			if (propertyInfo != null)
				objValue = propertyInfo.GetValue(o, null);

			return objValue;
		}
	}
}
