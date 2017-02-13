using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HexagonCoordinates))]
public class HexagonCoordinateDrawer : PropertyDrawer
{

    public override void OnGUI(
        Rect position, SerializedProperty property, GUIContent label
    )
    {
        HexagonCoordinates coordinates = new HexagonCoordinates(
            property.FindPropertyRelative("x").intValue,
            property.FindPropertyRelative("z").intValue
        );

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coordinates.ToString());
    }
}
