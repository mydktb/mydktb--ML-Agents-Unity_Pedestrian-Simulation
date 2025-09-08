using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMap : MonoBehaviour
{
    Material mMaterial;
    MeshRenderer mMeshRenderer;
    float[] mPoints;
    int mHitCount;


    void Start()
    {
        mMeshRenderer = GetComponent<MeshRenderer>();
        mMaterial = mMeshRenderer.material;
        mPoints = new float[32 * 3]; //32 point 
    }

    private void OnCollisionStay(Collision collision)    
    {
        foreach (ContactPoint cp in collision.contacts)
        {
            //Debug.Log("Contact with object " + cp.otherCollider.gameObject.name);

            Vector3 StartOfRay = cp.point - cp.normal;
            Vector3 RayDir = cp.normal;

            Ray ray = new Ray(StartOfRay, RayDir);
            RaycastHit hit;

            bool onGround = Physics.Raycast(ray, out hit, 1f, LayerMask.GetMask("HeatMapLayer"));

            if (onGround)
            {
                //Debug.Log("Hit Object " + hit.collider.gameObject.name);
                //Debug.Log("Hit Texture coordinates = " + hit.textureCoord.x + "," + hit.textureCoord.y);
                addHitPoint(hit.textureCoord.x * 4 - 2, hit.textureCoord.y * 4 - 2);
            }
  
        }
    }

    public void addHitPoint(float xp, float yp)
    {
        mPoints[mHitCount * 3] = xp;
        mPoints[mHitCount * 3 + 1] = yp;
        mPoints[mHitCount * 3 + 2] = Random.Range(1f, 3f);

        mHitCount++;
        mHitCount %= 32;

        mMaterial.SetFloatArray("_Hits", mPoints);
        mMaterial.SetInt("_HitCount", mHitCount);
    }
}
