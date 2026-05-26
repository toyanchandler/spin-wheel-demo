using UnityEditor;

namespace CartoonFX
{
    public class MaterialInspector : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            materialEditor.PropertiesDefaultGUI(properties);
        }
    }
}
