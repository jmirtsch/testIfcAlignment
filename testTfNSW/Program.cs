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
			//IfcCartesianPoint alignmentOrigin = new IfcCartesianPoint(db, 1000, 2000, 3000);
			IfcCartesianPoint alignmentOrigin = new IfcCartesianPoint(db, 0, 0, 0);
			IfcObjectPlacement alignmentPlacement = alignment.ObjectPlacement = new IfcLocalPlacement(new IfcAxis2Placement3D(alignmentOrigin));

			IfcAlignmentHorizontal alignmentHorizontal = new IfcAlignmentHorizontal(alignment);
			alignmentHorizontal.GlobalId = "0sEEGBFgr289x9s$R$T7N9";
			alignmentHorizontal.ObjectPlacement = new IfcLocalPlacement(alignmentPlacement, db.Factory.XYPlanePlacement);

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
			IfcCartesianPoint arcCentre = new IfcCartesianPoint(db, cx, cy);

			double arcLength = 100;
			double arcAngle = arcLength / radius * 180 / Math.PI;
			
			List<IfcSegment> compositeSegments = new List<IfcSegment>();

			IfcLine xLine = new IfcLine(db.Factory.Origin2d, new IfcVector(new IfcDirection(db, 1, 0), 1));
			
			IfcCurveSegment curveSegment = new IfcCurveSegment(IfcTransitionCode.CONTINUOUS, db.Factory.Origin2dPlace, lineLength, xLine);
			
			IfcAlignmentHorizontalSegment linearSegment = new IfcAlignmentHorizontalSegment(alignmentHorizontal, curveSegment, 0, 0, lineLength, IfcAlignmentHorizontalSegmentTypeEnum.LINE) { PredefinedType = IfcAlignmentHorizontalSegmentTypeEnum.LINE };
			IfcAxis2Placement2D axis2Placement2D = new IfcAxis2Placement2D(point1);
			compositeSegments.Add(new IfcCurveSegment(IfcTransitionCode.CONTINUOUS, axis2Placement2D, lineLength, xLine));
			linearSegment.ObjectPlacement = new IfcLocalPlacement(alignmentHorizontal.ObjectPlacement, axis2Placement2D);
			new IfcPropertySet(linearSegment, "PSet_AlignmentHorizontalSegmentCommon",
				new IfcPropertySingleValue(db, "StartDirection", new IfcPlaneAngleMeasure(0))
				); 

			List<double> coefficientsX = new List<double>() { 0, 1, 0, 0, 0, -0.9 * m * m, 0, 0, 0, 5.175 * Math.Pow(m,4), 0, 0, 0, -43.1948 * Math.Pow(m,6), 0, 0, 0, 426.0564 * Math.Pow(m,8)};
			List<double> coefficientsY = new List<double>() { 0, 0, 0, m, 0, 0, 0, -2.7 * Math.Pow(m,3), 0, 0, 0, 17.955 * Math.Pow(m,5), 0, 0, 0, -158.258 * Math.Pow(m,7), 0, 0, 0, 1604.338 * Math.Pow(m,9)};
			IfcSeriesParameterCurve seriesParameterCurve = new IfcSeriesParameterCurve(db.Factory.Origin2dPlace, coefficientsX, coefficientsY);
			curveSegment = new IfcCurveSegment(IfcTransitionCode.CONTSAMEGRADIENTSAMECURVATURE, db.Factory.Origin2dPlace, transitionLength, seriesParameterCurve);

			IfcAlignmentHorizontalSegment transitionSegment = new IfcAlignmentHorizontalSegment(alignmentHorizontal, curveSegment, 0, radius, transitionLength, IfcAlignmentHorizontalSegmentTypeEnum.NONLINEAR) { ObjectType = "TfNSW" };
			compositeSegments.Add(new IfcCurveSegment(IfcTransitionCode.CONTSAMEGRADIENTSAMECURVATURE, db.Factory.Origin2dPlace, transitionLength, seriesParameterCurve));
			transitionSegment.ObjectPlacement = new IfcLocalPlacement(alignmentHorizontal.ObjectPlacement, db.Factory.XYPlanePlacement);
			new IfcPropertySet(transitionSegment, "PSet_AlignmentHorizontalSegmentCommon",
							new IfcPropertySingleValue(db, "StartDirection", new IfcPlaneAngleMeasure(0)),
							new IfcPropertySingleValue(db, "IsStartRadiusCCW", new IfcBoolean(true)),
							new IfcPropertySingleValue(db, "IsEndRadiusCCW", new IfcBoolean(true))
							);

			new IfcPropertySet(transitionSegment, "TfNSW_Transition", new IfcPropertySingleValue(db, "m", m));

			IfcAxis2Placement2D circlePlacement = new IfcAxis2Placement2D(new IfcCartesianPoint(db, 0, radius)) { RefDirection = new IfcDirection(db, 0,-1) };
			IfcCircle circle = new IfcCircle(circlePlacement, radius);
			curveSegment = new IfcCurveSegment(IfcTransitionCode.CONTSAMEGRADIENTSAMECURVATURE, db.Factory.Origin2dPlace, arcLength, circle);
		
			IfcAlignmentHorizontalSegment arcSegment = new IfcAlignmentHorizontalSegment(alignmentHorizontal, curveSegment, radius, radius, arcLength, IfcAlignmentHorizontalSegmentTypeEnum.ARC);
			axis2Placement2D = new IfcAxis2Placement2D(point3) { RefDirection = new IfcDirection(db, Math.Cos(thiRadians), Math.Sin(thiRadians)) };
			compositeSegments.Add(new IfcCurveSegment(IfcTransitionCode.CONTSAMEGRADIENTSAMECURVATURE, axis2Placement2D, arcLength, circle));
			arcSegment.ObjectPlacement = new IfcLocalPlacement(alignmentHorizontal.ObjectPlacement,axis2Placement2D);
			new IfcPropertySet(arcSegment, "PSet_AlignmentHorizontalSegmentCommon",
								new IfcPropertySingleValue(db, "StartDirection", new IfcPlaneAngleMeasure(thi)),
								new IfcPropertySingleValue(db, "IsCCW", new IfcBoolean(true))
								);

			IfcCompositeCurve horizontalCurve = new IfcCompositeCurve(compositeSegments);
			alignmentHorizontal.Representation = new IfcProductDefinitionShape(new IfcShapeRepresentation(axisSubContext, horizontalCurve, ShapeRepresentationType.Curve2D));

			IfcAlignmentVertical alignmentVertical = new IfcAlignmentVertical(alignment);
			IfcDistanceExpression verticalDistanceExpression = new IfcDistanceExpression(0, horizontalCurve);
			double startHeight = 25;
			verticalDistanceExpression.OffsetVertical = startHeight;
			IfcLinearAxis2Placement verticalAxisPlacement = new IfcLinearAxis2Placement(verticalDistanceExpression);
			IfcLinearPlacement verticalLinearPlacement = new IfcLinearPlacement(alignmentPlacement, verticalAxisPlacement);
			alignmentVertical.ObjectPlacement = verticalLinearPlacement;

			List<IfcCurveSegment> verticalSegments = new List<IfcCurveSegment>();
			IfcLine linearGradient = new IfcLine(db.Factory.Origin, new IfcVector(new IfcDirection(db, 1, 0, 0.01), 1)); //not right and should it be xy or xz
			IfcCurveSegment verticalCurveSegment = new IfcCurveSegment(IfcTransitionCode.CONTINUOUS, new IfcAxis2Placement3D(db.Factory.Origin2d), 50, linearGradient);
			verticalSegments.Add(verticalCurveSegment);
			IfcAlignmentVerticalSegment verticalSegment = new IfcAlignmentVerticalSegment(alignmentVertical,
				verticalLinearPlacement, verticalCurveSegment, 0, 50, startHeight, 0.01, 
				IfcAlignmentVerticalSegmentTypeEnum.LINE);

			IfcGradientCurve gradientCurve = new IfcGradientCurve(horizontalCurve, verticalSegments);
			alignment.Representation = new IfcProductDefinitionShape(new IfcShapeRepresentation(axisSubContext, gradientCurve, ShapeRepresentationType.Curve3D));
			IfcDistanceExpression distanceExpression = new IfcDistanceExpression(150, horizontalCurve);
			distanceExpression.OffsetLateral = 5;
			IfcLinearAxis2Placement axis2Placement = new IfcLinearAxis2Placement(distanceExpression);
			IfcLinearPlacement linearPlacement = new IfcLinearPlacement(alignmentHorizontal.ObjectPlacement, axis2Placement);
			IfcExtrudedAreaSolid extrudedAreaSolid = new IfcExtrudedAreaSolid(new IfcRectangleProfileDef(db, "", 0.5, 0.5), 5);
			IfcProductDefinitionShape productDefinitionShape = new IfcProductDefinitionShape(new IfcShapeRepresentation(extrudedAreaSolid));
			IfcPile pile = new IfcPile(railway, linearPlacement, productDefinitionShape);
	
			db.WriteFile(args[0]);

		}
	}
}
