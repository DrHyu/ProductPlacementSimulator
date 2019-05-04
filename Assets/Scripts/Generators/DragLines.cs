using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DragLines  
{
    public Vector3[] points;


    public void MoveInDragline(ref int c_index, ref float c_pos, Vector3 towards_p, ref float move_budget)
    {

        Vector3 currentPos = points[c_index] + (points[c_index + 1] - points[c_index]) * c_pos;

        // Direction of the dragline
        Vector3 draglineDir = (points[c_index + 1] - points[c_index]).normalized;
        // curent_point to towards_dir
        Vector3 currentpToTowardspDir = (towards_p - currentPos).normalized;
        // Angle inbetween both vectors
        float alpha = Mathf.Acos(Vector3.Dot(draglineDir, currentpToTowardspDir) / (draglineDir.magnitude * currentpToTowardspDir.magnitude));


        // This representa how well the position of the cursor fits the direction of the current line
        float towards_position_speed_factor = Mathf.Abs(Mathf.Cos(alpha));

        // This represens if we are moiving towards the start (left) or end (right) of this dragline
        int right_or_left = (Mathf.Cos(alpha)) >= 0 ? 1 : -1;

        Vector3 dir_vector = right_or_left == 1 ? draglineDir : -draglineDir;

        // If we are going to the right the distance left will be measured agains the end (c_index +1) of this dragline
        // If we are going to the left the distance left will be measured against the start (c_index) of this dragline
        float dist_left_in_current_dragline = right_or_left == 1 ? (points[c_index + 1] - currentPos).magnitude : (points[c_index] - currentPos).magnitude;


        //Debug.Log("Actual move :" + move_budget * towards_position_speed_factor / (localDragLines[c_index + 1] - localDragLines[c_index]).magnitude + " RoL: " + right_or_left +" Speed fact: " + towards_position_speed_factor + " Dist left: " + dist_left_in_current_dragline + " Dist/fact: " + dist_left_in_current_dragline / towards_position_speed_factor + " budget: " + move_budget);


        // If there isn't enough move budget to finish this dragline
        // if towards_position_speed_factor == 0 it means that the towards point is in 90 (or 270) degrees, so we shouldnt move
        if (towards_position_speed_factor == 0 || dist_left_in_current_dragline / towards_position_speed_factor > move_budget)
        {
            //c_index = c_index;
            float moved_distance = move_budget * towards_position_speed_factor;

            c_pos = c_pos + right_or_left * (moved_distance / (points[c_index + 1] - points[c_index]).magnitude);
            move_budget = 0;
        }
        // If there is
        else
        {
            if (c_index == 0 && right_or_left == -1)
            {
                //c_index = c_index;
                c_pos = 0;
                move_budget = 0;
            }
            else if (c_index == points.Length - 2 && right_or_left == 1)
            {
                //c_index == c_index
                c_pos = 1;
                move_budget = 0;
            }
            else
            {
                c_index = c_index + right_or_left;
                c_pos = right_or_left == -1 ? 1 : 0;
                move_budget = move_budget - (dist_left_in_current_dragline / towards_position_speed_factor);
            }
        }
    }

    public DragLines(Vector3[] pts, float offset, bool doBeizer = false)
    {
        List<Vector3> newDragline = new List<Vector3>();
        if (pts.Length == 2)
        {
            Vector2 o1 = pts[0].to2DwoY();
            Vector2 o2 = pts[1].to2DwoY();

            Vector2 o1o2 = o2 - o1;

            Vector2 po1o2 = o1o2.PerpClockWise();
            po1o2.Normalize();

            Vector2 n1 = o1 - po1o2 * offset;
            Vector2 n2 = o2 - po1o2 * offset;

            List<int> vertexRelation = new List<int>();

            newDragline.Add(n1.to3DwY(pts[0].y));
            vertexRelation.Add(0);
            newDragline.Add(n2.to3DwY(pts[1].y));
            vertexRelation.Add(1);

        }
        else
        {


            Vector2 o1 = pts[0].to2DwoY();
            Vector2 o2 = pts[1].to2DwoY();

            Vector2 o1o2 = o2 - o1;

            Vector2 po1o2 = o1o2.PerpClockWise();
            po1o2.Normalize();

            Vector2 n1 = o1 - po1o2 * offset;
            Vector2 n2 = o2 - po1o2 * offset;

            List<int> vertexRelation = new List<int>();

            newDragline.Add(n1.to3DwY(pts[0].y));
            vertexRelation.Add(0);

            for (int i = 1; i < pts.Length - 1; i++)
            {

                o2 = pts[i].to2DwoY();
                Vector2 o3 = pts[i + 1].to2DwoY();

                Vector2 o2o3 = o3 - o2;

                Vector2 po2o3 = o2o3.PerpClockWise();

                po2o3.Normalize();

                Vector2 n3 = o2 - po2o3 * offset;
                Vector2 n4 = o3 - po2o3 * offset;

                Vector output;
                if (MiscFunc.Intersects(n1.toV(), n2.toV(), n3.toV(), n4.toV(), out output))
                {
                    Vector2 isc = output.toV2();
                    Vector2 n5 = new Ray2D(n1, isc - n1).GetPoint((isc - n1).magnitude - (n2 - isc).magnitude);
                    Vector2 n6 = new Ray2D(n4, isc - n4).GetPoint((isc - n4).magnitude - (n3 - isc).magnitude);

                    if (doBeizer)
                    {
                        Vector2[] res = MiscFunc.DoBezier(new Vector2[] { n5, isc, n6 }, 2, 40);

                        for (int p = 0; p < res.Length; p++)
                        {
                            newDragline.Add(res[p].to3DwY(pts[i].y));
                            vertexRelation.Add(i);
                        }

                        n1 = res[res.Length - 1];    //This one is correct for sure
                        n2 = n4;    //This one might get fixed next iteration
                    }
                    else
                    {
                        newDragline.Add(isc.to3DwY(pts[i].y));
                        vertexRelation.Add(i);
                        n1 = isc;    //This one is correct for sure
                        n2 = n4;    //This one might get fixed next iteration
                    }
                }
                else
                {
                    Vector2 isc;
                    if (!MiscFunc.IntersectRay2D(n1, n2 - n1, n4, n3 - n4, out isc))
                    {
                        Debug.LogError("Did not intersect");
                    }

                    if (doBeizer)
                    {

                        Vector2[] res = MiscFunc.DoBezier(new Vector2[] { n2, isc, n3 }, 2, 20);

                        for (int p = 0; p < res.Length; p++)
                        {
                            newDragline.Add(res[p].to3DwY(pts[i].y));
                            vertexRelation.Add(i);
                        }

                        n1 = res[res.Length - 1]; //This one is correct for sure
                        n2 = n4;    //This one might get fixed next iteration
                    }
                    else
                    {
                        newDragline.Add(isc.to3DwY(pts[i].y));
                        vertexRelation.Add(i);

                        n1 = isc;    //This one is correct for sure
                        n2 = n4;    //This one might get fixed next iteration
                    }
                }


                if (i == pts.Length - 2)
                {
                    newDragline.Add(n4.to3DwY(pts[i + 1].y));
                    vertexRelation.Add(i + 1);
                }
            }
        }
        points = newDragline.ToArray();
    }

    public Vector3 CalculateMatchingPoint(int c_index, float c_pos, float p_width, bool given_is_right, ref int index, ref float pos)
    {
        // We have the position of one of the vertices of the product in the dragline, calculate the other one

        Vector3 P = (points[c_index] + (points[c_index + 1] - points[c_index]) * c_pos);

        // The loop has to go in difrent directions depending if we are searching left or right
        int from = given_is_right ? c_index + 1 : c_index;
        int to = given_is_right ? points.Length : -1;
        int incr = given_is_right ? 1 : -1;

        int r_or_l = given_is_right ? 1 : 0;
        // The second point will be contained inside the same segment
        if ((P - points[c_index + r_or_l]).magnitude > p_width)
        {
            Vector3 dir = points[c_index + 1] - points[c_index];
            Vector3 C = c_pos * dir + p_width * (dir.normalized * incr);
            index = c_index;
            pos = C.magnitude / dir.magnitude;

            return C + points[c_index];
        }
        else
        {
            int near_index = -1;
            for (int i = from; i != to; i += incr)
            {
                float dist = (points[i] - P).magnitude;

                if (dist >= p_width)
                {
                    near_index = i;
                    break;
                }
            }

            // TODO: should give an error ?
            if (near_index == -1)
            {
                if (given_is_right) { index = c_index; pos = 1; return points[c_index + 1]; }
                else { index = c_index; pos = 0; return points[index]; }
            }


            // We now know  that the point must be found inbetween close_index - 1 and close_index
            Vector2 A = (given_is_right ? points[near_index - 1] : points[near_index + 1]).to2DwoY(); ;
            Vector2 B = points[near_index].to2DwoY();
            Vector2 D;   // Closest point in the line to P
            Vector2 C1;  // Poitn we want to find
            Vector2 C2;

            MiscFunc.PointOnTopOfSegment(P.to2DwoY(), A, B, out D);

            // Cos alpha = |PD|/|PC|
            float alpha = Mathf.Acos((D - P.to2DwoY()).magnitude / p_width);

            // Sin alpha = |CD|/|PC|
            float CDm = p_width * Mathf.Sin(alpha);

            //There can be two solutions to this, C = D + AB.normalized * (+ or -) |CD|
            // Chose the one closest to A ( if it is inside the semgment)

            C1 = D + (B - A).normalized * -CDm;
            C2 = D + (B - A).normalized * +CDm;


            bool C1inSeg = MiscFunc.PointIsInSegment(A, B, C1);
            bool C2inSeg = MiscFunc.PointIsInSegment(A, B, C2);

            Vector2 C;
            if (C1inSeg && C2inSeg)
            {
                float d1 = (C1 - A).magnitude;
                float d2 = (C2 - A).magnitude;

                C = d1 < d2 ? C1 : C2;
            }
            else if (C1inSeg)
            {
                C = C1;
            }
            else
            {
                C = C2;
            }


            //float PAm = (P - A).magnitude;
            //float PBm = (P - B).magnitude;

            //if(near_index == 0)
            //{
            //    string bug = "here";
            //}

            index = given_is_right ? near_index - 1 : near_index;

            pos = given_is_right ? (A - C).magnitude / (A - B).magnitude : 1 - (A - C).magnitude / (A - B).magnitude;
            return A + pos * (B - A);
            //pos = given_is_right ? (p_width - PAm) / (PBm - PAm) : 1 - (p_width - PAm) / (PBm - PAm);

            //return A + (B - A) * (p_width - PAm) / (PBm - PAm);
        }
    }

    public Vector2 GetRightVertextPosition(BoxJSON b)
    {
        if(b.cir >= points.Length)
        {
            return points[points.Length - 1];
        }
        else
        {
            return points[b.cir] + (points[b.cir + 1] - points[b.cir]) * b.cpr;
        }
    }

    public Vector2 GetLeftVertextPosition(BoxJSON b)
    {
        return points[b.cil] + (points[b.cil + 1] - points[b.cil]) * b.cpl;
    }
}
