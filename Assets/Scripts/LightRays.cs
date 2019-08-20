
/*
    connected to the light ray
    deals with drawing incoming ray
    and then drawing diffuse and specular rays
 */

using UnityEngine;

public class LightRays : MonoBehaviour
{
    public Material incMat, diffMat, specMat;

    Vector3 lightPoint;
    Vector3 reflectionPoint;

    Vector3 normal;
    Vector3 actualNormal;
    Vector3 incoming;
    Vector3 reflection;

    float normalOffsetAngle;

    float totalWidth = 0.05f;

    GameObject[] specularLines;
    GameObject[] diffuseLines;
    // whether it is rendered or not
    bool diffReflections = true, specReflections = true;

    float metalness;
    float smoothness;

    void Start()
    {
        lightPoint = GameObject.Find("Spot Light").transform.position;

        // call function with light point and direction of light
        findReflectionPoint(lightPoint, GameObject.Find("Spot Light").transform.forward);
        
        incoming = reflectionPoint - lightPoint;
        // only needs to be recalculated after map change
        reflection = reflectionPoint + Vector3.Reflect(incoming, actualNormal);

        // draw incoming line
        var line = gameObject.AddComponent<LineRenderer>();
        line.material = incMat;
        line.positionCount = 2;
        line.startWidth = totalWidth;
        line.SetPosition(0, lightPoint);
        line.SetPosition(1, reflectionPoint);
        line.useWorldSpace = true;

        drawSpecular();
        drawDiffuse();
    }

    public void setMetalness(float metalness) {
        this.metalness = metalness;
        destroyAndRefresh();
    }

    public void setSmoothness(float smoothness) {
        this.smoothness = smoothness;
        destroyAndRefresh();
    }

    public void setWidthFromLightIntesity(float intensity) {
        totalWidth = intensity / 10f;
        gameObject.GetComponent<LineRenderer>().startWidth = totalWidth;
        destroyAndRefresh();
    }

    void findReflectionPoint(Vector3 pos, Vector3 dir) {
        Ray incRay = new Ray(pos, dir);
        RaycastHit hit;
        if(Physics.Raycast(incRay, out hit, 2.0f)) {
            // save normal vector and reflection point
            reflectionPoint = hit.point;
            actualNormal = hit.normal;

            // find normal based on texcoords
            Vector2 texCoord = hit.textureCoord;
            Texture2D normalTex = 
                (Texture2D)GameObject.Find("Sphere").GetComponent<MeshRenderer>().material.GetTexture("_NormalMap");
            normal.x = normalTex.GetPixelBilinear(texCoord.x, texCoord.y).r;
            normal.y = normalTex.GetPixelBilinear(texCoord.x, texCoord.y).g;
            normal.z = normalTex.GetPixelBilinear(texCoord.x, texCoord.y).b;
        }
    }

    public void drawSpecular() {
        // slightly higher than just metalness as there is always spec
        // yes i break the physically based part of PBR
        float specWidth = 2 * (totalWidth * metalness + totalWidth * 0.1f);
        // 20 rays at starting intensity
        int numberOfSpecRays = (int) (specWidth * 600f);

        specularLines = new GameObject[numberOfSpecRays];

        // draw specular lines with normal and smoothness offsets
        for(int i = 0; i < numberOfSpecRays; i++) {
            specularLines[i] = new GameObject("specular ray");

            // making the base line to later rotate
            var line = specularLines[i].AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.startWidth = specWidth / numberOfSpecRays;
            line.SetPosition(0, reflectionPoint);
            line.SetPosition(1, reflection);
            line.useWorldSpace = false;
            line.material = specMat;

            // factoring in smoothness, smoother should result in denser highlight
            float minTilt = -normalOffsetAngle - (float)System.Math.Pow(5 * (1 - smoothness), 2);
            float maxTilt = normalOffsetAngle + (float)System.Math.Pow(5 * (1 - smoothness), 2);
            specularLines[i].GetComponent<Transform>().Rotate(
                Random.Range(minTilt, maxTilt), 
                Random.Range(minTilt, maxTilt), 
                Random.Range(minTilt, maxTilt));
        }
    }

    public void drawDiffuse() {
        // at max metalness, diffuse doesn't exist
        float diffWidth = 2 * (totalWidth - totalWidth * metalness);
        // tbh idk how many rays this is but should be enough
        int numberOfDiffRays = (int) (diffWidth * 600f);

        diffuseLines = new GameObject[numberOfDiffRays];

        for(int i = 0; i < numberOfDiffRays; i++) {
            diffuseLines[i] = new GameObject("diffuse ray");

            // makes the base line
            var line = diffuseLines[i].AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.startColor = Color.red;
            line.startWidth = diffWidth / numberOfDiffRays;
            line.SetPosition(0, reflectionPoint);
            line.SetPosition(1, reflection);
            line.useWorldSpace = false;

            line.material = diffMat;
        }
    }

    // public so it can be called after any UI change
    public void destroyAndRefresh() {
        // offsetting with normal map
        // angle with 128 128 256 as that is base in normal map
        normalOffsetAngle = Vector3.Angle(normal, new Vector3(128, 127, 255));

        findReflectionPoint(lightPoint, GameObject.Find("Spot Light").transform.forward);

        // self explanatory tbh
        foreach(GameObject s in specularLines) {
            Destroy(s);
        }
        if(specReflections) {
            drawSpecular();
        }

        foreach(GameObject g in diffuseLines) {
            Destroy(g);
        }
        if(diffReflections) {
            drawDiffuse();
        }
    }

    public void toggleDiffuse(bool toggle) {
        diffReflections = toggle;
        destroyAndRefresh();
    }

    public void toggleSpecular(bool toggle) {
        specReflections = toggle;
        destroyAndRefresh();
    }

    public void toggleNormals() {
        reflection = reflectionPoint + Vector3.Reflect(incoming, actualNormal);
        destroyAndRefresh();
    }
}
