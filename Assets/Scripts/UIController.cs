using System.Collections;
using UnityEngine;

public class UIController : MonoBehaviour {
    MeshRenderer renderer;
    
    public Texture[] albedo = new Texture[4];
    public Texture[] normal = new Texture[4];

    int currentTexture;
    bool normalStatus;

    void Awake() {
        renderer = GetComponent<MeshRenderer>();
        renderer.material.EnableKeyword("_NORMAL_MAP");
        renderer.material.EnableKeyword("_DIFFUSE");
        renderer.material.EnableKeyword("_SPECULAR");

        // setting default blank texture
        albedo[0] = Texture2D.whiteTexture;

        renderer.material.SetTexture("_MainTex", Texture2D.whiteTexture);
        renderer.material.SetTexture("_NormalMap", Texture2D.normalTexture);
        currentTexture = 0;
    }

    // all lighting settings

    public void toggleDiffuse(bool status) {
        if(status) {
            renderer.material.EnableKeyword("_DIFFUSE");
        } else {
            renderer.material.DisableKeyword("_DIFFUSE");
        }
    }

    public void toggleSpecular(bool status) {
        if(status) {
            renderer.material.EnableKeyword("_SPECULAR");
        } else {
            renderer.material.DisableKeyword("_SPECULAR");
        }
    }

    // all material settings

    // uses slider to set metallic value
    public void setMetalness(float metalness) {
        renderer.material.SetFloat("_Metallic", metalness);
    }

    public void setSmoothness(float smoothness) {
        renderer.material.SetFloat("_Smoothness", smoothness);
    }

    public void toggleNormals(bool status) {
        if(status) {
            switch(currentTexture){
                case 0:
                    renderer.material.SetTexture("_NormalMap", normal[0]);
                    break;
                case 1:
                    renderer.material.SetTexture("_NormalMap", normal[1]);
                    break;
                case 2:
                    renderer.material.SetTexture("_NormalMap", normal[2]);
                    break;
                case 3:
                    renderer.material.SetTexture("_NormalMap", normal[3]);
                    break;
            }
        } else {
            renderer.material.SetTexture("_NormalMap", Texture2D.normalTexture);
        }
        normalStatus = status;
    }

    // sets texture
    public void changeTexture(int texture){
        currentTexture = texture;
        switch(currentTexture){
            case 0:
                renderer.material.SetTexture("_MainTex", albedo[0]);
                break;
            case 1:
                renderer.material.SetTexture("_MainTex", albedo[1]);
                break;
            case 2:
                renderer.material.SetTexture("_MainTex", albedo[2]);
                break;
            case 3:
                renderer.material.SetTexture("_MainTex", albedo[3]);
                break;
        }
        // ensure correct normal is enabled
        toggleNormals(normalStatus);
    }
}
