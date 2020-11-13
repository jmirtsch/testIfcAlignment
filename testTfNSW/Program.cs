using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using GeometryGym.Ifc;

namespace testTfNSW
{
	class Program
	{
		static void Main(string[] args)
		{
			DatabaseIfc db = new DatabaseIfc(ReleaseVersion.IFC4X3_RC2);
			db.Factory.Options.AngleUnitsInRadians = false;
			db.Factory.Options.GenerateOwnerHistory = false;
			IfcRailway railway = new IfcRailway(db) { Name = "Default Railway", GlobalId = "2Iw5TO8gL8cQQMnPMEy58o" };
			IfcProject project = new IfcProject(railway, "Default Project", IfcUnitAssignment.Length.Metre) { GlobalId = "2VgFtp_1zCO98zJG_O3kln" };
			db.Factory.Options.GenerateOwnerHistory = true;
			project.OwnerHistory = db.Factory.OwnerHistoryAdded;
			db.Factory.Options.GenerateOwnerHistory = false;

			IfcGeometricRepresentationSubContext axisSubContext = db.Factory.SubContext(IfcGeometricRepresentationSubContext.SubContextIdentifier.Axis);

			IfcAlignment alignment = new IfcAlignment(railway) { Name = "TfNSW Alignment", GlobalId = "1F78QPlVv6N9AnnF$LSkp$" };
			IfcCartesianPoint alignmentOrigin = new IfcCartesianPoint(db, 0, 0, 0);
			//IfcCartesianPoint alignmentOrigin = new IfcCartesianPoint(db, 1000, 2000, 3000);
			alignment.ObjectPlacement = new IfcLocalPlacement(new IfcAxis2Placement3D(alignmentOrigin));

			double lineLength = 100;
			IfcCartesianPoint point1 = new IfcCartesianPoint(db, -lineLength, 0);
			IfcCartesianPoint point2 = new IfcCartesianPoint(db, 0, 0);

			double xc = 99.89734;
			double radius = 500;
			double transitionLength = 100;

			double d = -0.75 * Math.Sqrt(3) * xc / radius;
			double ang1 = (Math.Acos(-0.75 * Math.Sqrt(3) * xc / radius) * 180 / Math.PI / 3) + 240;
			double thiRadians = Math.Asin(2 / Math.Sqrt(3) * Math.Cos(ang1 *  Math.PI / 180));
			double thi = thiRadians * 180 / Math.PI;
			
			double m = Math.Tan(thiRadians) / (3 * xc * xc);
			double yc = m * Math.Pow(xc, 3);

			IfcCartesianPoint point3 = new IfcCartesianPoint(db, xc, yc);

			double cx = Math.Sin(thiRadians) * radius;
			double cy = Math.Cos(thiRadians) * radius;
			IfcCartesianPoint arcCentre = new IfcCartesianPoint(db, xc - cx, yc + cy);

			double arcLength = 100;
			double arcAngle = arcLength / radius * 180 / Math.PI;
			
			List<IfcSegment> compositeSegments = new List<IfcSegment>();

			IfcLine xLine = new IfcLine(db.Factory.Origin2d, new IfcVector(new IfcDirection(db, 1, 0), 1));
			
			IfcAlignmentHorizontalSegment linearSegment = new IfcAlignmentHorizontalSegment(point1, 0, 0, 0, lineLength, IfcAlignmentHorizontalSegmentTypeEnum.LINE);
			IfcLine line = new IfcLine(point1, new IfcVector(db.Factory.XAxis, 1));
			IfcTrimmedCurve trimmedCurve = new IfcTrimmedCurve(line, new IfcTrimmingSelect(0, point1), new IfcTrimmingSelect(lineLength, point2), true, IfcTrimmingPreference.PARAMETER);
			compositeSegments.Add(new IfcCompositeCurveSegment(IfcTransitionCode.CONTINUOUS, true, trimmedCurve));

			List<double> coefficientsX = new List<double>() { 0, 1, 0, 0, 0, -0.9 * m * m, 0, 0, 0, 5.175 * Math.Pow(m,4), 0, 0, 0, -43.1948 * Math.Pow(m,6), 0, 0, 0, 426.0564 * Math.Pow(m,8)};
			List<double> coefficientsY = new List<double>() { 0, 0, 0, m, 0, 0, 0, -2.7 * Math.Pow(m,3), 0, 0, 0, 17.955 * Math.Pow(m,5), 0, 0, 0, -158.258 * Math.Pow(m,7), 0, 0, 0, 1604.338 * Math.Pow(m,9)};
			IfcSeriesParameterCurve seriesParameterCurve = new IfcSeriesParameterCurve(db.Factory.Origin2dPlace, coefficientsX, coefficientsY);

			IfcAlignmentHorizontalSegment transitionSegment = new IfcAlignmentHorizontalSegment(point2, 0, 0, radius, transitionLength, IfcAlignmentHorizontalSegmentTypeEnum.CUBICSPIRAL);// { ObjectType = "TfNSW" };
			trimmedCurve = new IfcTrimmedCurve(seriesParameterCurve, new IfcTrimmingSelect(0, point2), new IfcTrimmingSelect(transitionLength, point3), true, IfcTrimmingPreference.PARAMETER);
			compositeSegments.Add(new IfcCompositeCurveSegment(IfcTransitionCode.CONTSAMEGRADIENTSAMECURVATURE, true, trimmedCurve));

			IfcAxis2Placement2D circlePlacement = new IfcAxis2Placement2D(arcCentre) { RefDirection = new IfcDirection(db, Math.Sin(thiRadians), -Math.Cos(thiRadians)) };
			IfcCircle circle = new IfcCircle(circlePlacement, radius);
			trimmedCurve = new IfcTrimmedCurve(circle, new IfcTrimmingSelect(0, point3), new IfcTrimmingSelect(arcLength / radius * 180 / Math.PI), true, IfcTrimmingPreference.PARAMETER);
		
			IfcAlignmentHorizontalSegment arcSegment = new IfcAlignmentHorizontalSegment(point3, thi, radius, radius, arcLength, IfcAlignmentHorizontalSegmentTypeEnum.CIRCULARARC);
			compositeSegments.Add(new IfcCompositeCurveSegment(IfcTransitionCode.CONTSAMEGRADIENTSAMECURVATURE,true, trimmedCurve));

			IfcCompositeCurve alignmentCurve = new IfcCompositeCurve(compositeSegments);
			alignment.Axis = alignmentCurve;

			double startDist = -123;
			IfcAlignmentHorizontal alignmentHorizontal = new IfcAlignmentHorizontal(alignment, linearSegment, transitionSegment, arcSegment)
			{
				StartDistAlong = startDist,
			};
			alignmentHorizontal.GlobalId = "0sEEGBFgr289x9s$R$T7N9";
			alignmentHorizontal.ObjectPlacement = alignment.ObjectPlacement;
			alignmentHorizontal.Representation = new IfcProductDefinitionShape(new IfcShapeRepresentation(axisSubContext, alignmentCurve, ShapeRepresentationType.Curve2D));

			IfcAlignmentSegment alignmentSegment = new IfcAlignmentSegment(alignmentHorizontal, transitionSegment);
			alignmentSegment.GlobalId = "2_crD$eoPDah_QvgsPhXF3";
			alignmentSegment.ObjectType = "TFNSW";
			new IfcPropertySet(alignmentSegment, "TfNSW_Transition", new IfcPropertySingleValue(db, "m", m));

			IfcPointByDistanceExpression verticalDistanceExpression = new IfcPointByDistanceExpression(0, alignmentCurve);
			double startHeight = 25;
			verticalDistanceExpression.OffsetVertical = startHeight;
			IfcAlignmentVerticalSegment verticalSegment = new IfcAlignmentVerticalSegment(db, startDist, lineLength + transitionLength + arcLength, startHeight, 0.01, IfcAlignmentVerticalSegmentTypeEnum.CONSTANTGRADIENT);
			
			IfcAlignmentVertical alignmentVertical = new IfcAlignmentVertical(alignment, verticalSegment);
			alignmentVertical.GlobalId = "2YR0TUxTv75RC2XxZVmlj8";

			//IfcLinearAxis2Placement verticalAxisPlacement = new IfcLinearAxis2Placement(verticalDistanceExpression);
			//IfcLinearPlacement verticalLinearPlacement = new IfcLinearPlacement(alignmentPlacement, verticalAxisPlacement);
			//alignmentVertical.ObjectPlacement = verticalLinearPlacement;

			//List<IfcCurveSegment> verticalSegments = new List<IfcCurveSegment>();
			//IfcLine linearGradient = new IfcLine(db.Factory.Origin, new IfcVector(new IfcDirection(db, 1, 0, 0.01), 1)); //not right and should it be xy or xz
			//IfcCurveSegment verticalCurveSegment = new IfcCurveSegment(IfcTransitionCode.CONTINUOUS, new IfcAxis2Placement3D(db.Factory.Origin2d), 50, linearGradient);
			//verticalSegments.Add(verticalCurveSegment);
			//IfcAlignmentVerticalSegment verticalSegment = new IfcAlignmentVerticalSegment(alignmentVertical,
			//	verticalLinearPlacement, verticalCurveSegment, 0, 50, startHeight, 0.01, 
			//	IfcAlignmentVerticalSegmentTypeEnum.LINE);

			//IfcGradientCurve gradientCurve = new IfcGradientCurve(horizontalCurve, verticalSegments);
			//alignment.Representation = new IfcProductDefinitionShape(new IfcShapeRepresentation(axisSubContext, gradientCurve, ShapeRepresentationType.Curve3D));
			IfcPointByDistanceExpression distanceExpression = new IfcPointByDistanceExpression(150, alignmentCurve);
			distanceExpression.OffsetLateral = 5;
			IfcAxis2PlacementLinear axis2PlacementLinear = new IfcAxis2PlacementLinear(distanceExpression);
			IfcLinearPlacement linearPlacement = new IfcLinearPlacement(alignment.ObjectPlacement , axis2PlacementLinear);
			IfcExtrudedAreaSolid extrudedAreaSolid = new IfcExtrudedAreaSolid(new IfcRectangleProfileDef(db, "", 0.5, 0.5), 5);
			IfcProductDefinitionShape productDefinitionShape = new IfcProductDefinitionShape(new IfcShapeRepresentation(extrudedAreaSolid));
			IfcPile pile = new IfcPile(railway, linearPlacement, productDefinitionShape);

			new IfcRelPositions(db, alignment, new List<IfcProduct>() { pile });

			// Conceptual 50m span Bridge from chainage -80
			distanceExpression = new IfcPointByDistanceExpression(-80 - startDist, alignmentCurve);
			axis2PlacementLinear = new IfcAxis2PlacementLinear(distanceExpression);
			IfcCurveSegment curveSegment = new IfcCurveSegment(IfcTransitionCode.CONTINUOUS, axis2PlacementLinear, 50, alignmentCurve);

			productDefinitionShape = new IfcProductDefinitionShape(new IfcShapeRepresentation(axisSubContext, curveSegment, ShapeRepresentationType.Curve2D));
			IfcBridge bridge = new IfcBridge(railway, alignment.ObjectPlacement, productDefinitionShape);
	
			db.WriteFile(args[0]);

		}
	}
}
