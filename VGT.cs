using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Windows.Forms;



namespace Map
{
    class Program
    {
        static void Main(string[] args)
        {
            double minLat = 0;
            double maxLat = 0;
            double minLon = 0;
            double maxLon = 0;
            string roadType = "";
            string osmFile = args[0];
            int numOfCars = Convert.ToInt32(args[1]);
            List<node> allNodes = new List<node>();
            List<point> tempPoints = new List<point>();
            List<link> allLinks = new List<link>();
            List<string> wayNodes = new List<string>();
            List<Car> allCars = new List<Car>();
            int carID = 1;
            Random rnd = new Random();

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(osmFile);
            XmlNode bounds = xDoc.SelectSingleNode("//osm/bounds");
            minLat = Convert.ToDouble(bounds.Attributes["minlat"].Value);
            maxLat = Convert.ToDouble(bounds.Attributes["maxlat"].Value);
            minLon = Convert.ToDouble(bounds.Attributes["minlon"].Value);
            maxLon = Convert.ToDouble(bounds.Attributes["maxlon"].Value);
            StreamWriter outFile = new StreamWriter("trace.txt");
            int maxWidth =Convert.ToInt32(distance(minLat, minLon, minLat, maxLon));
            int maxHeight =Convert.ToInt32(distance(minLat, minLon, maxLat, minLon));
            XmlNodeList allNodeTags = xDoc.SelectNodes("//osm/node");
            XmlNodeList wayNodeTags = xDoc.SelectNodes("//osm/way");
            XmlNodeList child;
            foreach (XmlNode xNode in allNodeTags)
            {
                allNodes.Add(new node(xNode.Attributes["id"].Value, distance(minLat, minLon, minLat, Convert.ToDouble(xNode.Attributes["lon"].Value)), distance(minLat, minLon, Convert.ToDouble(xNode.Attributes["lat"].Value), minLon)));
            }
            foreach(XmlNode n in wayNodeTags)
            {
                child = n.SelectNodes(".//nd");
                foreach(XmlNode a in child)
                {
                    wayNodes.Add(a.Attributes["ref"].Value);
                }
                
                child = n.SelectNodes(".//tag");
                foreach(XmlNode b in child)
                {
                    if (b.Attributes["k"].Value=="highway"&&b.Attributes["v"].Value!="footway"&&b.Attributes["v"].Value!="track"&&b.Attributes["v"].Value!="cycleway"&&b.Attributes["v"].Value!="pedestrian" && b.Attributes["v"].Value != "rest_area")
                    {
                        roadType = b.Attributes["v"].Value;
                        foreach(string str in wayNodes)
                        {
                            for (int i = 0; i < allNodes.Count; i++)
                            {
                                if (allNodes[i].id == str)
                                {
                                    tempPoints.Add(new point(allNodes[i].x, allNodes[i].y));
                                }
                            }
                        }
                        for (int k = 0; k < tempPoints.Count - 1; k++)
                        {
                            allLinks.Add(new link(tempPoints[k].x, tempPoints[k].y, tempPoints[k + 1].x, tempPoints[k + 1].y, roadType));
                        }
                    }
                }
                
                    
                tempPoints.Clear();
                wayNodes.Clear();
            } 
            for(int i=0; i<numOfCars; i++)
            {
                int index = rnd.Next(allLinks.Count);
                allCars.Add(new Car(allLinks[index].x1, allLinks[index].y1, index, carID));
                carID++;
            }

            while (allCars[0].time<=2000)
            {
                foreach (Car c in allCars)
                {
                    if (c.time <= 2000)
                    {
                        outFile.Write(c.time.ToString("0000.00") + ", " + c.id + ", " + c.xStart + ", " + c.yStart + ", ");
                        if (c.xStart == allLinks[c.linkIndex].x1)
                            c.moveCar(allLinks[c.linkIndex].x2, allLinks[c.linkIndex].y2, allLinks[c.linkIndex].velocity);
                        else if (c.xStart == allLinks[c.linkIndex].x2)
                            c.moveCar(allLinks[c.linkIndex].x1, allLinks[c.linkIndex].y1, allLinks[c.linkIndex].velocity);
                        for (int i = 0; i < allLinks.Count; i++)
                        {
                            if (c.xEnd == allLinks[i].x1 && c.yEnd == allLinks[i].y1)
                            {
                                c.linkIndex = i;
                                
                            }
                            else if(c.xEnd == allLinks[i].x2 && c.yEnd == allLinks[i].y2)
                            {
                                c.linkIndex = i;
                            }
                            
                        }
                        outFile.Write(c.xEnd + ", " + c.yEnd + ", " + c.duration.ToString("0.00")+ "\n");
                        c.xStart = c.xEnd;
                        c.yStart = c.yEnd;
                        if (c.xStart == allLinks[c.linkIndex].x1)
                        {
                            c.xEnd = allLinks[c.linkIndex].x2;
                            c.yEnd = allLinks[c.linkIndex].y2;
                        }
                        else if (c.xStart == allLinks[c.linkIndex].x2)
                        {
                            c.xEnd = allLinks[c.linkIndex].x1;
                            c.yEnd = allLinks[c.linkIndex].y1;
                        }
                    }
                }
            }
            using (Bitmap b = new Bitmap(maxWidth, maxHeight))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.Clear(Color.White);
                    foreach (link l in allLinks)
                    {
                        g.DrawLine(Pens.Red,Convert.ToSingle(l.x1), Convert.ToSingle(l.y1), Convert.ToSingle(l.x2), Convert.ToSingle(l.y2));
                    }
                }
                b.Save("map.png");
            }
            Console.WriteLine("Dimensions are: " + maxWidth + " X " + maxHeight);
            
        }
        struct link
        {
            private string roadType;
            public double x1, y1, x2, y2, velocity;
            public link(double x1, double y1, double x2, double y2, string roadType)
            {
                this.roadType = roadType;
                this.x1 = x1;
                this.x2 = x2;
                this.y1 = y1;
                this.y2 = y2;
                switch (roadType)
                {
                    case "residential":
                        velocity = 11.176;
                        break;
                    case "service":
                        velocity = 15.6464;
                        break;
                    case "trunk_link":
                        velocity = 11.176;
                        break;
                    case "tertiary":
                        velocity = 20.1168;
                        break;
                    case "motorway_link":
                        velocity = 15.6464;
                        break;
                    case "trunk":
                        velocity = 24.5872;
                        break;
                    case "primary":
                        velocity = 20.1168;
                        break;
                    case "motorway":
                        velocity = 29.0576;
                        break;
                    default:
                        velocity = 15.6464;
                        break;

                }
            }
        }
        struct point
        {
            public double x;
            public double y;
            public point(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

        }
        public static double distance(double lat1, double lon1, double lat2, double lon2)
        {
            double dist = 0;
            double rad = 6371000;
            double a = (Math.PI * lat1) / 180.0;
            double b = (Math.PI * lat2) / 180.0;
            double c = ((lat2 - lat1) * Math.PI) / 180.0;
            double d = ((lon2 - lon1) * Math.PI) / 180.0;
            double e = (Math.Sin(c / 2) * Math.Sin(c / 2)) + (Math.Cos(a) * Math.Cos(b) * Math.Sin(d / 2) * Math.Sin(d / 2));
            double f = 2 * Math.Atan2(Math.Sqrt(e), Math.Sqrt(1 - e));
            dist = rad * f;
            return dist;
        }
        public class Car
        {
            public double time, duration, xStart, yStart, xEnd, yEnd, velocity;
            public int id, linkIndex;
            public Car (double xStart, double yStart, int linkIndex, int id)
            {
                this.time = 0.0;
                this.xStart = xStart;
                this.yStart = yStart;
                this.linkIndex = linkIndex;
                this.id = id;
            }
            public void moveCar(double xEnd, double yEnd, double velocity)
            {
                this.xEnd = xEnd;
                this.yEnd = yEnd;
                this.velocity = velocity;
                duration = distance2Points(this.xStart, this.yStart, xEnd, yEnd) / velocity;
                time = time + duration;
                
            }
            public double distance2Points (double x1, double y1, double x2, double y2)
            {
                return Math.Sqrt(((x2 - x1) *(x2- x1)) + ((y2 - y1) *(y2- y1)));
            }
        }
        public struct node
        {
            public string id;
            public double x;
            public double y;
            public node(string id, double x, double y)
            {
                this.id = id;
                this.x = x;
                this.y = y;
            }
        }
    }


        
}

