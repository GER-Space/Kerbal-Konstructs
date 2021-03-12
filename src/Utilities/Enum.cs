/*
This file is part of Advanced Input.

Advanced Input is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Advanced Input is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with Advanced Input.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;

namespace KerbalKonstructs {
	public static class KK_Enum
	{
		public static bool ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue, out TEnum value)
		{
			if (!Enum.IsDefined(typeof(TEnum), strEnumValue)) {
				value = defaultValue;
				return false;
			}

			value = (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
			return true;
		}
	}
}
