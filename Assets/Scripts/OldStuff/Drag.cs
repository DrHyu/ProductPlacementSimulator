
using System.Collections;
using UnityEngine;

class Drag : MonoBehaviour
{
    private Color mouseOverColor = Color.blue;
    private Color originalColor = Color.yellow;
    private bool dragging = false;
    private bool touching = false;
    private float distance;

    private Vector3 startPos;

    private Vector3 last_pos;

    public Vector2[] dragLines = new Vector2[] { new Vector2(0.0f, 0.0f), new Vector2(20.0f, 20.0f), new Vector2(30.0f, 50.0f) };
    private int cPointStart = 0;
    private int cPointEnd = 1;



    private void Start()
    {
        startPos = GetComponent<Transform>().position;
    }

    void OnMouseEnter()
    {
        GetComponent<Renderer>().material.color = mouseOverColor;
    }

    void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = originalColor;
    }

    void OnMouseDown()
    {
        last_pos = GetComponent<Transform>().position;
        distance = Vector3.Distance(GetComponent<Transform>().position, Camera.main.transform.position);
        dragging = true;
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    private void OnMouseDrag()
    {
        
    }

    void Update()
    {
        if (dragging)
        {

            //Calcualte the estimaded mouse position in the 3D space
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 mousePos3D = ray.GetPoint(distance);
            //We don't care about the Y pos, since we will always be moving on a plane
            mousePos3D.y = startPos.y;

            //Do some trigonometry to find out the closes point in the vector to the mouse position
            Vector2 SE = dragLines[cPointEnd] - dragLines[cPointStart];
            Vector2 CS = (new Vector2(mousePos3D.x,mousePos3D.z)) - dragLines[cPointStart];
            
            float alpha = Mathf.Acos(Vector2.Dot(SE, CS) / (SE.magnitude * CS.magnitude));
            float con = CS.magnitude * Mathf.Cos(alpha);

            Ray r = new Ray(dragLines[cPointStart], SE);

            // This is the point in the segment where we want the mouse to move
            Vector2 insc = r.GetPoint(con);


            // The code below makes sure that the obect can't move out of the ends of the segment
            // Without it, the obect could move in a (infinite) line. 
            // This bounds the object movement to the ends to the segment.
            // Also, if either ends of the segment is reached, it will move on the respective next segment in the dragLines array
            Vector2 big_x     = (dragLines[cPointEnd].x > dragLines[cPointStart].x) ? dragLines[cPointEnd] : dragLines[cPointStart];
            Vector2 small_x   = (dragLines[cPointEnd].x < dragLines[cPointStart].x) ? dragLines[cPointEnd] : dragLines[cPointStart];

            Vector2 big_y = (dragLines[cPointEnd].y > dragLines[cPointStart].y) ? dragLines[cPointEnd] : dragLines[cPointStart];
            Vector2 small_y = (dragLines[cPointEnd].y < dragLines[cPointStart].y) ? dragLines[cPointEnd] : dragLines[cPointStart];

            int big_x_incr   = (dragLines[cPointEnd].x > dragLines[cPointStart].x) ? 1 : -1;
            int small_x_incr = (dragLines[cPointEnd].x < dragLines[cPointStart].x) ? 1 : -1;

            int big_y_incr = (dragLines[cPointEnd].y > dragLines[cPointStart].y) ? 1 : -1;
            int small_y_incr = (dragLines[cPointEnd].y < dragLines[cPointStart].y) ? 1 : -1;

            if (insc.x  > big_x.x)
            {
                insc = big_x;

                if(cPointStart + big_x_incr >= 0 && cPointStart + big_x_incr < dragLines.Length-1)
                {
                    cPointStart += big_x_incr;
                    cPointEnd += big_x_incr;
                }
            }
            else if (insc.x < small_x.x)
            {
                insc = small_x;
                if (cPointStart + small_x_incr >= 0 && cPointStart + small_x_incr < dragLines.Length - 1)
                {
                    cPointStart += small_x_incr;
                    cPointEnd += small_x_incr;
                }
            }

            if (insc.y > big_y.y)
            {
                insc = big_y;

                if (cPointStart + big_y_incr >= 0 && cPointStart + big_y_incr < dragLines.Length - 1)
                {
                    cPointStart += big_y_incr;
                    cPointEnd += big_y_incr;
                }
            }
            else if (insc.y < small_y.y)
            {
                insc = small_y;
                if (cPointStart + small_y_incr >= 0 && cPointStart + small_y_incr < dragLines.Length - 1)
                {
                    cPointStart += small_y_incr;
                    cPointEnd += small_y_incr;
                }
            }

            Vector3 moveTo = new Vector3(insc.x, startPos.y, insc.y);
            GetComponent<Transform>().position = moveTo;

            last_pos = moveTo;
        }
    }

    private void OnGUI()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 mousePos3D = ray.GetPoint(distance);



        mousePos3D.y = startPos.y;


        GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        GUILayout.Label("Mouse position: " + Input.mousePosition);
        GUILayout.Label("World position: " + mousePos3D.ToString("F3"));
        GUILayout.EndArea();
    }
}