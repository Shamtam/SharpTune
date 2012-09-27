/*
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Linq;
using SharpTune.Tables;
using Merp;

namespace SharpTune
{
    /// <summary>
    /// Represents an individual device definition
    /// Includes ALL scalings from base to top
    /// </summary>
    public class Definition
    {
        public string internalId { get; set; }
        /// <summary>
        /// Contains basic info for the definition
        /// Make, Model, etc
        /// </summary>
        public Dictionary<string,string> carInfo { get; set; }
        /// <summary>
        /// Contains the file path to the XML definition (top)
        /// </summary>
        public string defPath { get; set; }
        /// <summary>
        /// Holds all XElements pulled from XML for ROM tables
        /// Includes inherited XML
        /// </summary>

        public XElement romIdData { get; set; }

        public string include { get; set; }

        public Dictionary<string, XElement> xRomTableList { get; set; }
        /// <summary>
        /// Holds all Xelements pulled form XML for RAM tables
        /// Includes inherited XML
        /// </summary>
        public Dictionary<string, XElement> xRamTableList { get; set; }

        public Dictionary<string, XElement> xScalingList { get; set; }

        /// <summary>
        /// Holds all scalings pulled from XML
        /// Doesn't store XML because there is no inheritance among scalings
        /// TODO: Rethink this?
        /// </summary>
        public List<Scaling> scalingList { get; set; }

        /// <summary>
        /// Constructor
        /// TODO: Change to a factory to share already-opened definitions
        /// TODO: Include information about inheritance in the class for def-editing
        /// </summary>
        /// <param name="calID"></param>
        public Definition(string filepath)
        {
            this.internalId = null;
            this.xRomTableList = new Dictionary<string, XElement>();
            this.xRamTableList = new Dictionary<string, XElement>();
            this.scalingList = new List<Scaling>();
            this.defPath = filepath;
        }

        /// <summary>
        /// Recursively search directory for rom definition by filename
        /// TODO: open files and read XMLID instead of filename!
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string DirectorySearch(string directory, string calid)
        {
            string filepath = null;

            try	
            {

                foreach (string d in Directory.GetDirectories(directory)) 
                {
                    string[] files = Directory.GetFiles(d);
                    foreach (string f in files)
                    {
                        if (f.Contains(calid))
                        {
                            return f;
                        }
                    }
                    filepath = DirectorySearch(d,calid);
                    if (filepath != null) return filepath;
                }

                return null;
            }
            catch (System.Exception excpt) 
            {
                Console.WriteLine(excpt.Message);
            }

            return null;
        }

        /// <summary>
        /// Load parameters from XML an XML file
        /// </summary>
        public bool ReadXML(string fetchPath, bool recurse)
        {
            if (fetchPath == null) return false;
            XDocument xmlDoc = XDocument.Load(fetchPath);
            XElement xmlEle = XElement.Load(fetchPath);

            this.romIdData = xmlDoc.XPathSelectElement("/rom/romid");
            Dictionary<string,string> tempinfo = new Dictionary<string, string>();
            string tempcalid = null;

            if (romIdData == null)  tempcalid = "template";
            else
            {
                foreach (XElement element in romIdData.Elements())
                {
                    tempinfo.Add(element.Name.ToString(), element.Value.ToString());
                }
                tempcalid = tempinfo["internalidstring"];

                if (internalId == null)
                {
                    this.carInfo = tempinfo;
                    this.internalId = tempcalid;
                }
            }
            

            // ROM table fetches here!
            var tableQuery = from t in xmlDoc.XPathSelectElements("/rom/table")
                             //where table.Ancestors("table").First().IsEmpty
                             select t;
            foreach (XElement table in tableQuery)
            {
                //skip tables with no name
                if (table.Attribute("name") == null) continue;

                string tablename = table.Attribute("name").Value.ToString();

                    if (this.xRomTableList.ContainsKey(tablename)) 
                    {
                        //table already exists
                        //add data to existing table
                        this.xRomTableList[tablename].Merge(table);
                        //Console.WriteLine("table " + tablename + " already exists, merging tables");
                    }

                    else if (!tempcalid.ContainsCI("base"))
                    {
                        //table does not exist
                        //call constructor
                        this.xRomTableList.Add(tablename, table);
                        //Console.WriteLine("added new table from " + fetchCalID + " with name " + tablename);
                    }
            }

            // RAM table feteches here!
            var ramtableQuery = from t in xmlDoc.XPathSelectElements("/ram/table")
                             //where table.Ancestors("table").First().IsEmpty
                             select t;
            foreach (XElement table in ramtableQuery)
            {
                //skip tables with no name
                if (table.Attribute("name") == null) continue;

                string tablename = table.Attribute("name").Value.ToString();

                if (this.xRomTableList.ContainsKey(tablename))
                {
                    //table already exists
                    //add data to existing table
                    this.xRamTableList[tablename].Merge(table);
                    //Console.WriteLine("table " + tablename + " already exists, merging tables");
                }

                else if (!tempcalid.ContainsCI("base"))
                {
                    //table does not exist
                    //call constructor
                    this.xRamTableList.Add(tablename, table);
                    //Console.WriteLine("added new table from " + fetchCalID + " with name " + tablename);
                }
            }

            //Read Scalings
            this.xScalingList = new Dictionary<string, XElement>();

            var scalingQuery = from sc in xmlDoc.XPathSelectElements("/rom/scaling")
                               //where table.Ancestors("table").First().IsEmpty
                               select sc;
            foreach (XElement scaling in scalingQuery)
            {
                //skip scalings with no name
                if (scaling.Attribute("name") == null) continue;
                string scalingname = scaling.Attribute("name").Value.ToString();
                this.xScalingList.Add(scalingname, scaling);

                if (this.scalingList.Exists(sc => sc.name == scalingname)) //   this.fullTableList.Exists(o => o.name == tablename))
                {
                    //scaling already exists
                    //add data to existing table
                    int index = this.scalingList.FindIndex(o => o.name == scalingname);
                    Scaling newscaling = this.scalingList.ElementAt(index);
                    this.scalingList[index] = this.scalingList.ElementAt(index).AddBase(ScalingFactory.CreateScaling(scaling));
                    //Console.WriteLine("scaling " + scalingname + " already exists, merging scalings");
                }
                else
                {
                    //scaling does not exist
                    //call constructor
                    this.scalingList.Add(ScalingFactory.CreateScaling(scaling));
                    //Console.WriteLine("added new table from " + fetchCalID + " with name " + tablename);
                }
            }

            //Recurse if not working in the BASE
            if (!tempcalid.Contains("BASE") && recurse)
            {
                string include = xmlEle.Element("include").Value.ToString();
                //Down the rabbit hole
                //Continue Inheritance
                //Recursively until you hit the BASE
                ReadXML(DirectorySearch("rommetadata", include),true);
            }

            return false;
            
        }

        /// <summary>
        /// Load parameters from XML an XML file
        /// </summary>
        public static void ConvertXML(string fetchPath)
        {
            if (fetchPath == null) return;
            XDocument xmlDoc = XDocument.Load(fetchPath);
            XElement xmlEle = XElement.Load(fetchPath);

            

            // ROM table fetches here!
            var tableQuery = from t in xmlDoc.XPathSelectElements("/rom/table")
                             //where table.Ancestors("table").First().IsEmpty
                             select t;
            foreach (XElement table in tableQuery)
            {
                //skip tables with no name
                if (table.Attribute("name") == null) continue;
                bool xaxis = false;
                bool yaxis = false;
                foreach (XElement xel in table.Descendants())
                {

                    if (xel.Name == "table")
                    {
                        if (xel.Attribute("name") != null && xel.Attribute("name").Value == "X")
                        {
                            xel.Name = "xaxis";
                            xel.Attribute("name").Remove();
                            xaxis = true;
                        }
                        else if (xel.Attribute("type") != null && xel.Attribute("type").Value.ContainsCI("x axis"))
                        {
                            xel.Name = "xaxis";
                            xel.Attribute("type").Remove();
                            xaxis = true;
                        }
                        else if (xel.Attribute("name") != null && xel.Attribute("name").Value == "Y")
                        {
                            xel.Name = "yaxis";
                            xel.Attribute("name").Remove();
                            yaxis = true;
                        }
                        else if (xel.Attribute("type") != null && xel.Attribute("type").Value.ContainsCI("y axis"))
                        {
                            xel.Name = "yaxis";
                            xel.Attribute("type").Remove();
                            yaxis = true;
                        }
                    }
                }

                if (xaxis && yaxis) table.Name = "table3d";
                else if (xaxis || yaxis) table.Name = "table2d";
                else table.Name = "table1d";
                if(table.Attribute("type") != null) table.Attribute("type").Remove();
            }
            xmlDoc.Save(fetchPath);
        }

        public void ExportXML(string filepath)
        {
            XmlWriterSettings objXmlWriterSettings = new XmlWriterSettings();
            objXmlWriterSettings.Indent = true;
            objXmlWriterSettings.OmitXmlDeclaration = false;
            using (XmlWriter writer = XmlWriter.Create(filepath, objXmlWriterSettings ))
            {
                //Start writing doc
                writer.WriteStartDocument(); 
                
                //Write romid elements
                writer.WriteStartElement("rom");
                writer.WriteStartElement("romid");
                foreach (KeyValuePair<string, string> kvp in this.carInfo)
                {
                    writer.WriteElementString(kvp.Key.ToString(), kvp.Value.ToString());
                }
                writer.WriteEndElement();

                //Write include
                writer.WriteElementString("include", this.include.ToString());

                //Write scalings

                if(this.xScalingList != null)
                {
                    foreach (KeyValuePair<string, XElement> table in this.xScalingList)
                    {
                        table.Value.WriteTo(writer);
                    }
                }
                //Write ROM tables
                if (this.xRomTableList != null)
                {
                    foreach (KeyValuePair<string, XElement> table in this.xRomTableList)
                    {
                        table.Value.WriteTo(writer);
                    }
                }

                //Write RAM tables
                if (this.xRamTableList != null)
                {
                    writer.WriteStartElement("ram");
                    foreach (KeyValuePair<string, XElement> table in this.xRamTableList)
                    {
                        table.Value.WriteTo(writer);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndDocument();
            }
        }
    }
    

    public static class XExtrensions
    {

         public static XElement Merge(this XElement table, XElement xel)
         {
             //Merge the child (table) with the inherited/base (xel)
             foreach (XAttribute newattribute in xel.Attributes())
             {
                 bool found = false;
                 foreach (XAttribute existingattribute in table.Attributes())
                 {
                     if (newattribute.Name == existingattribute.Name)
                     {
                         found = true;
                         break;
                     }
                 }
             if(found == false)
             {
                table.Add(newattribute);
             }
             }

             //merge the children
             foreach (XElement newchild in xel.Elements())
             {
                 bool found = false;
                 foreach(XElement existingchild in table.Elements())
                 {
                     if ((newchild.Attribute("type") != null) && newchild.Attribute("type").Value.Contains(existingchild.Attribute("name").Value.ToString()))
                     {
                         //found a match, merge them
                         found = true;
                         existingchild.Attribute("name").Remove();
                         existingchild.Merge(newchild);
                         break;
                     }
                 }
                 if(found == false)
                 {
                     table.Add(newchild);
                 }
             }

             return table;

         }
    }
}
