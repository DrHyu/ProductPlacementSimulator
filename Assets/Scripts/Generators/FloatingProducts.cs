using UnityEngine;
using System.Collections;

public class FloatingProducts : MonoBehaviour
{
    // Used to provided a temporary space for the products that have been deattached and ar looking for a new shelf

    public Drag3D floatingProduct;

    public StandGenerator floatingPordOrigStand;
    public ShelfGenerator floatingProdOrigShelf;

    private bool return_floating_prod = false;

    public bool AddFloatingProduct(Drag3D fp, ShelfGenerator shg, StandGenerator sg)
    {
        // Only 1 product at a time
        if (floatingProduct != null) { return false; }

        floatingProduct = fp;
        floatingPordOrigStand = sg;
        floatingProdOrigShelf = shg;

        // Transfer the parenthood form the shg to this 
        // Move to the same position as the shelf generator so the position of the product is not alterd
        transform.localPosition = shg.transform.localPosition;
        fp.transform.parent = this.transform;

        return true;
    }

    public void ReturnFloatingProduct()
    {
        return_floating_prod = true;
    }

    private void LateUpdate()
    {
        if (return_floating_prod)
        {
            return_floating_prod = false;
            floatingProduct.deattached = false;
            floatingProdOrigShelf.AttatchProduct(floatingProduct.this_box, floatingProduct.gameObject);
            floatingProduct.ReturnToLastValidPosition();
            
            floatingProduct = null;
        }
    }

    public bool ProductFloating()
    {
        return floatingProduct != null;
    }
}
