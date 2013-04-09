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

using RomRaider.Logger.Ecu.UI.Handler.Dash;
using RomRaider.Logger.External.Core;
using RomRaider.Logger.External.Zt2.Plugin;
using Sharpen;

namespace RomRaider.Logger.External.Zt2.Plugin
{
	public class ZT2SensorConversions
	{
		public static readonly ExternalSensorConversion LAMBDA = new ExternalSensorConversion
			("Lambda", "x*0.00680272108843537", "0.00", new GaugeMinMax(0.6, 1.4, 0.08));

		public static readonly ExternalSensorConversion AFR_147 = new ExternalSensorConversion
			("AFR Gasoline", "x*0.1", "0.00", new GaugeMinMax(9, 20, 1));

		public static readonly ExternalSensorConversion AFR_90 = new ExternalSensorConversion
			("AFR Ethonal", "x*0.06122448979591837", "0.00", new GaugeMinMax(5, 12, 1));

		public static readonly ExternalSensorConversion AFR_146 = new ExternalSensorConversion
			("AFR Diesel", "x*0.09931972789115646", "0.00", new GaugeMinMax(9, 20, 1));

		public static readonly ExternalSensorConversion AFR_64 = new ExternalSensorConversion
			("AFR Methonal", "x*0.04353741496598639", "0.00", new GaugeMinMax(4, 9, 1));

		public static readonly ExternalSensorConversion AFR_155 = new ExternalSensorConversion
			("AFR LPG", "x*0.1054421768707483", "0.00", new GaugeMinMax(9, 20, 1));

		public static readonly ExternalSensorConversion AFR_172 = new ExternalSensorConversion
			("AFR CNG", "x*0.1170068027210884", "0.00", new GaugeMinMax(9, 20, 1));

		public static readonly ExternalSensorConversion AFR_34 = new ExternalSensorConversion
			("AFR Hydrogen", "x*0.2312925170068027", "0.00", new GaugeMinMax(20, 46, 2.5));

		public static readonly ExternalSensorConversion BOOST_PSI = new ExternalSensorConversion
			("psi", "x*0.1", "0.00", new GaugeMinMax(-10, 30, 5));

		public static readonly ExternalSensorConversion BOOST_BAR = new ExternalSensorConversion
			("bar", "x*0.0068947573", "0.00", new GaugeMinMax(-0.5, 4, 0.5));

		public static readonly ExternalSensorConversion BOOST_KPA = new ExternalSensorConversion
			("kPa", "x*0.6894757282", "0.0", new GaugeMinMax(98, 120, 2));

		public static readonly ExternalSensorConversion BOOST_KGCM2 = new ExternalSensorConversion
			("kg/cm^2", "x*0.0070306958", "0.00", new GaugeMinMax(-0.5, 4, 0.5));

		public static readonly ExternalSensorConversion RPM = new ExternalSensorConversion
			("rpm", "round(((1000000/x)*4.59)/2)", "0", new GaugeMinMax(0, 10000, 1000));
		// AFR conversion assumes reported DATA value is Gas AFR
		// gasoline
		// ethanol
		// diesel
		// methanol
		// LPG
		// CNG
		// Hydrogen
		// converts from PSI
		// converts from PSI
		// converts from PSI
	}
}
