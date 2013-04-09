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

using Java.Awt;
using Org.Jfree.Chart;
using Org.Jfree.Chart.Plot.Dial;
using Org.Jfree.UI;
using RomRaider.Logger.Ecu.Definition;
using RomRaider.Logger.Ecu.UI.Handler.Dash;
using Sharpen;

namespace RomRaider.Logger.Ecu.UI.Handler.Dash
{
	public sealed class SmallDialGaugeStyle : DialGaugeStyle
	{
		public SmallDialGaugeStyle(LoggerData loggerData) : base(loggerData)
		{
		}

		protected internal override Dimension GetChartSize()
		{
			return new Dimension(170, 190);
		}

		protected internal override JFreeChart BuildChart()
		{
			DialPlot plot = new DialPlot();
			plot.SetView(0.0, 0.0, 1.0, 1.0);
			plot.SetDataset(0, current);
			plot.SetDataset(1, max);
			plot.SetDataset(2, min);
			DialFrame dialFrame = new StandardDialFrame();
			plot.SetDialFrame(dialFrame);
			GradientPaint gp = new GradientPaint(new Point(), new Color(255, 255, 255), new Point
				(), new Color(170, 170, 220));
			DialBackground db = new DialBackground(gp);
			db.SetGradientPaintTransformer(new StandardGradientPaintTransformer(GradientPaintTransformType
				.VERTICAL));
			plot.SetBackground(db);
			unitsLabel.SetFont(new Font(Font.DIALOG, Font.BOLD, 14));
			unitsLabel.SetRadius(0.7);
			unitsLabel.SetLabel(loggerData.GetSelectedConvertor().GetUnits());
			plot.AddLayer(unitsLabel);
			DecimalFormat format = new DecimalFormat(loggerData.GetSelectedConvertor().GetFormat
				());
			DialValueIndicator dvi = new DialValueIndicator(0);
			dvi.SetNumberFormat(format);
			plot.AddLayer(dvi);
			EcuDataConvertor convertor = loggerData.GetSelectedConvertor();
			GaugeMinMax minMax = convertor.GetGaugeMinMax();
			StandardDialScale scale = new StandardDialScale(minMax.min, minMax.max, 225.0, -270.0
				, minMax.step, 5);
			scale.SetTickRadius(0.88);
			scale.SetTickLabelOffset(0.15);
			scale.SetTickLabelFont(new Font(Font.DIALOG, Font.PLAIN, 12));
			scale.SetTickLabelFormatter(format);
			plot.AddScale(0, scale);
			plot.AddScale(1, scale);
			plot.AddScale(2, scale);
			StandardDialRange range = new StandardDialRange(RangeLimit(minMax, 0.75), minMax.
				max, Color.RED);
			range.SetInnerRadius(0.52);
			range.SetOuterRadius(0.55);
			plot.AddLayer(range);
			StandardDialRange range2 = new StandardDialRange(RangeLimit(minMax, 0.5), RangeLimit
				(minMax, 0.75), Color.ORANGE);
			range2.SetInnerRadius(0.52);
			range2.SetOuterRadius(0.55);
			plot.AddLayer(range2);
			StandardDialRange range3 = new StandardDialRange(minMax.min, RangeLimit(minMax, 0.5
				), Color.GREEN);
			range3.SetInnerRadius(0.52);
			range3.SetOuterRadius(0.55);
			plot.AddLayer(range3);
			DialPointer needleCurrent = new DialPointer.Pointer(0);
			plot.AddLayer(needleCurrent);
			DialPointer needleMax = new DialPointer.Pin(1);
			needleMax.SetRadius(0.84);
			((DialPointer.Pin)needleMax).SetPaint(Color.RED);
			((DialPointer.Pin)needleMax).SetStroke(new BasicStroke(1.5F));
			plot.AddLayer(needleMax);
			DialPointer needleMin = new DialPointer.Pin(2);
			needleMin.SetRadius(0.84);
			((DialPointer.Pin)needleMin).SetPaint(Color.BLUE);
			((DialPointer.Pin)needleMin).SetStroke(new BasicStroke(1.5F));
			plot.AddLayer(needleMin);
			DialCap cap = new DialCap();
			cap.SetRadius(0.10);
			plot.SetCap(cap);
			JFreeChart chart = new JFreeChart(plot);
			chart.SetTitle(loggerData.GetName());
			return chart;
		}

		private double RangeLimit(GaugeMinMax minMax, double fraction)
		{
			return minMax.min + (minMax.max - minMax.min) * fraction;
		}
	}
}
