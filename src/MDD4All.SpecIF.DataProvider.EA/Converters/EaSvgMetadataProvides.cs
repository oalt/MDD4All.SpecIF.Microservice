using MDD4All.EnterpriseArchitect.SvgGenerator.Contracts;
using MDD4All.SpecIF.DataModels.DiagramInterchange;
using MDD4All.SpecIF.DataModels.DiagramInterchange.BaseElements;
using MDD4All.SpecIF.DataModels.DiagramInterchange.DiagramDefinition;
using System.Collections.Generic;
using System.Drawing;
using EAAPI = EA;

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
    public class EaSvgMetadataProvider : IMetaDataCreator
    {
        public SpecIfMetadata CreateMetaDataForDiagram(EAAPI.Diagram diagram, int height, int width)
        {
            SpecIfMetadata result = new SpecIfMetadata();

            Shape shape = new Shape
            {
                Bounds = new Bounds
                {
                    X = 0,
                    Y = 0,
                    Width = width,
                    Height = height
                },
                ResourceReference = new ResourceReference
                {
                    IdReference = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID),
                    RevisionReference = EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate),
                }
            };

            result.Shape = shape;

            return result;
        }

        public SpecIfMetadata CreateMetaDataForDiagramObject(EAAPI.DiagramObject diagramObject,
                                                     EAAPI.Element element)
        {
            SpecIfMetadata result = new SpecIfMetadata();

            Shape shape = new Shape
            {
                Bounds = new Bounds(),
                ResourceReference = new ResourceReference
                {
                    IdReference = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID),
                    RevisionReference = EaDateToRevisionConverter.ConvertDateToRevision(element.Modified)
                }
            };

            int recatangleWidth = (diagramObject.right - diagramObject.left);
            int rectangleHeight = (-diagramObject.bottom) - (-diagramObject.top);

            shape.Bounds.X = diagramObject.left;
            shape.Bounds.Y = (diagramObject.top * -1);
            shape.Bounds.Width = recatangleWidth;
            shape.Bounds.Height = rectangleHeight;

            result.Shape = shape;

            return result;
        }

        public SpecIfMetadata CreateMetaDataForDiagramLink(EAAPI.DiagramLink diagramLink,
                                                   EAAPI.Connector connector,
                                                   EAAPI.DiagramObject sourceDiagramObject,
                                                   EAAPI.DiagramObject targetDiagramObject,
                                                   EAAPI.Element sourceElement,
                                                   EAAPI.Element targetElement)

        {
            SpecIfMetadata result = new SpecIfMetadata();

            EAAPI.Element srcElement = sourceElement;
            EAAPI.Element trgtElement = targetElement;

            string directionAttribute = null; // unspecified

            if(connector.Direction == "Source -> Destination")
            {
                directionAttribute = "unidirectional";
            }
            else if(connector.Direction == "Destination -> Source")
            {
                srcElement = targetElement;
                trgtElement = sourceElement;
                directionAttribute = "unidirectional";
            }
            else if(connector.Direction == "Bi-Directional")
            {
                directionAttribute = "bidirectional";
            }

            Edge edge = new Edge
            {
                Waypoints = new List<Waypoint>(),
                References = new List<SpecIfReferenceBase>(),
                SourceResourceReference = new ResourceReference
                {
                    IdReference = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(srcElement.ElementGUID),
                    RevisionReference = EaDateToRevisionConverter.ConvertDateToRevision(srcElement.Modified)
                },
                TargetResourceReference = new ResourceReference
                {
                    IdReference = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(trgtElement.ElementGUID),
                    RevisionReference = EaDateToRevisionConverter.ConvertDateToRevision(trgtElement.Modified)
                },
                Direction = directionAttribute

            };

            edge.Waypoints = CalculateWaypointsForDiagramLink(diagramLink, sourceDiagramObject, targetDiagramObject);

            result.Edge = edge;

            return result;
        }

        private List<Waypoint> CalculateWaypointsForDiagramLink(EAAPI.DiagramLink diagramLink,
                                                                EAAPI.DiagramObject sourceDiagramObject,
                                                                EAAPI.DiagramObject targetDiagramObject)
        {
            List<Waypoint> result = new List<Waypoint>();

            int startX;
            int endX;
            int startY;
            int endY;

            List<Point> linkPathPoints = ParseEaLinkPath(diagramLink.Path);

            // horizontal coordinates calculation
            if (linkPathPoints.Count == 0) // direct line
            {
                // horizontal calculation
                // source --> target
                if (sourceDiagramObject.right < targetDiagramObject.left)
                {
                    startX = sourceDiagramObject.right;
                    endX = targetDiagramObject.left;
                }
                else if (sourceDiagramObject.left > targetDiagramObject.right) // target <-- source
                {
                    startX = sourceDiagramObject.left;
                    endX = targetDiagramObject.right;
                }
                else
                {
                    startX = sourceDiagramObject.left + (sourceDiagramObject.right - sourceDiagramObject.left) / 2;
                    endX = startX;
                }
            }
            else // bended line
            {

                // start of path

                // horizontal calculation
                // source --> target
                if (sourceDiagramObject.right < linkPathPoints[0].X)
                {
                    startX = sourceDiagramObject.right;
                }
                else if (sourceDiagramObject.left > linkPathPoints[0].X) // target <-- source
                {
                    startX = sourceDiagramObject.left;
                }
                else
                {
                    startX = linkPathPoints[0].X;
                }

                // end of path

                // horizontal calculation
                // target <-- last path
                if (targetDiagramObject.right < linkPathPoints[linkPathPoints.Count - 1].X)
                {
                    endX = targetDiagramObject.right;
                }
                else if (targetDiagramObject.left > linkPathPoints[linkPathPoints.Count - 1].X) // target --> last path
                {
                    endX = targetDiagramObject.left;
                }
                else
                {

                    endX = linkPathPoints[linkPathPoints.Count - 1].X;
                }
            }

            // vertical coordinates calculation
            if (linkPathPoints.Count == 0)
            {
                // vertical calculation
                // source above target, vertical
                if (-sourceDiagramObject.bottom < -targetDiagramObject.top)
                {
                    startY = -sourceDiagramObject.bottom;
                    endY = -targetDiagramObject.top;
                }
                // source below target, vertival
                else if (-sourceDiagramObject.top > -targetDiagramObject.bottom)
                {
                    startY = -sourceDiagramObject.top;
                    endY = -targetDiagramObject.bottom;
                }
                else
                {
                    startY = -sourceDiagramObject.top + (-sourceDiagramObject.bottom + sourceDiagramObject.top) / 2;
                    endY = startY;
                }
            }
            else
            {
                // source above target, vertical
                if (-sourceDiagramObject.bottom < linkPathPoints[0].Y)
                {
                    startY = -sourceDiagramObject.bottom;

                }
                // source below target, vertival
                else if (-sourceDiagramObject.top > linkPathPoints[0].Y)
                {
                    startY = -sourceDiagramObject.top;

                }
                else
                {
                    startY = linkPathPoints[0].Y; // -sourceDiagramObject.top + (-sourceDiagramObject.bottom + sourceDiagramObject.top) / 2;

                }

                // source above target, vertical
                if (-targetDiagramObject.top > linkPathPoints[linkPathPoints.Count - 1].Y)
                {
                    endY = -targetDiagramObject.top;

                }
                // source below target, vertival
                else if (-targetDiagramObject.bottom < linkPathPoints[linkPathPoints.Count - 1].Y)
                {
                    endY = -targetDiagramObject.bottom;

                }
                else
                {
                    endY = linkPathPoints[linkPathPoints.Count - 1].Y; //-targetDiagramObject.top + (-targetDiagramObject.bottom + targetDiagramObject.top) / 2;

                }
            }

            result.Add(new Waypoint
            {
                X = startX,
                Y = startY
            });

            foreach (Point bendPoint in linkPathPoints)
            {
                result.Add(new Waypoint
                {
                    X = bendPoint.X,
                    Y = bendPoint.Y
                });
            }

            result.Add(new Waypoint
            {
                X = endX,
                Y = endY
            });


            return result;
        }


        private List<Point> ParseEaLinkPath(string path)
        {
            List<Point> result = new List<Point>();

            char[] elementSeparator = { ';' };

            char[] xySeparator = { ':' };

            string[] pathElements = path.Split(elementSeparator);

            foreach (string pointText in pathElements)
            {
                if (!string.IsNullOrWhiteSpace(pointText))
                {
                    string[] pointSplitted = pointText.Split(xySeparator);

                    Point p = new Point(int.Parse(pointSplitted[0]), -int.Parse(pointSplitted[1]));

                    result.Add(p);
                }
            }

            return result;
        }

        
    }
}
