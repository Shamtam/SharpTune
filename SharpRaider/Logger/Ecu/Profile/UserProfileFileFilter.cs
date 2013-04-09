/*
 * This code is derived from the Java version of RomRaider
 *
 * RomRaider Open-Source Tuning, Logging and Reflashing
 * Copyright (C) 2006-2012 RomRaider.com
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using Javax.Swing.Filechooser;
using RomRaider.Logger.Ecu.Profile;
using RomRaider.Swing;
using Sharpen;

namespace RomRaider.Logger.Ecu.Profile
{
	public sealed class UserProfileFileFilter : FileFilter
	{
		private readonly FileFilter filter = new GenericFileFilter("ECU Logger User Profiles"
			, "xml");

		public override bool Accept(FilePath file)
		{
			return filter.Accept(file);
		}

		public override string GetDescription()
		{
			return filter.GetDescription();
		}
	}
}
