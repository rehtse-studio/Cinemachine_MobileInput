using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DigitalRubyShared
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    class MinMaxSliderDrawer : PropertyDrawer
    {
        const string kVectorMinName = "x";
        const string kVectorMaxName = "y";
        const float kFloatFieldWidth = 30f;
        const float kSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                var tt = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true).FirstOrDefault() as TooltipAttribute;
                if (tt != null)
                {
                    label.tooltip = tt.tooltip;
                }
                float spacing = kSpacing * EditorGUIUtility.pixelsPerPoint;

                Vector2 range = property.vector2Value;
                float min = range.x;
                float max = range.y;

                MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;

                EditorGUI.PrefixLabel(position, label);

                Rect sliderPos = position;
                sliderPos.x += EditorGUIUtility.labelWidth + kFloatFieldWidth + spacing;
                sliderPos.width -= EditorGUIUtility.labelWidth + (kFloatFieldWidth + spacing) * 2.0f;

                EditorGUI.BeginChangeCheck();
                EditorGUI.MinMaxSlider(sliderPos, ref min, ref max, attr.Min, attr.Max);
                if (EditorGUI.EndChangeCheck())
                {
                    range.x = min;
                    range.y = max;
                    property.vector2Value = range;
                }

                Rect minPos = position;
                minPos.x += EditorGUIUtility.labelWidth;
                minPos.width = kFloatFieldWidth;

                EditorGUI.showMixedValue = property.FindPropertyRelative(kVectorMinName).hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                min = EditorGUI.FloatField(minPos, min);
                if (EditorGUI.EndChangeCheck())
                {
                    range.x = Mathf.Max(min, attr.Min);
                    property.vector2Value = range;
                }

                Rect maxPos = position;
                maxPos.x += maxPos.width - kFloatFieldWidth;
                maxPos.width = kFloatFieldWidth;

                EditorGUI.showMixedValue = property.FindPropertyRelative(kVectorMaxName).hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                max = EditorGUI.FloatField(maxPos, max);
                if (EditorGUI.EndChangeCheck())
                {
                    range.y = Mathf.Min(max, attr.Max);
                    property.vector2Value = range;
                }

                EditorGUI.showMixedValue = false;
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("Vector2 support only"));
            }
        }
    }
}
