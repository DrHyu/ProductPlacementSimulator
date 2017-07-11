using UnityEngine;
using System.Collections.Generic;

public class ProductAesthetics : MonoBehaviour
{
    public const float BOX_STACK_X_SPACING = 0.03f;
    public const float BOX_STACK_Y_SPACING = 0.01f;
    public const float BOX_STACK_Z_SPACING = 0.05f;

    private BoxJSON box;

    public bool selected = false;
    public bool dragging = false;
    public bool collided = false;
    public bool collided_upon = false;

    private Color dragColor = new Color(1, 1, 1, 0.5f);
    private Color collidedColor = new Color(1, 0, 0, 0.5f);
    private Color collidedUponColor = new Color(0, 0, 1, 0.5f);
    private Color selectedColor = new Color(0, 1, 0, 0.5f);
    private Color originalColor = new Color(1, 0.91f, 0.62f, 0.5f);

    private Drag3D D3D;

    private bool IS_GROUP_CONTROLLER = false;

    private List<ProductAesthetics> paChilds;


    // If in group controller mode all it will do is spread the "messages" from the UI to each individual cubes
    public void InitializeAsGroupController(BoxJSON box)
    {
        this.box = box;
        this.D3D = D3D;

        IS_GROUP_CONTROLLER = true;

        paChilds = new List<ProductAesthetics>();

        // Count all the childs
        foreach (Transform child in transform)
        {
            ProductAesthetics pa = child.GetComponent<ProductAesthetics>();
            if (pa != null) { paChilds.Add(pa); }
        }
    }

    public void Initialize(BoxJSON box, Drag3D D3D)
    {
        this.box = box;
        this.D3D = D3D;


        if (D3D != null)
        {
            D3D.RegisterOnMyCollisionEnterCallback(OnMyCollisionEnter);
            D3D.RegisterOnMyCollisionExitCallback(OnMyCollisionExit);
            D3D.RegisterOnDragStartCallback(OnDragStart);
            D3D.RegisterOnDragEndCallback(OnDragEnd);
        }
        else
        {
            Debug.LogError("Null D3D in Product Aesthetics");
        }

    }

    private void Start()
    {
        Material transparent_m = Resources.Load("Materials/StandardTransparent", typeof(Material)) as Material;

        if (!IS_GROUP_CONTROLLER)
        {
            if (box.img_path != null)
            {
                // Make sure it is at the very start 
                GetComponent<Renderer>().material = Resources.Load("Materials/StandardTransparent", typeof(Material)) as Material;
                GetComponent<Renderer>().material.color = originalColor;

                GameObject imageHolder = GameObject.CreatePrimitive(PrimitiveType.Plane);

                imageHolder.transform.parent = transform;
                imageHolder.transform.localPosition = new Vector3(0, 0, 0.51f);
                imageHolder.transform.eulerAngles = new Vector3(90, 0, 0);
                imageHolder.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                imageHolder.GetComponent<MeshCollider>().enabled = false;

                Material mat = new Material(Resources.Load("Materials/PictureMaterial", typeof(Material)) as Material);
                mat.mainTexture = Resources.Load(box.img_path, typeof(Texture)) as Texture;
                imageHolder.GetComponent<MeshRenderer>().material = mat;
            }
        }
    }

    public void SetSelected(bool sel)
    {
        if (!IS_GROUP_CONTROLLER)
        {
            selected = sel;
            UpdateColor();
        }
        else
        {
            foreach(ProductAesthetics pa in paChilds)
            {
                pa.selected = sel;
                pa.UpdateColor();
            }
        }
    }

    public void OnMyCollisionEnter(bool collided_upon)
    {
        this.collided_upon = collided_upon;
        this.collided = !collided_upon;
        UpdateColor();
    }
    public void OnMyCollisionExit()
    {
        collided = false;
        collided_upon = false;
        UpdateColor();
    }
    public void OnDragStart()
    {
        dragging = true;
        UpdateColor();
    }
    public void OnDragEnd()
    {
        dragging = false;
        UpdateColor();
    }

    public void UpdateColor()
    {
        if (collided_upon)
        {
            GetComponent<Renderer>().material.color = collidedUponColor;

        }
        else if (collided)
        {
            GetComponent<Renderer>().material.color = collidedColor;
        }
        else if (dragging)
        {
            GetComponent<Renderer>().material.color = dragColor;
        }
        else if (selected)
        {
            GetComponent<Renderer>().material.color = selectedColor;
        }
        else
        {
            GetComponent<Renderer>().material.color = originalColor;
        }
    }

}
